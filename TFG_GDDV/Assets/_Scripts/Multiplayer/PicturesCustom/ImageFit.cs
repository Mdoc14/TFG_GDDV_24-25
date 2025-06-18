using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ImageFit : MonoBehaviour
{
    public Texture2D texture;

    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void ApplyImage()
    {
        if (texture == null)
        {
            Debug.LogWarning("No se ha asignado ninguna imagen.");
            return;
        }

        MeshRenderer rend = GetComponent<MeshRenderer>();
        texture.wrapMode = TextureWrapMode.Clamp;
        rend.material.mainTexture = texture;

        float imageAspect = (float)texture.width / texture.height;
        float frameAspect = originalScale.x / originalScale.y;

        Vector3 newScale = originalScale;

        if (imageAspect > frameAspect)
        {
            newScale.y = originalScale.x / imageAspect;
        }
        else
        {
            newScale.x = originalScale.y * imageAspect;
        }

        transform.localScale = newScale;
    }
}
