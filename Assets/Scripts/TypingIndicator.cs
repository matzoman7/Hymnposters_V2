using UnityEngine;
using TMPro;

public class TypingIndicator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI typingText;
    private bool _votingStarted = false;

    private void Update()
    {
        if (_votingStarted) return;

        if (GameManager.instance != null)
        {
            bool isMyTurn = GameManager.instance.IsMyTurn();

            if (!isMyTurn)
            {
                string playerName = GameManager.instance.GetCurrentPlayerName();
                typingText.text = $"{playerName} is typing...";
                typingText.gameObject.SetActive(true);
            }
            else
            {
                typingText.gameObject.SetActive(false);
            }
        }
    }

    private void OnEnable()
    {
        GameManager.onVotingRoundStart += HideTypingIndicator;
    }

    private void OnDisable()
    {
        GameManager.onVotingRoundStart -= HideTypingIndicator;
    }

    private void HideTypingIndicator()
    {
        _votingStarted = true;
        typingText.gameObject.SetActive(false);
    }
}