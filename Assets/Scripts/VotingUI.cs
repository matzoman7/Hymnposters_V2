using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class VotingUI : MonoBehaviour
{
    public List<TextMeshProUGUI> hymnLines = new List<TextMeshProUGUI>();
    public List<Button> buttonList = new List<Button>();
    public List<VotingButton> votingButtons = new List<VotingButton>();

    public GameObject votePanel;

    public bool hasVoted;

    void Awake()
    {
        // Later: Display hymns line by line in player color

        GameManager.onVotingRoundStart += EnableUI;
        Debug.Log("VotingUI subscirbed to onVotingRoundStart");
        GameManager.onVotingRoundEnd += DisableUI;
        Debug.Log("VotingUI subscirbed to onVotingRoundEnd");


    }

    public void RegisterVote(ulong votedPlayerClientId)
    {
        if (hasVoted) return;

        if (GameManager.instance == null)
        {
            Debug.LogWarning("GameManager instance is null.");
            return;
        }

        GameManager.instance.SubmitVoteServerRpc(votedPlayerClientId);

        foreach (Button button in buttonList)
        {
            button.interactable = false;
        }

        hasVoted = true;
    }

    public void AssignVoteTargets(List<ulong> playerClientIds)
    {
        ulong localClientId = NetworkManager.Singleton.LocalClientId;

        for (int i = 0; i < votingButtons.Count; i++)
        {
            // If there are fewer players than buttons, disable extras
            if (i >= playerClientIds.Count)
            {
                buttonList[i].interactable = false;
                votingButtons[i].targetClientId = ulong.MaxValue;
                continue;
            }

            votingButtons[i].targetClientId = playerClientIds[i];
            votingButtons[i].voteManager = this;

            // Disable self-vote
            if (playerClientIds[i] == localClientId)
            {
                buttonList[i].interactable = false;
            }
        }
    }

    public void EnableUI()
    {
        votePanel.SetActive(true);
        hasVoted = false;
        foreach (Button button in buttonList)
        {
            button.interactable = true;
        }

        List<ulong> allPlayers = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);
        AssignVoteTargets(allPlayers);
    }

    public void DisableUI()
    {
        votePanel.SetActive(false);
    }

    private void OnDestroy()
    {
        GameManager.onVotingRoundStart -= EnableUI;
        GameManager.onVotingRoundEnd -= DisableUI;
    }

    
}
