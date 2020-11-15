using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementIndicators : MonoBehaviour
{
    public SpriteRenderer[] tiles { get; private set; }

    private void Awake()
    {
        tiles = GetComponentsInChildren<SpriteRenderer>();
    }

    public void SetColor(Color color)
    {
        foreach(SpriteRenderer tile in tiles)
        {
            tile.color = color;
        }
    }

    public void DeactivateAll()
    {
        foreach(SpriteRenderer tile in tiles)
        {
            tile.enabled = false;
        }
    }
}
