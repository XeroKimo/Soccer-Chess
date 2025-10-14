using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardPiece : MonoBehaviour
{
    [HideInInspector]
    public Vector2Int initialPosition;
    public Vector2Int position;

    public byte team;
    public Vector2 gizmoSquareSize;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    
    public Color GetOutlineColor()
    {
        return spriteRenderer.material.GetColor("_OutlineColor");
    }
    public void EnableOutline(float width = 0.03f)
    {
        spriteRenderer.material.SetFloat("_OutlineWidth", Mathf.Max(width, 0.01f));
    }
    public void EnableOutline(Color color, float width = 0.03f)
    {
        spriteRenderer.material.SetColor("_OutlineColor", color);
        spriteRenderer.material.SetFloat("_OutlineWidth", Mathf.Max(width, 0.01f));
    }

    public void DisableOutline()
    {
        spriteRenderer.material.SetColor("_OutlineColor", Color.black);
        spriteRenderer.material.SetFloat("_OutlineWidth", 0);
    }


}
