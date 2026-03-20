using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    // Called when this object spawns on the network
    public override void OnNetworkSpawn()
    {
        if (IsOwner) // Only the owner sends their name
        {
            SendNameToServerRpc(LobbyCreation.PlayerName);
        }
    }

    [ServerRpc] // Runs on the server
    private void SendNameToServerRpc(string name)
    {
        StartCoroutine(BroadcastAfterDelay(name)); // Add delay before broadcasting
    }

    private IEnumerator BroadcastAfterDelay(string name)
    {
        yield return new WaitForSeconds(2f); // Wait for clients to be ready
        ReceiveNameOnClientsClientRpc(name);
    }

    [ClientRpc] // Runs on all clients
    private void ReceiveNameOnClientsClientRpc(string name)
    {
        PlayerListUI.Instance?.AddPlayer(name); // Add to the player list
    }
}