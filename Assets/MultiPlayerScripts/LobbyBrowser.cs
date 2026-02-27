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

    private async void Start()
    {
        await InitializeUnityServices(); // Connect to Unity's cloud services

        if (_isInitialized)
        {
            await RefreshLobbyList(); // Get initial list of lobbies
            InvokeRepeating(nameof(RefreshLobbyListRepeating), _refreshInterval, _refreshInterval); // Keep refreshing every 5 seconds
        }
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            // Initialize Unity Services if not already done
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();

                // Sign in anonymously if not already signed in
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
        await RefreshLobbyList(); // Wrapper for InvokeRepeating since it can't call async methods directly
    }

    private async Task RefreshLobbyList()
    {
        if (!_isInitialized) return; // Don't query if services aren't ready

        try
        {
            // Set up query to find lobbies with open slots
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 25, // Get up to 25 lobbies
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) // Only show lobbies with available slots
                }
            };

            // Query Unity's lobby service
            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
            _availableLobbies = response.Results;

            UpdateLobbyListUI(); // Update the UI with the new list
        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.RateLimited)
            {
                // Too many requests, slow down refresh rate
                Debug.LogWarning("Rate limited - slowing down refresh");
                CancelInvoke(nameof(RefreshLobbyListRepeating));
                InvokeRepeating(nameof(RefreshLobbyListRepeating), 10f, 10f); // Refresh every 10 seconds instead
            }
            else
            {
                Debug.LogWarning($"Failed to refresh lobbies: {e.Message}");
            }
        }
    }

    private void UpdateLobbyListUI()
    {
        // Clear old lobby buttons
        foreach (Transform child in _lobbyListContent)
        {
            Destroy(child.gameObject);
        }

        // Create a button for each available lobby
        foreach (Lobby lobby in _availableLobbies)
        {
            GameObject buttonObj = Instantiate(_lobbyButtonPrefab, _lobbyListContent);

            // Update button text with lobby name and player count
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                int playerCount = lobby.MaxPlayers - lobby.AvailableSlots;
                buttonText.text = $"{lobby.Name}\n{playerCount}/{lobby.MaxPlayers} players";
            }

            // Add click listener to join this lobby
            Button button = buttonObj.GetComponent<Button>();
            string joinCode = lobby.Data["JoinCode"].Value; // Get the Relay join code from lobby data
            button.onClick.AddListener(() => OnLobbyButtonClicked(joinCode));
        }
    }

    private void OnLobbyButtonClicked(string joinCode)
    {
        // Tell LobbyCreation to join this lobby using its join code
        FindFirstObjectByType<LobbyCreation>().JoinLobbyByCode(joinCode);
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(RefreshLobbyListRepeating)); // Stop refreshing when this object is destroyed
    }
}