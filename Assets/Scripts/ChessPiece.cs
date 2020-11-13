using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChessType
{
    Knight,
    Bishop,
    Rook,
    Queen,
    King,
}

public class ChessPiece : BoardPiece
{
    public delegate bool MoveRestriction(ChessPiece piece, GameBoard boardState, Vector2Int position);
    public delegate BoardPiece RaycastMovement(ChessPiece piece, GameBoard boardState, Vector2Int targetPosition);

    public ChessType type;

    private MoveRestriction CanMoveDelegate;
    private RaycastMovement RaycastMovementDelegate;

    // Start is called before the first frame update
    void Start()
    {
        RaycastMovementDelegate = LinearRaycast;
        switch(type)
        {
        case ChessType.Knight:
            CanMoveDelegate = CanMoveKnight;
            RaycastMovementDelegate = KnightRaycast;
            break;
        case ChessType.Bishop:
            CanMoveDelegate = CanMoveBishop;
            break;
        case ChessType.Rook:
            CanMoveDelegate = CanMoveRook;
            break;
        case ChessType.Queen:
            CanMoveDelegate = CanMoveQueen;
            break;
        case ChessType.King:
            CanMoveDelegate = CanMoveKing;
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

    public BoardPiece ProjectMovement(GameBoard boardState, Vector2Int position)
    {
        return RaycastMovementDelegate(this, boardState, position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        if(raycastCollider)
            Gizmos.DrawWireCube(transform.position, raycastCollider.size);
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

    public static BoardPiece LinearRaycast(ChessPiece piece, GameBoard boardState, Vector2Int targetPosition)
    {
        Vector2Int direction = targetPosition - piece.position;

        direction.x = Mathf.Clamp(direction.x, -1, 1);
        direction.y = Mathf.Clamp(direction.y, -1, 1);

        Vector2Int currentPos = piece.position;

        do
        {
            currentPos += direction;

            BoardPiece checkPieceAt = boardState.GetBoardPieceAt(currentPos);
            if(checkPieceAt != null)
                return checkPieceAt;

        } while(currentPos != targetPosition);



        return null;
    }


    public static BoardPiece KnightRaycast(ChessPiece piece, GameBoard boardState, Vector2Int targetPosition)
    {
        Vector2Int direction = targetPosition - piece.position;

        Debug.Assert((Mathf.Abs(direction.x) == 1 && Mathf.Abs(direction.y) == 2 )|| 
            (Mathf.Abs(direction.y) == 1 && Mathf.Abs(direction.x) == 2),
            "The movement does not follow rules of a knight, this function will not work like you would expect\n," +
            "Please check if the piece can move first before raycasting");
        Vector2Int currentPosition = piece.position;
        Vector2Int currentPosition2 = piece.position;
        currentPosition2 += (direction.x == 1) ? new Vector2Int(1, 0) : new Vector2Int(0, 1);

        direction.x -= currentPosition2.x - currentPosition.x;

        BoardPiece checkPieceAt = boardState.GetBoardPieceAt(currentPosition2);
        if(checkPieceAt != null)
            return checkPieceAt;
        do
        {
            currentPosition += direction;
            currentPosition2 += direction;

            checkPieceAt = boardState.GetBoardPieceAt(currentPosition);
            if(checkPieceAt != null)
                return checkPieceAt;

            checkPieceAt = boardState.GetBoardPieceAt(currentPosition2);
            if(checkPieceAt != null)
                return checkPieceAt;

        } while(currentPosition2 != targetPosition);

        return null;
    }
}
