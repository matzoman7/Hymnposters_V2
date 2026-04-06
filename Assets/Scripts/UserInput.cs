using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UserInput : MonoBehaviour
{
    [Header("Inscribed")]
    public TMP_InputField playerInput;

    private string lastInputText = ""; // Track what was in the field last frame
    private void Update()
    {
        // Only enable input if it's their turn
        if (GameManager.instance != null)
        {
            bool isMyTurn = GameManager.instance.IsMyTurn();
            playerInput.interactable = isMyTurn;

            // If it's my turn and I'm typing, notify the server
            if (isMyTurn)
            {
                string currentText = playerInput.text;

                // Check if the player is actively typing (text is changing)
                bool isTyping = !string.IsNullOrEmpty(currentText);

                if (currentText != lastInputText)
                {
                    // Text changed, send typing status to server
                    GameManager.instance.UpdateTypingStatusServerRpc(NetworkManager.Singleton.LocalClientId, isTyping);
                    lastInputText = currentText;
                }
            }
        }
    }
    public void GetUserInput()
    {
        // Double check it's their turn before submitting
        if (!GameManager.instance.IsMyTurn()) return;

        string hymn = playerInput.text;
        Debug.Log(hymn);
        ulong playerID = NetworkManager.Singleton.LocalClientId;// Get the ClientId from Netcode
        GameManager.instance.SubmitHymnServerRpc(playerID, hymn);
        // clear input field
        playerInput.text = "";
        //send hymn to GM
        playerID = (playerID + 1) % 4;

    }
}
