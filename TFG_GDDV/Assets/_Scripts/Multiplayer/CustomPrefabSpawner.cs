using Unity.Netcode;
using UnityEngine;

public class CustomPrefabSpawner : NetworkBehaviour
{
    public GameObject hostPrefab;
    public GameObject clientVRPrefab;

    private void Awake()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager is not initialized.");
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        // Desactivar auto-spawn del PlayerPrefab
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = null;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost && clientId == NetworkManager.Singleton.LocalClientId)
        {
            // El host se spawnea a sí mismo
            GameObject hostObj = Instantiate(hostPrefab);
            hostObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
        else if (!IsServer && clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Si soy cliente, pido al servidor que me spawnee
            RequestClientVRSpawnServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestClientVRSpawnServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        GameObject clientObj = Instantiate(clientVRPrefab);
        clientObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}
