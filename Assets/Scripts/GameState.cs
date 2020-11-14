using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PieceMovement
{
    public BoardPiece piece;
    public Vector2 targetWorldPos;
    public AudioSource audio;

    public Vector3 initialPos;
    public float time;
    public PieceMovement(BoardPiece piece, Vector2 targetWorldPos)
    {
        this.piece = piece;
        this.targetWorldPos = targetWorldPos;
        initialPos = piece.transform.position;
        time = 0;
        //audio = GetComponent<AudioSource>();

       // audio.Play();
    }
}

public class GameState : MonoBehaviour
{
    public static GameState instance { get; private set; }
    public GameBoard gameBoard;
    public ChessPiece selectedPiece;
    public Camera mainCamera;

    public ChessPiece[] playerOnePieces;
    public ChessPiece[] playerTwoPieces;
    public SoccerPiece soccerPiece;
    public int currentPlayerTurn = 0;

    bool isBallPossessed;

    public int goalSize = 3;


    //List<PieceMovement> pieceMovements = new List<PieceMovement>(2);

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        RegisterPieces();
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
            RaycastBoardTarget();
        if(Input.GetKeyDown(KeyCode.R))
        {
            ResetBoard();
        }
    }

    private void FixedUpdate()
    {
        //List<PieceMovement> thingsToUpdate = new List<PieceMovement>(pieceMovements);
        //foreach(PieceMovement movement in thingsToUpdate)
        //{
        //    movement.piece.gameObject.transform.position = Vector3.Lerp(movement.initialPos, movement.targetWorldPos, movement.time);

        //    if(movement.time >= 1)
        //    {
        //        gameBoard.MovePiece(movement.piece, GameBoard.WorldPositionToBoardPosition(gameBoard, movement.targetWorldPos));
        //        pieceMovements.Remove(movement);

        //        if(pieceMovements.Count == 0)
        //        {
        //            currentPlayerTurn = (currentPlayerTurn + 1) % 2;
        //        }
        //    }

        //    movement.time += Time.deltaTime;
        //}
        
    }

    public void RaycastBoardTarget()
    {
        //if(pieceMovements.Count > 0)
        //    return;
        //Convert our mouse position into world space
        Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        //Convert the world space mouse position to a board position
        Vector2Int selectedBoardPosition = GameBoard.WorldPositionToBoardPosition(gameBoard, worldMousePos);

        //If the selected board position is not in range, set the selected piece to null and do nothing,

        if(!gameBoard.IsInBoardRange(selectedBoardPosition))
        {
            selectedPiece = null;
        }
        else
        {
            if(!selectedPiece)
            {
                selectedPiece = gameBoard.GetBoardPieceAt(selectedBoardPosition) as ChessPiece;
                if(selectedPiece)
                {
                    selectedPiece = (selectedPiece.team == currentPlayerTurn) ? selectedPiece : null;
                }
            }
            //If we've selected a piece, the next input will determine what action will take,
            else
            {
                if(isBallPossessed)
                {
                    if(selectedPiece.CanMove(gameBoard, selectedBoardPosition) && ValidBallPosition(selectedBoardPosition))
                    {
                        List<BoardPiece> collidedPieces = selectedPiece.ProjectMovement(gameBoard, selectedBoardPosition);
                        if(collidedPieces.Count == 0)
                        {
                            isBallPossessed = false;
                            soccerPiece.transform.position = GameBoard.BoardPositionToWorldPosition(gameBoard, selectedBoardPosition);
                            selectedPiece = null;
                            if(IsInGoal(selectedBoardPosition))
                            {
                                HandleGoal();
                            }
                            else
                            {
                                currentPlayerTurn = (currentPlayerTurn + 1) % 2;
                            }
                        }
                        else
                        {
                            selectedPiece = collidedPieces[0] as ChessPiece;
                            //if(collidedPieces[0].team != selectedPiece.team)
                            //{
                            //    gameBoard.RemovePiece(collidedPieces[0]);
                            //    gameBoard.MovePiece(selectedPiece, selectedBoardPosition);

                            //    selectedPiece.transform.position = GameBoard.BoardPositionToWorldPosition(gameBoard, selectedBoardPosition);
                            //    currentPlayerTurn = (currentPlayerTurn + 1) % 2;
                            //}
                        }
                    }
                }
                else if(ValidPlayerPosition(selectedBoardPosition))
                {
                    //if the move is invalid, or have selected our current position, do nothing and deselect our selected piece
                    if(selectedPiece.position != selectedBoardPosition && selectedPiece.CanMove(gameBoard, selectedBoardPosition))
                    {
                        List<BoardPiece> collidedPieces = selectedPiece.ProjectMovement(gameBoard, selectedBoardPosition);

                        if(collidedPieces.Count == 0)
                        {
                            gameBoard.MovePiece(selectedPiece, selectedBoardPosition);

                            selectedPiece.transform.position = GameBoard.BoardPositionToWorldPosition(gameBoard, selectedBoardPosition);

                            if(selectedPiece.position == GameBoard.WorldPositionToBoardPosition(gameBoard, soccerPiece.transform.position))
                            {
                                isBallPossessed = true;
                            }
                            if(!isBallPossessed)
                                currentPlayerTurn = (currentPlayerTurn + 1) % 2;
                        }
                        else
                        {
                            if(collidedPieces[0].team != selectedPiece.team)
                            {
                                gameBoard.RemovePiece(collidedPieces[0]);
                                gameBoard.MovePiece(selectedPiece, selectedBoardPosition);

                                selectedPiece.transform.position = GameBoard.BoardPositionToWorldPosition(gameBoard, selectedBoardPosition);
                                currentPlayerTurn = (currentPlayerTurn + 1) % 2;
                            }
                        }
                    }
                }
                if(!isBallPossessed)
                    selectedPiece = null;
            }
        }
    }

    void RegisterPieces()
    {
        foreach(ChessPiece piece in playerOnePieces)
        {
            gameBoard.RegisterPiece(piece, GameBoard.WorldPositionToBoardPosition(gameBoard, piece.transform.position), 0);
        }
        foreach(ChessPiece piece in playerTwoPieces)
        {
            gameBoard.RegisterPiece(piece, GameBoard.WorldPositionToBoardPosition(gameBoard, piece.transform.position), 1);
        }

        //gameBoard.RegisterPiece(soccerPiece, GameBoard.WorldPositionToBoardPosition(gameBoard, soccerPiece.transform.position), 2);
        soccerPiece.initialPosition = GameBoard.WorldPositionToBoardPosition(gameBoard, soccerPiece.transform.position);
        soccerPiece.transform.position = GameBoard.BoardPositionToWorldPosition(gameBoard, soccerPiece.initialPosition);
    }

    void ResetBoard()
    {
        foreach(ChessPiece piece in playerOnePieces)
        {
            gameBoard.RemovePiece(piece);
        }
        foreach(ChessPiece piece in playerTwoPieces)
        {
            gameBoard.RemovePiece(piece);
        }

        gameBoard.RemovePiece(soccerPiece);

        foreach(ChessPiece piece in playerOnePieces)
        {
            gameBoard.PlacePiece(piece, piece.initialPosition);
        }
        foreach(ChessPiece piece in playerTwoPieces)
        {
            gameBoard.PlacePiece(piece, piece.initialPosition);
        }

        gameBoard.PlacePiece(soccerPiece, soccerPiece.initialPosition);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if(selectedPiece)
            Gizmos.DrawCube(GameBoard.BoardPositionToWorldPosition(gameBoard, selectedPiece.position), gameBoard.cellSize);
    }

    bool ValidPlayerPosition(Vector2Int targetPos)
    {
        return (targetPos.x > 0 && targetPos.x < gameBoard.boardSize.x - 1) &&
         (targetPos.y >= 0 && targetPos.y < gameBoard.boardSize.y);
    }

    bool ValidBallPosition(Vector2Int targetPos)
    {
        if((targetPos.x == 0 || targetPos.x == gameBoard.boardSize.x - 1) &&
         (targetPos.y >= 0 && targetPos.y < gameBoard.boardSize.y))
        {
            return IsInGoal(targetPos);
        }
        return true;

    }

    bool IsInGoal(Vector2Int targetPos)
    {
        int startingGoalHeight = gameBoard.boardSize.y / 2 - goalSize / 2;

        return targetPos.y >= startingGoalHeight && targetPos.y < startingGoalHeight + goalSize;
    }

    void HandleGoal()
    {
        ResetBoard();
    }
}
