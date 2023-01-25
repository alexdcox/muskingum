using UnityEngine;

public class BlockColor : MonoBehaviour {
    public Color color;

    void Start() {
        SetColor();
    }

    void OnValidate() {
        SetColor();
    }

    void SetColor() {
        // Apparently this doesn't play nicely with prefabs because you can't
        // access the material of the renderer. You can access `sharedMaterial`
        // perhaps I need to have a play around here.

        var meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null && meshRenderer.sharedMaterial != null) {
            meshRenderer.sharedMaterial.color = color;
        }

        var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sharedMaterial != null) {
            spriteRenderer.sharedMaterial.color = color;
        }
    }
}
