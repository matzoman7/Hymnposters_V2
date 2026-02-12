using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Buttons : MonoBehaviour
{
    [Header("Inscribed")]
    public string lobbyName;

    public void StartButton()
    {
        SceneManager.LoadScene("JoinLobbyScreen");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void BackToStartScreen()
    {
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
}
