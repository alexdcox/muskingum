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
        if (meshRenderer != null && meshRenderer.material != null) {
            meshRenderer.material.color = color;
        }

        var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.material != null) {
            spriteRenderer.material.color = color;
        }
    }
}
