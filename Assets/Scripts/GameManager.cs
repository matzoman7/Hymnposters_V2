using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour // NetworkBehaviour allows this to use networking features
{
    public static GameManager instance; 

    // Dictionary that stores hymns for each player
    // Key: ClientId (0, 1, 2, 3...)
    // Value: List of hymns that player has submitted
    public Dictionary<ulong, List<string>> playerHymns = new Dictionary<ulong, List<string>>();

    // NetworkVariable automatically syncs across all clients - everyone sees the same value
    // This tracks whose turn it currently is by their ClientId
    private NetworkVariable<ulong> currentTurnClientId = new NetworkVariable<ulong>(0); // Starts at 0 (host goes first)

    void Awake()
    {
        // Singleton setup - make sure only one GameManager exists
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        //Generate Prompt
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