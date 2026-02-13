using UnityEngine;
using PurrNet;
using System.Collections;

public class PlayerNetwork : NetworkIdentity // NetworkIdentity lets this object exist on the network
{
    //This script is on a prefab that gets Instantiated by the PersistNetwork script, which is on the NetworkManager.
    protected override void OnSpawned()// called when this object spawns on the network
    {
        base.OnSpawned();
        SendNameToServer(LobbyCreation.PlayerName);// send this player's name to the server
    }

    [ServerRpc(requireOwnership: false)]// this method runs on the server, anyone can call it
    private void SendNameToServer(string name)
    {
        StartCoroutine(BroadcastAfterDelay(name));
    }

    private IEnumerator BroadcastAfterDelay(string name)// coroutine that adds a small delay before broadcasting. Without it the client/joined player did not recieve the info.
    {
        yield return new WaitForSeconds(0.5f);
        ReceiveNameOnClients(name);
    }

    [ObserversRpc]// this method runs on every connected player
    private void ReceiveNameOnClients(string name)
    {
        PlayerListUI.Instance?.AddPlayer(name);// add the name to the player list UI if it exists
    }
}