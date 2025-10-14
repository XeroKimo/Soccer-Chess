using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ChessType
{
    Knight,
    Bishop,
    Rook,
    Queen,
    King,
}

public struct EnumeratePositionOutput
{
    public Vector2Int position;
    public BoardPiece occupiedPiece;
    public bool blocked;
};

public class ChessPiece : BoardPiece
{
    public delegate bool MoveRestriction(ChessPiece piece, GameBoard boardState, Vector2Int position);

    public ChessType type;
    private MoveRestriction CanMoveDelegate;

    private delegate IEnumerable<EnumeratePositionOutput> EnumeratePositions(ChessPiece piece, GameBoard boardState, bool skipBlocked, Vector2Int? direction);
    private EnumeratePositions EnumeratePositionsDelegate;

    // Start is called before the first frame update
    void Start()
    {
        switch(type)
        {
        case ChessType.Knight:
            CanMoveDelegate = CanMoveKnight;
            EnumeratePositionsDelegate = KnightEnumeration;
            break;
        case ChessType.Bishop:
            CanMoveDelegate = CanMoveBishop;
                EnumeratePositionsDelegate = BishopEnumeration;
                break;
        case ChessType.Rook:
            CanMoveDelegate = CanMoveRook;
                EnumeratePositionsDelegate = RookEnumeration;
                break;
        case ChessType.Queen:
            CanMoveDelegate = CanMoveQueen;
                EnumeratePositionsDelegate = QueenEnumeration;
                break;
        case ChessType.King:
            CanMoveDelegate = CanMoveKing;
                EnumeratePositionsDelegate = KingEnumeration;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool CanMove(GameBoard boardState, Vector2Int position)
    {
        return CanMoveDelegate(this, boardState, position);
    }

    public IEnumerable<EnumeratePositionOutput> GetValidPositions(GameBoard boardState, bool skipBlocked, Vector2Int? direction = null)
    {
        return EnumeratePositionsDelegate(this, boardState, skipBlocked, direction);
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

    public static bool CanMoveKnight(ChessPiece piece, GameBoard boardState, Vector2Int position)
    {
        Vector2Int positionDiff = position - piece.position;

        positionDiff.x = Mathf.Abs(positionDiff.x);
        positionDiff.y = Mathf.Abs(positionDiff.y);

        return (positionDiff.x == 1 && positionDiff.y == 2) ||
            (positionDiff.x == 2 && positionDiff.y == 1);
    }
    public static bool CanMoveBishop(ChessPiece piece, GameBoard boardState, Vector2Int position)
    {
        Vector2Int positionDiff = position - piece.position;

        positionDiff.x = Mathf.Abs(positionDiff.x);
        positionDiff.y = Mathf.Abs(positionDiff.y);

        return positionDiff.x == positionDiff.y;
    }

    public static bool CanMoveRook(ChessPiece piece, GameBoard boardState, Vector2Int position)
    {
        Vector2Int positionDiff = position - piece.position;

        return positionDiff.x == 0 || positionDiff.y == 0;
    }

    public static bool CanMoveQueen(ChessPiece piece, GameBoard boardState, Vector2Int position)
    {
        return CanMoveRook(piece, boardState, position) || CanMoveBishop(piece, boardState, position);
    }

    public static bool CanMoveKing(ChessPiece piece, GameBoard boardState, Vector2Int position)
    {
        Vector2Int positionDiff = position - piece.position;

        positionDiff.x = Mathf.Abs(positionDiff.x);
        positionDiff.y = Mathf.Abs(positionDiff.y);


        return positionDiff.x < 2 && positionDiff.y < 2; //&& CanMoveQueen(piece, boardState, position);
    }
    private static IEnumerable<EnumeratePositionOutput> LinearEnumeration(ChessPiece piece, GameBoard boardState, bool skipBlocked, Vector2Int direction, int? maxDistance)
    {
        maxDistance = maxDistance ?? int.MaxValue;
        Vector2Int position = piece.position + direction;
        bool blocked = false;
        int distance = 0;
        while (boardState.IsInBoardRange(position) && distance < maxDistance.Value)
        {
            BoardPiece occupiedPiece = boardState.GetBoardPieceAt(position);
            yield return new EnumeratePositionOutput { position = position, occupiedPiece = occupiedPiece, blocked = blocked};
            if (occupiedPiece && skipBlocked)
                break;

            position += direction;
            distance++;
        }

        yield break;
    }

    private static IEnumerable<EnumeratePositionOutput> StraightEnumeration(ChessPiece piece, GameBoard boardState, bool skipBlocked, Vector2Int? direction, int? maxDistance)
    {
        Vector2Int[] directions =
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
        };

        if (direction.HasValue && directions.Contains(direction.Value))
        {
            foreach (var v in LinearEnumeration(piece, boardState, skipBlocked, direction.Value, maxDistance))
            {
                yield return v;
            }
        }
        else
        {
            foreach (var v in directions.SelectMany(dir => LinearEnumeration(piece, boardState, skipBlocked, dir, maxDistance)))
            {
                yield return v;
            }
        }
        yield break;
    }

    private static IEnumerable<EnumeratePositionOutput> DiagonalEnumeration(ChessPiece piece, GameBoard boardState, bool skipBlocked, Vector2Int? direction, int? maxDistance)
    {
        Vector2Int[] directions =
        {
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1),
        };

        if (direction.HasValue && directions.Contains(direction.Value))
        {
            foreach (var v in LinearEnumeration(piece, boardState, skipBlocked, direction.Value, maxDistance))
            {
                yield return v;
            }
        }
        else
        {
            foreach (var v in directions.SelectMany(dir => LinearEnumeration(piece, boardState, skipBlocked, dir, maxDistance)))
            {
                yield return v;
            }
        }
        yield break;
    }

    private static IEnumerable<EnumeratePositionOutput> BishopEnumeration(ChessPiece piece, GameBoard boardState, bool skipBlocked, Vector2Int? direction)
    {
        foreach(var v in DiagonalEnumeration(piece, boardState, skipBlocked, direction, null))
            yield return v;
        yield break;
    }

    private static IEnumerable<EnumeratePositionOutput> RookEnumeration(ChessPiece piece, GameBoard boardState, bool skipBlocked, Vector2Int? direction)
    {
        foreach (var v in StraightEnumeration(piece, boardState, skipBlocked, direction, null))
            yield return v;
        yield break;
    }

    private static IEnumerable<EnumeratePositionOutput> QueenEnumeration(ChessPiece piece, GameBoard boardState, bool skipBlocked, Vector2Int? direction)
    {
        foreach (var v in DiagonalEnumeration(piece, boardState, skipBlocked, direction, null)
            .Concat(StraightEnumeration(piece, boardState, skipBlocked, direction, null)))
        {
            yield return v;
        }
        yield break;
    }

    private static IEnumerable<EnumeratePositionOutput> KingEnumeration(ChessPiece piece, GameBoard boardState, bool skipBlocked, Vector2Int? direction)
    {
        foreach (var v in DiagonalEnumeration(piece, boardState, skipBlocked, direction, 1)
            .Concat(StraightEnumeration(piece, boardState, skipBlocked, direction, 1)))
        {
            yield return v;
        }
    }

    private static IEnumerable<EnumeratePositionOutput> KnightEnumeration(ChessPiece piece, GameBoard boardState, bool skipBlocked, Vector2Int? direction)
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
            Vector2Int position = piece.position + offset;
            if (boardState.IsInBoardRange(position))
            {
                BoardPiece occupiedPiece = boardState.GetBoardPieceAt(position);
                yield return new EnumeratePositionOutput { position = position, occupiedPiece = occupiedPiece, blocked = occupiedPiece };
            }
        }
        yield break;
    }
}
