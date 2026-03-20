using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;


public class PromptGenerator : NetworkBehaviour
{
    [SerializeField] private List<string> prompts = new List<string>();
    public string currentPrompt;
    public TextMeshProUGUI promptUI;
    
    private Dictionary<int, string> usedPrompts = new Dictionary<int, string>();
    private NetworkVariable<int> currentPromptIndex = new NetworkVariable<int>(-1);


    [ServerRpc(RequireOwnership = false)]
    public void GeneratePromptServerRpc()
    {
        if (usedPrompts.Count >= prompts.Count)
        {
            usedPrompts.Clear();
        }

        int randomNum;
        do
        {
            randomNum = Random.Range(0, prompts.Count);
        } while (usedPrompts.ContainsKey(randomNum));

        usedPrompts.Add(randomNum, prompts[randomNum]);

        // Update the NetworkVariable 
        currentPromptIndex.Value = randomNum;

        Debug.Log($"[SERVER] New prompt generated: {prompts[randomNum]}");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Only the server generates the prompt
        if (IsServer)
        {
            GeneratePromptServerRpc();
        }

        // Update UI whenever the NetworkVariable changes
        currentPromptIndex.OnValueChanged += OnPromptChanged;

        // If the value was already set before this client spawned
        if (currentPromptIndex.Value != -1)
        {
            OnPromptChanged(-1, currentPromptIndex.Value);
        }
    }

    private void OnPromptChanged(int oldValue, int newValue)
    {
        promptUI.text = prompts[newValue];
    }
}
