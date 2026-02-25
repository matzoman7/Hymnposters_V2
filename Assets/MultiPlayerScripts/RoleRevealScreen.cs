using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class RoleRevealScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _roleText;
    [SerializeField] private Image _panelBackground;
    [SerializeField] private GameObject _canvas;
    [SerializeField] private float _displayDuration = 3f;

    private void Start()
    {
        // Get this client's unique ID
        ulong myId = NetworkManager.Singleton.LocalClientId;

        // Look up their assigned role from the dictionary, default to Angel if not found
        string role = WaitingRoomManager.PlayerRoles.ContainsKey(myId)
            ? WaitingRoomManager.PlayerRoles[myId]
            : "Angel";

        Debug.Log($"My role: {role}");

        // Update UI based on role
        if (role == "Fallen Angel")
        {
            _roleText.text = "You are the\nFallen Angel!";
            _roleText.color = Color.white;
            _panelBackground.color = Color.red; // Red background for fallen angel
        }
        else
        {
            _roleText.text = "You are an\nAngel!";
            _roleText.color = Color.white;
            _panelBackground.color = Color.green; // Green background for angel
        }

        _canvas.SetActive(true); // Show the updated canvas
        StartCoroutine(WaitThenLoadGame()); // Start countdown to main game
    }

    private IEnumerator WaitThenLoadGame()
    {
        yield return new WaitForSeconds(_displayDuration); // Wait for players to see their role

        // Only the server loads the scene, clients follow automatically
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MainGamePlayScene", LoadSceneMode.Single);
        }
    }
}