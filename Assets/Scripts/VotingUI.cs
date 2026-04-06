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

    public bool hasVoted;

    void Awake()
    {
        // Later: Display hymns line by line in player color
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
                votingButtons[i].targetClientId = 999999;
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
        this.gameObject.SetActive(true);
        hasVoted = false;
        Debug.Log("Voting Round Start");
        foreach (Button button in buttonList)
        {
            button.interactable = true;
        }

        List<ulong> allPlayers = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);
        AssignVoteTargets(allPlayers);
    }

    public void DisableUI()
    {
        this.gameObject.SetActive(false);
    }

    public void OnEnable()
    {
        GameManager.onVotingRoundStart += EnableUI;
        GameManager.onVotingRoundEnd += DisableUI;
    }

    private void OnDisable()
    {
        GameManager.onVotingRoundStart -= EnableUI;
        GameManager.onVotingRoundEnd -= DisableUI;
    }
}
