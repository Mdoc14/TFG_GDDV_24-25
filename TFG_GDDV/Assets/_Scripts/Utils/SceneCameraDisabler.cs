using UnityEngine;

public class SceneCameraDisabler : MonoBehaviour
{
    void Update()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            var netObj = player.GetComponent<Unity.Netcode.NetworkObject>();
            if (netObj != null && netObj.IsOwner)
            {
                // Desactiva solo los componentes, no el objeto entero
                var cam = GetComponent<Camera>();
                if (cam != null) cam.enabled = false;

                var audio = GetComponent<AudioListener>();
                if (audio != null) audio.enabled = false;

                // Opcional: destruir el script si ya ha hecho su trabajo
                Destroy(this);
            }
        }
    }
}
