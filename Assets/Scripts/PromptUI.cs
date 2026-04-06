using TMPro;
using UnityEngine;

public class PromptUI : MonoBehaviour
{
    [Header("Inscribed")]
    public TextMeshProUGUI promptUI;

    [Header("Dynamic")]
    public string promptTxt;

    private bool displayed;

    public void SetPromptText()
    {
        
        if (displayed)
        {
            promptUI.text = "Prompt";
            displayed = false;
        } 
        else if (!displayed)
        {
            promptUI.text = promptTxt;
            displayed = true;
        }
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
