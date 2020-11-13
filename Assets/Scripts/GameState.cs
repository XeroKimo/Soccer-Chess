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
                if(selectedPiece.position != selectedBoardPosition && selectedPiece.CanMove(selectedPiece, selectedBoardPosition))
                    gameBoard.MovePiece(selectedPiece, selectedBoardPosition);
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
