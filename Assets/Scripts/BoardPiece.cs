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

    public SpriteRenderer spriteRenderer { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
}
