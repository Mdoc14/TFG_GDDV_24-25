using Unity.Netcode;
using UnityEngine;

public class ChildTransformSync : NetworkBehaviour
{
    [SerializeField] private Transform trackedSource;

    [SerializeField] private Transform remoteVisualTarget;

    void Update()
    {
        if (IsOwner)
        {
            SendTransformToServerRpc(trackedSource.position, trackedSource.rotation);
        }
    }

    [ServerRpc]
    private void SendTransformToServerRpc(Vector3 position, Quaternion rotation)
    {
        if (remoteVisualTarget != null)
        {
            remoteVisualTarget.position = position;
            remoteVisualTarget.rotation = rotation;
        }
    }
}
