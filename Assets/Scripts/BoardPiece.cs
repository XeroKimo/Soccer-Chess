using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BoardPiece : MonoBehaviour
{
    public Vector2Int position;

    public BoxCollider2D raycastCollider { get; private set; }

    private void Awake()
    {
        raycastCollider = GetComponent<BoxCollider2D>();
    }
}
