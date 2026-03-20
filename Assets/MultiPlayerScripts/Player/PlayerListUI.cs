using TMPro;
using UnityEngine;

public class PlayerListUI : MonoBehaviour
{
    public static PlayerListUI Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI _playerListText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPlayer(string name)
    {
        // Make text visible when first player is added
        Color c = _playerListText.color;
        c.a = 1f;
        _playerListText.color = c;

        // Add header if this is the first name
        if (string.IsNullOrEmpty(_playerListText.text))
        {
            _playerListText.text = "Player List:\n";
        }

        if (!_playerListText.text.Contains(name))
            _playerListText.text += name + "\n";
    }

    public void ClearPlayers()
    {
        _playerListText.text = "";
    }
}