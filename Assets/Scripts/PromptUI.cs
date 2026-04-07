using TMPro;
using UnityEngine;

public class PromptUI : MonoBehaviour
{
    [Header("Inscribed")]
    public TextMeshProUGUI promptUI;

    [Header("Dynamic")]
    public string promptTxt;

    private bool displayed;

    private void Awake()
    {
        GameManager.onHymnRoundEnd += DisableUI;
        Debug.Log("PromptUI subscirbed to onHymnRoundEnd");
    }

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

    private void OnDestroy()
    {
        GameManager.onHymnRoundEnd -= DisableUI;
    }

    
}
