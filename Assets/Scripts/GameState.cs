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
        Debug.Log(worldMousePos);
        if(!gameBoard.InBoardRangeWorldSpace(worldMousePos))
        {
            selectedPiece = null;
        }
        else
        {
            Debug.Log(GameBoard.WorldPositionToBoardPosition(gameBoard, worldMousePos));
            selectedPiece = gameBoard.GetBoardPieceAt(GameBoard.WorldPositionToBoardPosition(gameBoard, worldMousePos)) as ChessPiece;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if(selectedPiece)
            Gizmos.DrawCube(GameBoard.BoardPositionToWorldPosition(gameBoard, selectedPiece.position), gameBoard.cellSize);
    }
}
