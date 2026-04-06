using UnityEngine;
using TMPro;

public class TypingIndicator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI typingText;

    private void Update()
    {
        if (GameManager.instance != null)
        {
            //Only show typing indicator if it's not your turn
            bool isMyTurn = GameManager.instance.IsMyTurn();
            bool someoneIsTyping = GameManager.instance.IsCurrentPlayerTyping();

            if (!isMyTurn && someoneIsTyping)
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
}