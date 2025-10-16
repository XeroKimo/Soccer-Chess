using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public enum ChessType
{
    Knight,
    Bishop,
    Rook,
    Queen,
    King,
    Ball
}

public class ChessPiece : MonoBehaviour
{
    public ChessType type;
    public PlacedPiece? placedData;

    public Vector2Int position => placedData.Value.position;
    public GameBoard board => placedData.Value.board;

    public byte team;
    public Vector2 gizmoSquareSize;

    private SpriteRenderer spriteRenderer;

    void Awake()
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

    // Update is called once per frame
    void Update()
    {

    }

    public bool IsValidMove(Vector2Int position)
    {
        return type switch
        {
            ChessType.Knight => CanMoveKnight(placedData.Value.position, position),
            ChessType.Rook => CanMoveRook(placedData.Value.position, position),
            ChessType.Bishop => CanMoveBishop(placedData.Value.position, position),
            ChessType.Queen => CanMoveQueen(placedData.Value.position, position),
            ChessType.King => CanMoveKing(placedData.Value.position, position),
            ChessType.Ball => true,
            _ => throw new System.Exception("Unhandled type")
        };
    }

    public IEnumerable<(Vector2Int, bool)> GetValidPositions(bool skipBlocked, Vector2Int? direction = null)
    {
        return type switch
        {
            ChessType.Knight => KnightEnumeration(placedData.Value.position, placedData.Value.board, skipBlocked, direction),
            ChessType.Rook => StraightEnumeration(placedData.Value.position, placedData.Value.board, skipBlocked, direction, null),
            ChessType.Bishop => DiagonalEnumeration(placedData.Value.position, placedData.Value.board, skipBlocked, direction, null),
            ChessType.Queen => StraightEnumeration(placedData.Value.position, placedData.Value.board, skipBlocked, direction, null)
                .Concat(DiagonalEnumeration(placedData.Value.position, placedData.Value.board, skipBlocked, direction, null)),
            ChessType.King => StraightEnumeration(placedData.Value.position, placedData.Value.board, skipBlocked, direction, 1)
                .Concat(DiagonalEnumeration(placedData.Value.position, placedData.Value.board, skipBlocked, direction, 1)),
            _ => default
        };
    }

    private void OnDrawGizmos()
    {
        if(!enabled)
            return;
        Gizmos.color = Color.green;

        switch(type)
        {
        case ChessType.Knight:
            Gizmos.color = Color.yellow;

            break;
        case ChessType.Bishop:
            Gizmos.color = Color.magenta;
            break;
        case ChessType.Rook:
            Gizmos.color = Color.black;
            break;
        case ChessType.Queen:
            Gizmos.color = Color.white;
            break;
        case ChessType.King:
            Gizmos.color = Color.red;
            break;
        }
        if(team == 1)
            Gizmos.color -= new Color(0, 0, 0, 0.2f);

        Gizmos.DrawCube(transform.position, gizmoSquareSize);
    }

    public static bool CanMoveKnight(Vector2Int currentPosition, Vector2Int position)
    {
        Vector2Int positionDiff = position - currentPosition;

        positionDiff.x = Mathf.Abs(positionDiff.x);
        positionDiff.y = Mathf.Abs(positionDiff.y);

        return (positionDiff.x == 1 && positionDiff.y == 2) ||
            (positionDiff.x == 2 && positionDiff.y == 1);
    }
    public static bool CanMoveBishop(Vector2Int currentPosition, Vector2Int position)
    {
        Vector2Int positionDiff = position - currentPosition;

        positionDiff.x = Mathf.Abs(positionDiff.x);
        positionDiff.y = Mathf.Abs(positionDiff.y);

        return positionDiff.x == positionDiff.y && positionDiff.x + positionDiff.y > 0;
    }

