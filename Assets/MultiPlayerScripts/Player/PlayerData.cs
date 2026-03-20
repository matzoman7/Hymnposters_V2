using System;

[Serializable]
public class PlayerData
{
    public ulong ClientId;      // Unique network ID assigned by Netcode (0, 1, 2, etc.) Host is always 0
    public string PlayerName;   // Display name chosen by the player
    public string Role;         // "Fallen Angel" or "Angel"
    public bool IsAlive;        // Whether the player is still in the game

    // Constructor to create a new player with their info
    public PlayerData(ulong clientId, string playerName, string role = "Angel")
    {
        ClientId = clientId;
        PlayerName = playerName;
        Role = role;
        IsAlive = true; // Everyone starts alive
    }
}