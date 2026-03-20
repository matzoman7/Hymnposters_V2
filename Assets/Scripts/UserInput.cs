using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UserInput : MonoBehaviour
{
    [Header("Inscribed")]
    public TMP_InputField playerInput;
    public void GetUserInput(string hymn)
    {
        Debug.Log(hymn);
        ulong playerID = NetworkManager.Singleton.LocalClientId;// Get the ClientId from Netcode
        GameManager.instance.SubmitHymnServerRpc(playerID, hymn);
        // clear input field
        playerInput.text = "";
        Invoke("DisableInput", 0.01f);
        //send hymn to GM
        Invoke("ResetField", 2f);
        playerID = (playerID + 1) % 4;

    }

    public void DisableInput()
    {
        playerInput.interactable = false;
    }

    public void ResetField()
    {
        playerInput.interactable = true;
    }
}
