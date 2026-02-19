using UnityEngine;
using PurrNet;// PurrNet library
using PurrNet.Transports;// needed for UDPTransport
using TMPro;

public class LobbyCreation : NetworkIdentity// NetworkIdentity lets this object exist on the network
{
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private UDPTransport _udpTransport;
    [SerializeField] private string joinIP = "127.0.0.1";// the IP address to connect to, defaults to localhost for testing
    [SerializeField] private TMP_InputField _nameInputField;
    [SerializeField] private GameObject _playerNetworkPrefab;

    public static string PlayerName { get; private set; } = "Player";// stores the player's name, defaults to "Player"

    private void SavePlayerName()
    {
        string inputName = _nameInputField.text.Trim();// get the typed name and remove any accidental spaces
        PlayerName = string.IsNullOrEmpty(inputName) ? "Player" : inputName;// if they left it blank use "Player", otherwise use what they typed
    }

    public void OnHostClicked()
    {
        if (_networkManager.isServer || _networkManager.isClient) return;// prevent starting multiple sessions
        SavePlayerName();
        _networkManager.StartServer();
        _networkManager.StartClient();// also connect as a client so the host can play too
        _networkManager.sceneModule.LoadSceneAsync("KacyScene");// load the game scene through PurrNet so it can track network objects in it
    }

    public void OnJoinClicked()
    {
        if (_networkManager.isClient) return;
        SavePlayerName();
        _udpTransport.address = joinIP;// set the IP address to connect to
        _networkManager.StartClient();// connect to the host as a client, server will handle loading the scene
    }
}