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
using System.Threading.Tasks; // For async/await

public class LobbyCreation : MonoBehaviour
{
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private TMP_InputField _nameInputField;
    [SerializeField] private TMP_InputField _lobbyNameInputField;

    public static string PlayerName { get; private set; } = "Player"; // Stores player's name
    private Lobby _currentLobby;

    private async void Start() // async allows 'await' for Unity Services
    {
        await UnityServices.InitializeAsync(); // Connect to Unity's cloud services
        await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Sign in anonymously
    }

    private void SavePlayerName()
    {
        string inputName = _nameInputField.text.Trim(); // Get typed name, remove spaces


        PlayerName = string.IsNullOrEmpty(inputName) ? "Player" : inputName; //THIS DOESNT WORK idk why ill figure it out later.
    }

    public async void OnHostClicked() // waits for Relay/Lobby setup
    {
        if (_networkManager.IsServer || _networkManager.IsClient) return; // Prevent multiple sessions
        SavePlayerName();

        string lobbyName = _lobbyNameInputField.text.Trim(); // Get the lobby name
        if (string.IsNullOrEmpty(lobbyName)) lobbyName = $"{PlayerName}'s Lobby"; // Default lobby name

        await StartHost(lobbyName); // Create the lobby and start hosting
    }

    private async Task StartHost(string lobbyName)
    {
        try
        {
            //Create a Relay allocation
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4); // Max 4 players
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId); // Get a join code like "ABCD-1234"

            //Unity Transport uses Relay instead of direct IP
            var transport = _networkManager.GetComponent<UnityTransport>();
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4, // Relay server IP
                (ushort)allocation.RelayServer.Port, // Relay server port
                allocation.AllocationIdBytes, // Your unique room ID
                allocation.Key, // Security key
                allocation.ConnectionData // Connection info
            );

            //Creates a lobby that shows up in the lobby list
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false, // Public lobby, anyone can see it
                Data = new System.Collections.Generic.Dictionary<string, DataObject>
                {
                    { "JoinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) } // Store join code in lobby data so clients can connect via Relay
                }
            };

            _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 4, options); // Create the lobby with max 4 players

            //Start hosting
            _networkManager.StartHost(); // Start as host (server + client)
            _networkManager.SceneManager.LoadScene("LobbyWaitingRoom", UnityEngine.SceneManagement.LoadSceneMode.Single); // Load game scene
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}"); // Log any errors
        }
    }
    public async void JoinLobbyByCode(string joinCode)
    {
        if (_networkManager.IsServer || _networkManager.IsClient) return; // Prevent joining if already connected
        SavePlayerName();

        try
        {
            // Join the Relay using the join code
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            //Configure transport to connect via Relay
            var transport = _networkManager.GetComponent<UnityTransport>();
            transport.SetClientRelayData(
                allocation.RelayServer.IpV4, // Relay server IP
                (ushort)allocation.RelayServer.Port, // Relay server port
                allocation.AllocationIdBytes, // Your unique room ID
                allocation.Key, // Security key
                allocation.ConnectionData, // Connection data
                allocation.HostConnectionData // Host's connection data
            );

            //Start as client
            _networkManager.StartClient();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");
        }
    }
}