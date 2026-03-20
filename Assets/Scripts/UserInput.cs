using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInput : MonoBehaviour
{
    [Header("Inscribed")]
    public TMP_InputField playerInput;
    public ulong playerID;//NetCode automatillcay makes playerId's ulong
    public void GetUserInput(string hymn)
    {
        Debug.Log(hymn);
        GameManager.instance.AddHymn(playerID, hymn);
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
