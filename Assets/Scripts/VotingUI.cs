using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class VotingUI : MonoBehaviour
{
    public List<TextMeshProUGUI> player1HymnLines = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> player2HymnLines = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> player3HymnLines = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> player4HymnLines = new List<TextMeshProUGUI>();
    public List<Button> buttonList = new List<Button>();
    public List<VotingButton> votingButtons = new List<VotingButton>();
    public Color player1Color = Color.white;
    public Color player2Color = Color.yellow;
    public Color player3Color = Color.red;
    public Color player4Color = Color.blue;

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

    public void displayHyms()
    {
        List<string> allPlayerHymns = new List<string>();
        foreach (KeyValuePair<ulong, List<string>> playerEntry in GameManager.instance.playerHymns)
        {
            ulong clientId = playerEntry.Key;
            List<string> hymns = playerEntry.Value;

            for (int i = 0; i < hymns.Count; i++)
            {
                allPlayerHymns.Add(hymns[i]); //This results in allPlayerHymns having 0,1,2 be the first 3 hymns for player 1 then so on and so forth 
            }
        }
        int j = 0;
        foreach(TextMeshProUGUI hymnText in player1HymnLines)
        {
            hymnText.text = allPlayerHymns[j];
            hymnText.color = player1Color;
            j++;
        }
        foreach (TextMeshProUGUI hymnText in player2HymnLines)
        {
            hymnText.text = allPlayerHymns[j];
            hymnText.color = player2Color;
            j++;
        }
        foreach (TextMeshProUGUI hymnText in player3HymnLines)
        {
            hymnText.text = allPlayerHymns[j];
            hymnText.color = player3Color;
            j++;
        }
        foreach (TextMeshProUGUI hymnText in player4HymnLines)
        {
            hymnText.text = allPlayerHymns[j];
            hymnText.color = player4Color;
            j++;
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
        displayHyms();
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
