using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Buttons : MonoBehaviour
{
    [Header("Inscribed")]
    public string lobbyName;
    public GameObject tutorialScreen;
    public void StartButton()
    {
        SceneManager.LoadScene("JoinLobbyScreen");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        // Clear all player data
        PlayerManager.Instance?.ClearAllPlayers();

        // Clear the UI list
        PlayerListUI.Instance?.ClearPlayers();

        // Shut down networking
        if (Unity.Netcode.NetworkManager.Singleton != null)
            Unity.Netcode.NetworkManager.Singleton.Shutdown();

        SceneManager.LoadScene("StartScene");
    }

    public void GoToTestLobby()
    {
        SceneManager.LoadScene(lobbyName);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MainGamePlayScene");
    }

    public void JoinLobbyScreen()
    {
        SceneManager.LoadScene("JoinLobbyScreen");
    }
    public void MainMenuScreen()
    {
        SceneManager.LoadScene("StartScene");
    }

    public void BackToMainButton()
    {
        tutorialScreen.SetActive(false);
    }

    public void GoToTutorialButton()
    {
        tutorialScreen.SetActive(true);
    }
}
