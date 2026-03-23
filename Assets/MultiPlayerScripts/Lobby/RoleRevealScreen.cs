using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class RoleRevealScreen : MonoBehaviour
{
    [SerializeField] private GameObject _angelPanel;
    [SerializeField] private GameObject _fallenAngelPanel;
    [SerializeField] private float _displayDuration = 3f;

    private void Start()
    {
        Debug.Log($"RoleRevealScreen: PlayerManager.AllPlayers.Count = {PlayerManager.AllPlayers.Count}");

        foreach (var kvp in PlayerManager.AllPlayers)
        {
            Debug.Log($"  ClientId {kvp.Key}: Name={kvp.Value.PlayerName}, Role={kvp.Value.Role}");
        }

        PlayerData localPlayerData = PlayerManager.Instance.GetLocalPlayer();


        if (localPlayerData != null)
        {
            Debug.Log($"localPlayerData.ClientId: {localPlayerData.ClientId}");
            Debug.Log($"localPlayerData.Role: {localPlayerData.Role}");
        }

        string role = (localPlayerData != null) ? localPlayerData.Role : "Angel";

        Debug.Log($"Final role being used: {role}");

        if (role == "Fallen Angel")
        {
            _fallenAngelPanel.SetActive(true);
            _angelPanel.SetActive(false);
        }
        else
        {
            _angelPanel.SetActive(true);
            _fallenAngelPanel.SetActive(false);
        }

        StartCoroutine(WaitThenLoadGame());
    }

    private IEnumerator WaitThenLoadGame()
    {
        yield return new WaitForSeconds(_displayDuration);

        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("AlexTest", LoadSceneMode.Single);
        }
    }
}