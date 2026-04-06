using UnityEngine;

public class VotingButton : MonoBehaviour
{
    public ulong targetClientId; //this says what player was voted for 
    public VotingUI voteManager;

    public void VoteButton()
    {
        if (voteManager == null)
        {
            Debug.LogWarning("VotingButton has no VotingUI assigned.");
            return;
        }

        voteManager.RegisterVote(targetClientId);
    }
}
