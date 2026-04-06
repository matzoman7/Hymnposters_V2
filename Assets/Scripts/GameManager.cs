using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

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
    private NetworkVariable<ulong> currentTurnClientId = new NetworkVariable<ulong>(0); // Starts at 0 (host goes first)  
    private NetworkVariable<bool> isCurrentPlayerTyping = new NetworkVariable<bool>(false);// Track who is currently typing

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