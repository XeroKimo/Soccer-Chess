using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ChessType
{
    Pawn,
    Knight,
    Bishop,
    Rook,
    Queen,
    King,
}

public class ChessPiece : BoardPiece
{
    public ChessType type;
    public delegate bool MoveRestriction(ChessPiece piece, Vector2Int position);
    public MoveRestriction CanMove;

    // Start is called before the first frame update
    void Start()
    {
        switch(type)
        {
        case ChessType.Pawn:
            break;
        case ChessType.Knight:
            break;
        case ChessType.Bishop:
            break;
        case ChessType.Rook:
            CanMove = RookMovement;
            break;
        case ChessType.Queen:
            break;
        case ChessType.King:
            break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        if(raycastCollider)
            Gizmos.DrawWireCube(transform.position, raycastCollider.size);
    }

    public static bool RookMovement(ChessPiece piece, Vector2Int position)
    {
        Vector2Int positionDiff = position - piece.position;

        return positionDiff.x == 0 || positionDiff.y == 0;
    }

}
