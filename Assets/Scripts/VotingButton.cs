using UnityEngine;

public class VotingButton : MonoBehaviour
{
    public int buttonID; //this says what player was voted for 
    public VotingUI voteManager;

    public void VoteButton()
    {
        voteManager.RegisterVote(buttonID);
    }
}
