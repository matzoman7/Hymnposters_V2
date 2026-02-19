using UnityEngine;
using PurrNet;

public class PersistNetwork : MonoBehaviour
{
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private GameObject _playerNetworkPrefab;

    private bool _serverReady = false;// tracks whether the server has finished connecting
    private bool _clientReady = false;// tracks whether the client has finished connecting

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()// called when the object becomes active
    {
        _networkManager.onPlayerJoined += OnPlayerJoined; // becomes this event
    }

    private void OnDisable()// called when the object becomes inactive
    {
        _networkManager.onPlayerJoined -= OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerID player, bool isReconnect, bool asServer)
    {
        Debug.Log("Player joined! asServer: " + asServer);

        if (asServer) _serverReady = true;// mark server as ready
        else _clientReady = true;// mark client as ready

        if (_serverReady && _clientReady) 
        {
            Debug.Log("Both ready, spawning PlayerNetwork!");
            _networkManager.Spawn(Instantiate(_playerNetworkPrefab));// instantiate the PlayerNetwork Prefab which has the PlayerNetwork Script on it.
        }
    }
}