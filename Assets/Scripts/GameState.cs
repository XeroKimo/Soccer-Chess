using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState instance { get; private set; }
    public GameBoard gameBoard;
    public ChessPiece selectedPiece;
    public Camera mainCamera;

    public int currentPlayerTurn = 0;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
            RaycastBoardTarget();
    }

    public void RaycastBoardTarget()
    {
        Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        Vector2Int selectedBoardPosition = GameBoard.WorldPositionToBoardPosition(gameBoard, worldMousePos);
        if(!gameBoard.IsInBoardRange(selectedBoardPosition))
        {
            selectedPiece = null;
        }
        else
        {
            if(selectedPiece)
            {
                if(selectedPiece.position != selectedBoardPosition && selectedPiece.CanMove(gameBoard, selectedBoardPosition))
                {
                    BoardPiece piece = selectedPiece.ProjectMovement(gameBoard, selectedBoardPosition);

                    //ChessPiece chessPiece = piece as ChessPiece;
                    //if(chessPiece != null)
                    //{
                    //    if(chessPiece
                    //}
                    //If the piece is a chess piece and it is an enemy
                    //Check if the selected position == enemy position
                    //If true, consume the enemy
                    //If the piece is an ally piece, do not move
                    //If the piece is the soccer ball, move the ball

                    gameBoard.MovePiece(selectedPiece, selectedBoardPosition);
                }
                selectedPiece = null;
            }
            else
            {

                selectedPiece = gameBoard.GetBoardPieceAt(selectedBoardPosition) as ChessPiece;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if(selectedPiece)
            Gizmos.DrawCube(GameBoard.BoardPositionToWorldPosition(gameBoard, selectedPiece.position), gameBoard.cellSize);
    }
}
