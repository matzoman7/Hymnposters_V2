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
        AssignRolesIfMissing();

        string assignedRole = "Angel";
        if (PlayerManager.AllPlayers.TryGetValue(clientId, out PlayerData data))
        {
            assignedRole = data.Role;
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } }
        };

        ReturnRoleClientRpc(assignedRole, clientRpcParams);
    }

    [ClientRpc]
    private void ReturnRoleClientRpc(string role, ClientRpcParams clientRpcParams = default)
    {
        ulong myId = NetworkManager.Singleton.LocalClientId;
        Debug.Log($"[CLIENT {myId}] Received Role: {role}");

        if (PlayerManager.Instance != null)
        {
            PlayerData myData = PlayerManager.Instance.GetPlayer(myId);
            if (myData == null)
            {
                // If the dictionary was empty on the client, create the entry now
                PlayerManager.Instance.AddPlayer(myId, "LocalPlayer", role);
            }
            else
            {
                myData.Role = role;
            }
        }

        // Show the correct UI
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
        // Only the Server checks this
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
            Debug.Log($"Assigned Fallen Angel to Client ID: {winner}");
        }
    }

    private IEnumerator WaitThenLoad()
    {
        yield return new WaitForSeconds(_displayDuration);

        // Only the server initiates the scene change for everyone
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("AlexTest", LoadSceneMode.Single);
        }
    }
}