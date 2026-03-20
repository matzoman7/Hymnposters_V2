using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections.Generic;

public class WaitingRoomManager : NetworkBehaviour
{
    [SerializeField] private Button _startGameButton;
    [SerializeField] private string _gameSceneName = "RoleReveal";

    private List<string> _allPlayerNames = new List<string>();

    private void Start()
    {
        // Only show Start Game button to the host
        if (_startGameButton != null)
        {
            _startGameButton.gameObject.SetActive(IsServer);
            _startGameButton.onClick.AddListener(OnStartGameClicked);
        }

        // Every client sends their name to the server when they join
        if (IsClient)
        {
            Invoke(nameof(SendPlayerName), 0.5f); // Small delay to ensure everything is ready
        }
    }

    private void SendPlayerName()
    {
        Debug.Log($"Sending player name: {LobbyCreation.PlayerName}");
        SendPlayerNameServerRpc(LobbyCreation.PlayerName);
    }

    [ServerRpc(RequireOwnership = false)] // Runs on server, any client can call
    private void SendPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        // Get the ClientId of the player who sent this
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        // Add player to PlayerManager with their ClientId and name
        PlayerManager.Instance.AddPlayer(clientId, playerName);

        // Add new player to the server's name list (for UI display)
        if (!_allPlayerNames.Contains(playerName))
        {
            _allPlayerNames.Add(playerName);
        }

        // Tell all clients to refresh their player list display
        RefreshAllClientsListClientRpc();
    }

    [ClientRpc] // Runs on all clients
    private void RefreshAllClientsListClientRpc()
    {
        if (IsServer)
        {
            // Server already has the list, just display it
            PlayerListUI.Instance?.ClearPlayers();
            foreach (string name in _allPlayerNames)
            {
                PlayerListUI.Instance?.AddPlayer(name);
            }
        }
        else
        {
            // Clients need to request the full list from server
            RequestFullListServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestFullListServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Get the ID of the client who requested the list
        ulong clientId = serverRpcParams.Receive.SenderClientId;

        // Clear their current list
        SendClearListClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }
        });

        // Send them each name one by one
        foreach (string name in _allPlayerNames)
        {
            SendSingleNameClientRpc(name, new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }
            });
        }
    }

    [ClientRpc]
    private void SendClearListClientRpc(ClientRpcParams clientRpcParams = default)
    {
        PlayerListUI.Instance?.ClearPlayers();
    }

    [ClientRpc]
    private void SendSingleNameClientRpc(string name, ClientRpcParams clientRpcParams = default)
    {
        PlayerListUI.Instance?.AddPlayer(name);
    }

    private void OnStartGameClicked()
    {
        if (!IsServer) return; // Only host can start
        AssignRolesAndStart();
    }

    private void AssignRolesAndStart()
    {
        // Get all connected players
        List<ulong> allPlayerIds = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);

        // Randomly pick one to be the Fallen Angel
        int randomIndex = Random.Range(0, allPlayerIds.Count);
        ulong fallenAngelClientId = allPlayerIds[randomIndex];

        Debug.Log($"Assigning roles - Fallen Angel: {fallenAngelClientId}");

        // Assign roles to everyone using PlayerManager
        foreach (ulong clientId in allPlayerIds)
        {
            string assignedRole = (clientId == fallenAngelClientId) ? "Fallen Angel" : "Angel";

            // Update PlayerManager with the role
            PlayerManager.Instance.SetPlayerRole(clientId, assignedRole);

            // Send each player their role via ClientRpc
            SendRoleClientRpc(assignedRole, new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }
            });
        }

        // Wait a moment for roles to sync, then load the reveal scene
        Invoke(nameof(LoadGameScene), 1f);
    }

    [ClientRpc]
    private void SendRoleClientRpc(string assignedRole, ClientRpcParams clientRpcParams = default)
    {
        // Store this client's role in PlayerManager
        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        // Make sure the player exists in PlayerManager first
        if (PlayerManager.AllPlayers.ContainsKey(localClientId))
        {
            PlayerManager.Instance.SetPlayerRole(localClientId, assignedRole);
            Debug.Log($"Received role: {assignedRole}");
        }
    }

    private void LoadGameScene()
    {
        // Server loads the role reveal scene, clients follow automatically
        NetworkManager.Singleton.SceneManager.LoadScene(_gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}