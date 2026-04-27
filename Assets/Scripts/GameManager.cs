using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class GameManager : NetworkBehaviour // NetworkBehaviour allows this to use networking features
{
    public static GameManager instance;

    [System.Serializable]
    public struct PlayerHymnData : INetworkSerializable
    {
        public ulong clientId;
        public List<string> hymns;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);

            // Serialize list length
            int count = hymns == null ? 0 : hymns.Count;
            serializer.SerializeValue(ref count);

            if (serializer.IsReader)
            {
                hymns = new List<string>();
                for (int i = 0; i < count; i++)
                {
                    string line = "";
                    serializer.SerializeValue(ref line);
                    hymns.Add(line);
                }
            }
            else // writer
            {
                for (int i = 0; i < count; i++)
                {
                    string line = hymns[i];
                    serializer.SerializeValue(ref line);
                }
            }
        }
    }


    public CameraMovement cameraScript;
    // Dictionary that stores hymns for each player
    // Key: ClientId (0, 1, 2, 3...)
    // Value: List of hymns that player has submitted
    public Dictionary<ulong, List<string>> playerHymns = new Dictionary<ulong, List<string>>();
    public string currentPrompt;
    // NetworkVariable automatically syncs across all clients - everyone sees the same value
    // This tracks whose turn it currently is by their ClientId

    public int maxHymnsPerRound;
    public static event Action onHymnRoundEnd;
    public static event Action onVotingRoundStart;
    public static event Action onVotingRoundEnd;

    [Header("VotingStuff")]
    public int totalVotes;
    private Dictionary<ulong, int> votesByPlayer = new Dictionary<ulong, int>();
    private HashSet<ulong> playersWhoAlreadyVoted = new HashSet<ulong>();

    public GameObject winScreen;
    public GameObject loseScreen;

    private int hymnsCount;
    private NetworkVariable<ulong> currentTurnClientId = new NetworkVariable<ulong>(0); // Starts at 0 (host goes first)

    // Track who is currently typing
    private NetworkVariable<bool> isCurrentPlayerTyping = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] GameManager spawned. Starting sync check...");
            StartCoroutine(WaitAndSync());
        }
    }

    void Awake()
    {
        // Singleton setup - make sure only one GameManager exists
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (IsServer)
        {
            PromptGenerator promptGen = FindObjectOfType<PromptGenerator>();
            if (promptGen != null)
            {
                promptGen.GeneratePromptServerRpc();
            }
        }

    }

    // Check if it's the local player's turn (the player on this computer)
    public bool IsMyTurn()
    {
        ulong localClientId = NetworkManager.Singleton.LocalClientId; // Get this computer's ClientId
        return localClientId == currentTurnClientId.Value; // Compare with whose turn it is
    }

    // Get the ClientId of whose turn it currently is
    public ulong GetCurrentTurnClientId()
    {
        return currentTurnClientId.Value;
    }

    // Add a hymn to a player's list

    [ServerRpc(RequireOwnership = false)]
    public void SubmitHymnServerRpc(ulong clientId, string hymn)
    {
        AddHymnServer(clientId, hymn);
    }
    public void AddHymnServer(ulong clientId, string hymn)
    {
        // If this player doesn't have a list yet, create one
        if (!playerHymns.ContainsKey(clientId))
            playerHymns[clientId] = new List<string>();

        // Add the hymn to their list
        playerHymns[clientId].Add(hymn);
        Debug.Log($"[SERVER] Player {clientId} submitted hymn: {hymn}");
        hymnsCount++;
        Debug.Log(hymnsCount);
        if (hymnsCount == maxHymnsPerRound)
        {
            

            //trigger event that round for adding hymns is over
            HymnRoundEndClientRpc();

            //Convert the dictonary to the new struct
            List<PlayerHymnData> dataList = new List<PlayerHymnData>();

            foreach (var entry in playerHymns)
            {
                dataList.Add(new PlayerHymnData
                {
                    clientId = entry.Key,
                    hymns = new List<string>(entry.Value)
                });
            }
            SendHymnsClientRpc(dataList.ToArray());
            VotingRoundStartClientRpc();
        }
    }

    [ClientRpc]
    private void SendHymnsClientRpc(PlayerHymnData[] hymnDataArray)
    {
        playerHymns.Clear();

        foreach (var data in hymnDataArray)
        {
            playerHymns[data.clientId] = data.hymns;
        }
    }

    [ClientRpc]
    private void HymnRoundEndClientRpc()
    {
        onHymnRoundEnd?.Invoke();
        Debug.Log("HymnRoundEnd Event fired");
    }

    [ClientRpc]
    private void VotingRoundStartClientRpc()
    {
        onVotingRoundStart?.Invoke();
        Debug.Log("VotingRoundStart Event fired");
    }

    [ClientRpc]
    private void VotingRoundEndClientRpc()
    {
        onVotingRoundEnd?.Invoke();
        Debug.Log("VotingRoundEnd Event fired");
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitVoteServerRpc(ulong votedPlayerClientId, ServerRpcParams rpcParams = default)
    {
        ulong voterClientId = rpcParams.Receive.SenderClientId;

        // Prevent double voting
        if (playersWhoAlreadyVoted.Contains(voterClientId))
        {
            Debug.Log($"Player {voterClientId} already voted.");
            return;
        }

        playersWhoAlreadyVoted.Add(voterClientId);

        // Add vote
        if (!votesByPlayer.ContainsKey(votedPlayerClientId)) 
        {
            votesByPlayer[votedPlayerClientId] = 0;
        }
            

        votesByPlayer[votedPlayerClientId]++;
        totalVotes++;

        Debug.Log($"Player {voterClientId} voted for player {votedPlayerClientId}");

        // If everyone has voted
        if (totalVotes >= NetworkManager.Singleton.ConnectedClientsIds.Count)
        {
            ResolveVotingRound();
        }
    }

    private void ResolveVotingRound()
    {
        VotingRoundEndClientRpc();

        ulong votedOutClientId = ulong.MaxValue;
        int mostVotes = -1;

        foreach (var entry in votesByPlayer)
        {
            ulong clientId = entry.Key;
            int votes = entry.Value;

            Debug.Log($"Player {clientId} received {votes} votes.");

            if (votes > mostVotes)
            {
                mostVotes = votes;
                votedOutClientId = clientId;
            }
        }

        if (votedOutClientId == ulong.MaxValue)
        {
            Debug.Log("No valid voted out player found.");
            return;
        }

        PlayerData votedOutPlayer = PlayerManager.Instance?.GetPlayer(votedOutClientId);

        if (votedOutPlayer != null)
        {
            Debug.Log($"Player voted out: {votedOutPlayer.PlayerName} ({votedOutClientId})");
            Debug.Log($"Their role was: {votedOutPlayer.Role}");

            if (votedOutPlayer.Role == "Fallen Angel")
            {
                Debug.Log("The impostor was caught!");
                ShowWinScreenClientRpc();
            }
            else
            {
                Debug.Log("Wrong vote! The impostor survives.");
                ShowLoseScreenClientRpc();
            }
        }
        else
        {
            Debug.LogWarning($"Could not find PlayerData for ClientId {votedOutClientId}");
        }
    }


    // End the current turn and move to the next player
    // [ServerRpc] means this runs on the server, but any client can call it
    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc()
    {

        if (!IsServer) return; // Safety check - only server should execute this logic

        // Get all connected players
        var allPlayers = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);

        // Find where the current turn player is in the list
        int currentIndex = allPlayers.IndexOf(currentTurnClientId.Value);

        // Move to next player (% makes it loop back to 0 when it reaches the end)
        int nextIndex = (currentIndex + 1) % allPlayers.Count;
        currentTurnClientId.Value = allPlayers[nextIndex]; // Setting this NetworkVariable syncs to all clients automatically

        if (nextIndex == 0)
        {
            cameraScript.PlayerZeroCameraPosition();

        }
        else if (nextIndex == 1)
        {
            cameraScript.PlayerOneCameraPosition();

        }
        else if (nextIndex == 2)
        {
            cameraScript.PlayerTwoCameraPosition();

        }
        else if (nextIndex == 3)
        {
            cameraScript.PlayerThreeCameraPosition();

        }

        Debug.Log($"Turn ended. Next turn: ClientId {currentTurnClientId.Value}");
    }
    [ServerRpc(RequireOwnership = false)]
    public void UpdateTypingStatusServerRpc(ulong clientId, bool isTyping)
    {
        // Only update if it's the current turn player
        if (clientId == currentTurnClientId.Value)
        {
            isCurrentPlayerTyping.Value = isTyping;
        }
    }

    // Get if the current player is typing
    public bool IsCurrentPlayerTyping()
    {
        return isCurrentPlayerTyping.Value;
    }

    // Get the name of the current turn player
    public string GetCurrentPlayerName()
    {
        ulong currentId = currentTurnClientId.Value;

        PlayerData player = PlayerManager.Instance?.GetPlayer(currentId);

        if (player != null && !string.IsNullOrEmpty(player.PlayerName))
        {
            return player.PlayerName;
        }

        return "Player " + currentId;
    }
    private IEnumerator WaitAndSync()
    {
        // Wait until PlayerManager exists and is spawned on the network
        while (PlayerManager.Instance == null || !PlayerManager.Instance.IsSpawned)
        {
            Debug.Log("Waiting for PlayerManager to spawn...");
            yield return null; // Wait for the next frame
        }

        Debug.Log("PlayerManager is ready! Requesting sync...");
        PlayerManager.Instance.RequestSyncServerRpc(NetworkManager.Singleton.LocalClientId);
    }
    [ClientRpc]
    private void ShowWinScreenClientRpc()
    {
        winScreen.SetActive(true);
    }

    [ClientRpc]
    private void ShowLoseScreenClientRpc()
    {
        loseScreen.SetActive(true);
    }
}