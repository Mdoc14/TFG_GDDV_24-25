using System.Collections.Generic;
using UnityEngine;

public class ForceObjectsActive : MonoBehaviour
{
    public List<GameObject> objectsToForceActive;

    void Update()
    {
        foreach (GameObject obj in objectsToForceActive)
        {
            if (obj != null && !obj.activeSelf)
            {
                obj.SetActive(true);
            }
        }
    }
}
