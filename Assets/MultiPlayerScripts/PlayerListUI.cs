using UnityEngine;
using TMPro;

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
        if (!_playerListText.text.Contains(name))
            _playerListText.text += (string.IsNullOrEmpty(_playerListText.text) ? "" : ", ") + name;// if list is empty just add the name, otherwise add a comma then the name
    }
}