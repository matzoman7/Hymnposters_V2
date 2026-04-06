using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.Rendering;

public class GameManager : NetworkBehaviour // NetworkBehaviour allows this to use networking features
{
    public static GameManager instance;


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


    private int hymnsCount;
    private NetworkVariable<ulong> currentTurnClientId = new NetworkVariable<ulong>(0); // Starts at 0 (host goes first)
    

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
        if (hymnsCount == maxHymnsPerRound)
        {
            //trigger event that round for adding hymns is over
            HymnRoundEndClientRpc();
            VotingRoundStartClientRpc();
        }
    }

    [ClientRpc]
    private void HymnRoundEndClientRpc()
    {
        onHymnRoundEnd?.Invoke();
    }

    [ClientRpc]
    private void VotingRoundStartClientRpc()
    {
        onVotingRoundStart?.Invoke();
    }

    [ClientRpc]
    private void VotingRoundEndClientRpc()
    {
        onVotingRoundEnd?.Invoke();
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
                // TODO: Trigger Innocents Win
            }
            else
            {
                Debug.Log("Wrong vote! The impostor survives.");
                // TODO: Trigger Fallen Angel Win or continue round
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
    // Debug method to print all hymns in the dictionary (hook this to a button)
    public void PrintAllHymns()
    {
        Debug.Log("=== HYMN DICTIONARY DEBUG ===");
        Debug.Log($"Total players in dictionary: {playerHymns.Count}");

        if (playerHymns.Count == 0)
        {
            Debug.Log("Dictionary is EMPTY!");
            return;
        }

        foreach (KeyValuePair<ulong, List<string>> playerEntry in playerHymns)
        {
            ulong clientId = playerEntry.Key;
            List<string> hymns = playerEntry.Value;

            // Get player name if available
            PlayerData player = PlayerManager.Instance?.GetPlayer(clientId);
            string playerName = player != null ? player.PlayerName : $"Player {clientId}";

            Debug.Log($"--- {playerName} (ClientId: {clientId}) ---");
            Debug.Log($"  Total hymns: {hymns.Count}");

            for (int i = 0; i < hymns.Count; i++)
            {
                Debug.Log($"  Hymn {i + 1}: {hymns[i]}");
            }
        }

        Debug.Log("=== END DEBUG ===");
    }
}