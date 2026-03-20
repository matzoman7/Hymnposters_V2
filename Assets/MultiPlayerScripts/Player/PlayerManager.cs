using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; } // Singleton so any script can access PlayerManager.Instance

    // Dictionary that stores all players - Key is their ClientId, Value is their PlayerData
    public static Dictionary<ulong, PlayerData> AllPlayers = new Dictionary<ulong, PlayerData>();

    private void Awake()
    {
        // Singleton pattern - only one PlayerManager exists, persists across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Don't destroy when loading new scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicates
        }
    }

    // Add a new player to the dictionary when they join
    public void AddPlayer(ulong clientId, string playerName, string role = "Angel")
    {
        if (!AllPlayers.ContainsKey(clientId)) // Only add if they don't exist already
        {
            AllPlayers[clientId] = new PlayerData(clientId, playerName, role);
            Debug.Log($"Added player: {playerName} (ID: {clientId})");
        }
    }

    // Get a specific player's data by their ClientId - EX: PlayerData secondPlayer = PlayerManager.Instance.GetPlayer(1); // Get player with ClientId 1
    public PlayerData GetPlayer(ulong clientId)
    {
        return AllPlayers.ContainsKey(clientId) ? AllPlayers[clientId] : null;
    }

    // Get the local player's data (the player on this machine) - EX: PlayerData localPlayerData = PlayerManager.Instance.GetLocalPlayer();
    public PlayerData GetLocalPlayer()
    {
        if (NetworkManager.Singleton == null) return null;
        ulong myId = NetworkManager.Singleton.LocalClientId; // Get this client's ID
        return GetPlayer(myId);
    }

    // Update a player's role (called when roles are assigned)
    public void SetPlayerRole(ulong clientId, string role)
    {
        if (AllPlayers.ContainsKey(clientId))
        {
            AllPlayers[clientId].Role = role;
        }
    }

    // Clear all players (when returning to lobby or resetting game)
    public void ClearAllPlayers()
    {
        AllPlayers.Clear();
    }
}