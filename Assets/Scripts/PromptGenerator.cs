using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;


public class PromptGenerator : NetworkBehaviour
{
    [SerializeField] private List<string> prompts = new List<string>();
    [SerializeField] private List<string> fallenAngelPrompts = new List<string>();
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
        /*if (IsServer)
        {
            GeneratePromptServerRpc();
        }*/

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
        // Instead of calling it directly, start the "Wait and Display" process
        StopAllCoroutines();
        StartCoroutine(WaitAndDisplayPrompt());
    }
    private IEnumerator WaitAndDisplayPrompt()
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;

        // Safety check: Wait until the PlayerManager Instance is actually ready in this scene
        while (PlayerManager.Instance == null)
        {
            yield return null; // Wait one frame
        }

        // Now call the display logic
        ActualDisplayLogic();
    }

    private void ActualDisplayLogic()
    {
        int index = currentPromptIndex.Value;
        if (index == -1) return;

        ulong localId = NetworkManager.Singleton.LocalClientId;
        PlayerData localPlayer = PlayerManager.Instance?.GetPlayer(localId);

        if (localPlayer != null)
        {
            Debug.Log($"[CLIENT {localId}] Role: <color=yellow>{localPlayer.Role}</color>");

            if (localPlayer.Role == "Fallen Angel")
            {
                promptUI.text = (index < fallenAngelPrompts.Count) ? fallenAngelPrompts[index] : "Blend in";
            }
            else
            {
                if (index < prompts.Count)
                {
                    promptUI.text = prompts[index];
                }
            }
        }
    }
}
