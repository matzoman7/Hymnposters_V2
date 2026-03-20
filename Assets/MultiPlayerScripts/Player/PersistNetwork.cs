using UnityEngine;
using Unity.Netcode;

public class PersistNetwork : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // Keep NetworkManager alive across scenes
    }
}