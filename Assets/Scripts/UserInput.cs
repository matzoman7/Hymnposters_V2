using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UserInput : MonoBehaviour
{
    [Header("Inscribed")]
    public TMP_InputField playerInput;
    private void Update()
    {
        // Only enable input if it's their turn
        if (GameManager.instance != null)
        {
            bool isMyTurn = GameManager.instance.IsMyTurn();
            playerInput.interactable = isMyTurn;
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

    public void DisableUI()
    {
        this.gameObject.SetActive(false);

    }

    public void OnEnable()
    {
        GameManager.onHymnRoundEnd += DisableUI;
    }

    public void OnDisable() 
    { 
        GameManager.onHymnRoundEnd -= DisableUI;
    }
}
