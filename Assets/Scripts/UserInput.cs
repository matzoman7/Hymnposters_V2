using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UserInput : MonoBehaviour
{
    [Header("Inscribed")]
    public TMP_InputField playerInput;
    public void GetUserInput()
    {
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