    public static bool CanMoveRook(Vector2Int currentPosition, Vector2Int position)
    {
        Vector2Int positionDiff = position - currentPosition;

        positionDiff.x = Mathf.Abs(positionDiff.x);
        positionDiff.y = Mathf.Abs(positionDiff.y);

        return (positionDiff.x == 0 || positionDiff.y == 0) && positionDiff.x + positionDiff.y > 0;
    }

    public static bool CanMoveQueen(Vector2Int currentPosition, Vector2Int position)
    {
        return CanMoveRook(currentPosition, position) || CanMoveBishop(currentPosition, position);
    }

    public static bool CanMoveKing(Vector2Int currentPosition, Vector2Int position)
    {
        Vector2Int positionDiff = position - currentPosition;

        positionDiff.x = Mathf.Abs(positionDiff.x);
        positionDiff.y = Mathf.Abs(positionDiff.y);


        return positionDiff.x < 2 && positionDiff.y < 2 && positionDiff.x + positionDiff.y > 0; //&& CanMoveQueen(piece, boardState, position);
    }
    private static IEnumerable<(Vector2Int, bool)> LinearEnumeration(Vector2Int currentPosition, GameBoard boardState, bool skipBlocked, Vector2Int direction, int? maxDistance)
    {
        maxDistance = maxDistance ?? int.MaxValue;
        Vector2Int position = currentPosition + direction;
        bool blocked = false;
        int distance = 0;
        while (boardState.InRange(position) && distance < maxDistance.Value && !(blocked && skipBlocked))
        {
            ChessPiece occupiedPiece = boardState.GetPiece(position);
            yield return (position, blocked);

            //Hard coding the ball being ignored for block piecing for now, as there
            //hasn't been a need to ignore anything else
            blocked = blocked || (occupiedPiece && occupiedPiece.type != ChessType.Ball);
            position += direction;
            distance++;
        }

        yield break;
    }

    private static IEnumerable<(Vector2Int, bool)> StraightEnumeration(Vector2Int currentPosition, GameBoard boardState, bool skipBlocked, Vector2Int? direction, int? maxDistance)
    {
        Vector2Int[] directions =
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
        };

        if (direction.HasValue)
        {
            return directions.Contains(direction.Value)
                ? LinearEnumeration(currentPosition, boardState, skipBlocked, direction.Value, maxDistance)
                : LinearEnumeration(currentPosition, boardState, skipBlocked, direction.Value, 0);
        }
        else
        {
            return directions.SelectMany(dir => LinearEnumeration(currentPosition, boardState, skipBlocked, dir, maxDistance));
        }
    }

    private static IEnumerable<(Vector2Int, bool)> DiagonalEnumeration(Vector2Int currentPosition, GameBoard boardState, bool skipBlocked, Vector2Int? direction, int? maxDistance)
    {
        Vector2Int[] directions =
        {
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1),
        };

        if (direction.HasValue)
        {
            return directions.Contains(direction.Value)
                ? LinearEnumeration(currentPosition, boardState, skipBlocked, direction.Value, maxDistance)
                : LinearEnumeration(currentPosition, boardState, skipBlocked, direction.Value, 0);
        }
        else
        {
            return directions.SelectMany(dir => LinearEnumeration(currentPosition, boardState, skipBlocked, dir, maxDistance));
        }
    }
    private static IEnumerable<(Vector2Int, bool)> KnightEnumeration(Vector2Int currentPosition, GameBoard boardState, bool skipBlocked, Vector2Int? direction)
    {
        Vector2Int[] offsets = 
        { 
            new Vector2Int(1, -2), 
            new Vector2Int(1, 2), 
            new Vector2Int(-1, -2), 
            new Vector2Int(-1, 2), 
            new Vector2Int(2, -1), 
            new Vector2Int(2, 1),
            new Vector2Int(-2, -1),
            new Vector2Int(-2, 1),
        };

        foreach (var offset in offsets)
        {
            Vector2Int position = currentPosition + offset;
            if (boardState.InRange(position))
            {
                yield return (position, false);
            }
        }
    }
}
