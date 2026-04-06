using NUnit.Framework;
using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class VotingUI : MonoBehaviour
{
    public List<TextMeshProUGUI> hymnLines = new List<TextMeshProUGUI>();
    public List<Button> buttonList = new List<Button>();
    public bool hasVoted;


    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RegisterVote(int votedPlayerID) 
    {
        if (hasVoted) return;
        ulong playerID = NetworkManager.Singleton.LocalClientId;
        switch (votedPlayerID) 
        {
            case 1:
                
                GameManager.instance.SubmitVoteServerRpc(playerID, votedPlayerID);
                break;
            case 2:
                
                GameManager.instance.SubmitVoteServerRpc(playerID, votedPlayerID);
                break;
            case 3:
                
                GameManager.instance.SubmitVoteServerRpc(playerID, votedPlayerID);
                break;
            case 4:
                
                GameManager.instance.SubmitVoteServerRpc(playerID, votedPlayerID);
                break;
        }

        foreach (Button button in buttonList)
        {
            button.interactable = false;
        }

        hasVoted = true;
    }


    public void EnableUI()
    {
        this.gameObject.SetActive(true);
    }

    public void OnEnable()
    {
        GameManager.onVotingRoundStart += EnableUI;
    }

    private void OnDisable()
    {
        GameManager.onVotingRoundStart -= EnableUI;
    }
}
