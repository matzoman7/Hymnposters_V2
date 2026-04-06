using UnityEngine;
using Unity.Netcode;

public class NetworkSpawner : MonoBehaviour
{
    // Drag your PlayerManager PREFAB here in the Inspector
    [SerializeField] private GameObject playerManagerPrefab;

    private void Start()
    {
        // We tell the NetworkManager: "When the server starts, run my code!"
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        }
    }

    private void OnServerStarted()
    {
        // ONLY the server (Host) can spawn networked objects
        if (NetworkManager.Singleton.IsServer)
        {
            // 1. Create the object from your prefab
            GameObject go = Instantiate(playerManagerPrefab);

            // 2. This is the magic line that makes it a NETWORK object
            // This tells all clients to create their own version of this object too
            go.GetComponent<NetworkObject>().Spawn();

            Debug.Log("PlayerManager has been officially SPAWNED on the network.");
        }
    }

    private void OnDestroy()
    {
        // Good practice to unsubscribe when the object is destroyed
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
    }
}
