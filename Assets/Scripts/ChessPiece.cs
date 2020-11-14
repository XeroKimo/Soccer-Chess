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
    public delegate List<BoardPiece> RaycastMovement(ChessPiece piece, GameBoard boardState, Vector2Int targetPosition);

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

    public List<BoardPiece> ProjectMovement(GameBoard boardState, Vector2Int position)
    {
        return RaycastMovementDelegate(this, boardState, position);
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

    public static List<BoardPiece> LinearRaycast(ChessPiece piece, GameBoard boardState, Vector2Int targetPosition)
    {
        Vector2Int direction = targetPosition - piece.position;

        direction.x = Mathf.Clamp(direction.x, -1, 1);
        direction.y = Mathf.Clamp(direction.y, -1, 1);

        Vector2Int currentPos = piece.position;

        List<BoardPiece> collidedPieces = new List<BoardPiece>(5);
        do
        {
            currentPos += direction;

            BoardPiece checkPieceAt = boardState.GetBoardPieceAt(currentPos);
            if(checkPieceAt != null)
            {
                collidedPieces.Add(checkPieceAt);
            }

        } while(currentPos != targetPosition);



        return collidedPieces;
    }


    public static List<BoardPiece> KnightRaycast(ChessPiece piece, GameBoard boardState, Vector2Int targetPosition)
    {
        Vector2Int direction = targetPosition - piece.position;

        Debug.Assert((Mathf.Abs(direction.x) == 1 && Mathf.Abs(direction.y) == 2) ||
            (Mathf.Abs(direction.y) == 1 && Mathf.Abs(direction.x) == 2),
            "The movement does not follow rules of a knight, this function will not work like you would expect\n," +
            "Please check if the piece can move first before raycasting");

        Vector2Int currentPosition = piece.position;
        Vector2Int currentPosition2 = piece.position;
        currentPosition2 += (direction.x == 1) ? new Vector2Int(Mathf.Clamp(direction.x, -1, 1), 0) : new Vector2Int(0, Mathf.Clamp(direction.y, -1, 1));

        direction -= currentPosition2 - currentPosition;
        direction.x = Mathf.Clamp(direction.x, -1, 1);
        direction.y = Mathf.Clamp(direction.y, -1, 1);

        List<BoardPiece> collidedPieces = new List<BoardPiece>(5);

        BoardPiece checkPieceAt = boardState.GetBoardPieceAt(currentPosition2);

        if(checkPieceAt != null)
            collidedPieces.Add(checkPieceAt);
        do
        {
            currentPosition += direction;
            currentPosition2 += direction;

            checkPieceAt = boardState.GetBoardPieceAt(currentPosition);
            if(checkPieceAt != null)
                collidedPieces.Add(checkPieceAt);

            checkPieceAt = boardState.GetBoardPieceAt(currentPosition2);
            if(checkPieceAt != null)
                collidedPieces.Add(checkPieceAt);

            if(!boardState.IsInBoardRange(currentPosition) || !boardState.IsInBoardRange(currentPosition2))
            {
                Debug.Log("CurrentPos: " + currentPosition + "\nCurrentPos2: " +  
                    currentPosition2 + "\nDirection: " + direction+
                    "\n OriginalPos: " + piece.position + "\nTargetPos: " + targetPosition);
                break;
            }

        } while(currentPosition2 != targetPosition);

        return collidedPieces;
    }
}
