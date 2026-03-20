using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PromptGenerator : MonoBehaviour
{
    [SerializeField] private List<string> prompts = new List<string>();
    public string currentPrompt;
    public TextMeshProUGUI promptUI;
    
    private Dictionary<int, string> usedPrompts = new Dictionary<int, string>();
    
    public void GeneratePrompt()
    {
        if (usedPrompts.Count >= prompts.Count) 
        {
            Debug.Log("All prompts used.");
            usedPrompts.Clear();
        }

        int randomNum;
        do
        {
            randomNum = Random.Range(0, prompts.Count);
        }
        while (usedPrompts.ContainsKey(randomNum));
        //check if we already used this prompt 
        currentPrompt = prompts[randomNum];
        promptUI.text = currentPrompt;
        usedPrompts.Add(randomNum, prompts[randomNum]);
        
        
    }
}
