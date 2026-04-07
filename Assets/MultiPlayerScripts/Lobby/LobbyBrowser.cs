using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Core;
using Unity.Services.Authentication;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LobbyBrowser : MonoBehaviour
{
    [SerializeField] private Transform _lobbyListContent;
    [SerializeField] private GameObject _lobbyButtonPrefab;
    [SerializeField] private float _refreshInterval = 5f;

    private List<Lobby> _availableLobbies = new List<Lobby>();
    private bool _isInitialized = false;
    private string _joinedLobbyId = null; // Track which lobby we've joined

    private async void Start()
    {
        await InitializeUnityServices();

        if (_isInitialized)
        {
            await RefreshLobbyList();
            InvokeRepeating(nameof(RefreshLobbyListRepeating), _refreshInterval, _refreshInterval);
        }
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
            }
            _isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
        }
    }

    private async void RefreshLobbyListRepeating()
    {
        if (_joinedLobbyId != null)
        {
            await RefreshJoinedLobby();
        }
        else
        {
            await RefreshLobbyList();
        }
    }

    private async Task RefreshLobbyList()
    {
        if (!_isInitialized) return;

        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GE)
                }
            };

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
            _availableLobbies = response.Results;

            UpdateLobbyListUI();
        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.RateLimited)
            {
                Debug.LogWarning("Rate limited - slowing down refresh");
                CancelInvoke(nameof(RefreshLobbyListRepeating));
                InvokeRepeating(nameof(RefreshLobbyListRepeating), 7f, 7f);
            }
            else
            {
                Debug.LogWarning($"Failed to refresh lobbies: {e.Message}");
            }
        }
    }
    private async Task RefreshJoinedLobby()
    {
        try
        {
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobbyId);

            // Check if the host has started the game (you set this flag in LobbyCreation)
            if (lobby.Data.ContainsKey("GameStarted") && lobby.Data["GameStarted"].Value == "true")
            {
                Debug.Log("Game started — leaving lobby browser");
                CancelInvoke(nameof(RefreshLobbyListRepeating));
                return;
            }

            // Update just the joined lobby's entry in our list so its player count refreshes
            for (int i = 0; i < _availableLobbies.Count; i++)
            {
                if (_availableLobbies[i].Id == _joinedLobbyId)
                {
                    _availableLobbies[i] = lobby;
                    break;
                }
            }

            UpdateLobbyListUI();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning($"Failed to refresh joined lobby: {e.Message}");
            _joinedLobbyId = null; // Lobby probably closed, go back to browsing
        }
    }

    private void UpdateLobbyListUI()
    {
        foreach (Transform child in _lobbyListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (Lobby lobby in _availableLobbies)
        {
            GameObject buttonObj = Instantiate(_lobbyButtonPrefab, _lobbyListContent);

            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                int playerCount = lobby.Players.Count;
                buttonText.text = $"{lobby.Name}\n{playerCount}/{lobby.MaxPlayers} players";
            }

            Button button = buttonObj.GetComponent<Button>();

            if (lobby.Data != null && lobby.Data.ContainsKey("JoinCode"))
            {
                string lobbyId = lobby.Id;
                string joinCode = lobby.Data["JoinCode"].Value;
                button.onClick.AddListener(() => OnLobbyButtonClicked(lobbyId, joinCode));
            }
            else
            {
                button.interactable = false; // Lobby not ready yet
            }
        }
    }
    private void OnLobbyButtonClicked(string lobbyId, string joinCode)
    {
        _joinedLobbyId = lobbyId;
        FindFirstObjectByType<LobbyCreation>().JoinLobbyByCode(joinCode);
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(RefreshLobbyListRepeating));
    }
}