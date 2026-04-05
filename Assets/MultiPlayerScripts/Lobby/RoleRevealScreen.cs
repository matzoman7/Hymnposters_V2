using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class RoleRevealScreen : NetworkBehaviour
{
    [SerializeField] private GameObject _angelPanel;
    [SerializeField] private GameObject _fallenAngelPanel;
    [SerializeField] private float _displayDuration = 5f;

    private void Awake()
    {
        _angelPanel.SetActive(false);
        _fallenAngelPanel.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        // Every client asks the server: "Who am I?"
        RequestRoleServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestRoleServerRpc(ulong clientId)
    {
        // 1. The Server ensures roles are actually assigned
        AssignRolesIfMissing();

        // 2. The Server finds the role for this specific client
        string assignedRole = "Angel"; // Default
        if (PlayerManager.AllPlayers.TryGetValue(clientId, out PlayerData data))
        {
            assignedRole = data.Role;
        }

        // 3. The Server tells only that client what their role is
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }
        };

        ReturnRoleClientRpc(assignedRole, clientRpcParams);
    }

    [ClientRpc]
    private void ReturnRoleClientRpc(string role, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"Role received from server: {role}");

        if (role == "Fallen Angel")
        {
            _fallenAngelPanel.SetActive(true);
            _angelPanel.SetActive(false);
        }
        else
        {
            _angelPanel.SetActive(true);
            _fallenAngelPanel.SetActive(false);
        }

        StartCoroutine(WaitThenLoad());
    }

    private void AssignRolesIfMissing()
    {
        bool roleExists = false;
        foreach (var p in PlayerManager.AllPlayers.Values)
        {
            if (p.Role == "Fallen Angel") { roleExists = true; break; }
        }

        if (!roleExists && PlayerManager.AllPlayers.Count > 0)
        {
            var keys = new System.Collections.Generic.List<ulong>(PlayerManager.AllPlayers.Keys);
            ulong winner = keys[Random.Range(0, keys.Count)];
            PlayerManager.AllPlayers[winner].Role = "Fallen Angel";
        }
    }

    private IEnumerator WaitThenLoad()
    {
        yield return new WaitForSeconds(_displayDuration);

        // Only the server handles the actual scene transition
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("AlexTest", LoadSceneMode.Single);
        }
    }
}