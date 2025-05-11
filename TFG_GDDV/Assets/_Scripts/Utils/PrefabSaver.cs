#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class PrefabSaver : MonoBehaviour
{
    [ContextMenu("Guardar Prefab Actualizado")]
    public void GuardarPrefab()
    {
        var prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(gameObject);
        if (prefabRoot == null)
        {
            Debug.LogWarning("Este objeto no pertenece a un prefab.");
            return;
        }

        var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);
        PrefabUtility.SaveAsPrefabAssetAndConnect(prefabRoot, path, InteractionMode.AutomatedAction);

        Debug.Log($"Prefab guardado en: {path}");
    }
}
#endif
