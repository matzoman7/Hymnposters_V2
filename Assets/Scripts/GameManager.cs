using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Dynamic")]
    public Dictionary<int, List<string>> playerHymns = new Dictionary<int, List<string>>();


    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void AddHymn(int playerNumber, string hymn)
    {
        if (!playerHymns.ContainsKey(playerNumber))
            playerHymns[playerNumber] = new List<string>();

        playerHymns[playerNumber].Add(hymn);
        Debug.Log($"Player {playerNumber} submitted hymn: {hymn}");
    }
}
