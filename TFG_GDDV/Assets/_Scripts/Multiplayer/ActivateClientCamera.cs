using Unity.Netcode;
using UnityEngine;


public class ActivateClientCamera : NetworkBehaviour
{
    public Camera cam;
    public void Start()
    {
        if (IsClient)
        {
            if (cam != null)
            {
                cam.gameObject.SetActive(true);
                cam.tag = "MainCamera";
            }
        }
    }

}
