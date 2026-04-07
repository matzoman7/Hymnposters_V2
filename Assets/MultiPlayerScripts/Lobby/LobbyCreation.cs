using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Threading.Tasks;

public class LobbyCreation : MonoBehaviour
{
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private TMP_InputField _nameInputField;
    [SerializeField] private TMP_InputField _lobbyNameInputField;

    public static string PlayerName { get; private set; } = "Player";
    private Lobby _currentLobby;
    private float _heartbeatTimer;
    private const float HeartbeatInterval = 15f; // Ping every 15s, lobby times out at 30s

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        if (_currentLobby == null) return;

        _heartbeatTimer -= Time.deltaTime;
        if (_heartbeatTimer <= 0)
        {
            _heartbeatTimer = HeartbeatInterval;
            SendHeartbeat();
        }
    }

    private async void SendHeartbeat()
    {
        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
            Debug.Log("Lobby heartbeat sent");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning($"Heartbeat failed: {e.Message}");
        }
    }

    private void SavePlayerName()
    {
        string inputName = _nameInputField.text.Trim();

        if (string.IsNullOrEmpty(inputName))
        {
            int counter = PlayerPrefs.GetInt("PlayerCounter", 0);
            PlayerName = $"Player_{counter}";
            PlayerPrefs.SetInt("PlayerCounter", counter + 1);
            PlayerPrefs.Save();
        }
        else
        {
            PlayerName = inputName;
        }

        Debug.Log($"Player name set to: {PlayerName}");
    }

    public async void OnHostClicked()
    {
        if (_networkManager.IsServer || _networkManager.IsClient) return;
        SavePlayerName();

        string lobbyName = _lobbyNameInputField.text.Trim();
        if (string.IsNullOrEmpty(lobbyName)) lobbyName = $"{PlayerName}'s Lobby";

        await StartHost(lobbyName);
    }

    private async Task StartHost(string lobbyName)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var transport = _networkManager.GetComponent<UnityTransport>();
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new System.Collections.Generic.Dictionary<string, DataObject>
            {
                { "JoinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) },
                { "HostName", new DataObject(DataObject.VisibilityOptions.Public, PlayerName) }
            }
            };

            _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 4, options);
            _heartbeatTimer = HeartbeatInterval;

            DontDestroyOnLoad(gameObject);

            _networkManager.StartHost();
            _networkManager.SceneManager.LoadScene("LobbyWaitingRoom", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
        }
    }

    public async void JoinLobbyByCode(string joinCode)
    {
        if (_networkManager.IsServer || _networkManager.IsClient) return;
        SavePlayerName();

        try
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            var transport = _networkManager.GetComponent<UnityTransport>();
            transport.SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );

            _networkManager.StartClient();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");
        }
    }

    private async void OnDestroy()
    {
        // FIX: Clean up lobby when host leaves so it doesn't linger as a ghost lobby
        if (_currentLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogWarning($"Failed to delete lobby on exit: {e.Message}");
            }
        }
    }
}