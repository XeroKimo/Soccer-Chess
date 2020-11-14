using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

    public void RaycastBoardTarget()
    {
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
            //If we've selected a piece, the next input will determine what action will take,
            if(selectedPiece)
            {
                //if the move is invalid, or have selected our current position, do nothing and deselect our selected piece
                if(selectedPiece.position != selectedBoardPosition && selectedPiece.CanMove(gameBoard, selectedBoardPosition))
                {
                    //Project our movement to see if we would collide with anything on the way to our target destination
                    List<BoardPiece> collidedPieces = selectedPiece.ProjectMovement(gameBoard, selectedBoardPosition);

                    //If we didn't collide with anything, it is safe to think we can just move to our position
                    if (collidedPieces.Count == 0)
                    {
                        //Move player
                        selectedPiece.transform.position = GameBoard.BoardPositionToWorldPosition(gameBoard, selectedBoardPosition);
                        gameBoard.MovePiece(selectedPiece, selectedBoardPosition);

                        currentPlayerTurn = (currentPlayerTurn + 1) % 2;
                    }
                    //If we collided with something as a knight, it will be handle differently compared to other pieces
                    else if(selectedPiece.type == ChessType.Knight)
                    {
                        BoardPiece endPoint = gameBoard.GetBoardPieceAt(selectedBoardPosition);

                        bool canMove = true;
                        bool didCollideWithBall = false;
                        bool targetAtEnd = false;
                        //If the end point is occupied by an ally, we don't have to check anything
                        if(endPoint != null)
                        {
                            if(endPoint is ChessPiece && endPoint.team == selectedPiece.team)
                            {
                                canMove = false;
                            }
                            else
                            {
                                targetAtEnd = true;
                            }
                        }

                        foreach(BoardPiece piece in collidedPieces)
                        {
                            if(piece == soccerPiece)
                            {
                                didCollideWithBall = true;
                            }
                        }

                        if(canMove)
                        {
                            if(didCollideWithBall)
                            {
                                Vector2Int direction = selectedBoardPosition - selectedPiece.position;

                                soccerPiece.transform.position = GameBoard.BoardPositionToWorldPosition(gameBoard, soccerPiece.position + direction);
                                gameBoard.MovePiece(soccerPiece, soccerPiece.position + direction);
                            }
                            if(targetAtEnd)
                            {
                                gameBoard.RemovePiece(endPoint);
                            }
                            selectedPiece.transform.position = GameBoard.BoardPositionToWorldPosition(gameBoard, selectedBoardPosition);
                            gameBoard.MovePiece(selectedPiece, selectedBoardPosition);
                            currentPlayerTurn = (currentPlayerTurn + 1) % 2;
                        }
                    }
                    else
                    {
                        //Get the first object we collided with
                        BoardPiece firstCollision = collidedPieces[0];

                        //Only do stuff if our first collision is our intended position
                        if(firstCollision.position == selectedBoardPosition)
                        {
                            //Check to see if our first collision is a chess piece and is not from the same team
                            if(firstCollision is ChessPiece && firstCollision.team != selectedPiece.team)
                            {
                                //Capture piece if it holds true
                                gameBoard.RemovePiece(firstCollision);

                                //Move player
                                selectedPiece.transform.position = GameBoard.BoardPositionToWorldPosition(gameBoard, selectedBoardPosition);
                                gameBoard.MovePiece(selectedPiece, selectedBoardPosition);
                                currentPlayerTurn = (currentPlayerTurn + 1) % 2;
                            }
                            else if(firstCollision == soccerPiece)
                            {
                                Vector2Int direction = selectedBoardPosition - selectedPiece.position;
                                Vector2Int stopPos = new Vector2Int(Mathf.Clamp(direction.x, -1, 1), Mathf.Clamp(direction.y, -1, 1));
                                
                                //Move ball
                                soccerPiece.transform.position = GameBoard.BoardPositionToWorldPosition(gameBoard, soccerPiece.position + direction);
                                gameBoard.MovePiece(soccerPiece, selectedBoardPosition + direction);

                                //Move player
                                selectedPiece.transform.position = GameBoard.BoardPositionToWorldPosition(gameBoard, selectedBoardPosition - stopPos);
                                gameBoard.MovePiece(selectedPiece, selectedBoardPosition - stopPos);
                                currentPlayerTurn = (currentPlayerTurn + 1) % 2;
                            }
                        }
                    }
                }
                selectedPiece = null;
            }

            //else check to see if the selected board position corresponds to a chess piece for the given player turn.
            //reset the piece to null if we don't own that piece
            else
            {
                //Ch
                selectedPiece = gameBoard.GetBoardPieceAt(selectedBoardPosition) as ChessPiece;
                if(selectedPiece)
                {
                    selectedPiece = (selectedPiece.team == currentPlayerTurn) ? selectedPiece : null;
                }
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

        gameBoard.RegisterPiece(soccerPiece, GameBoard.WorldPositionToBoardPosition(gameBoard, soccerPiece.transform.position), 2);
    }

    void ResetBoard()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if(selectedPiece)
            Gizmos.DrawCube(GameBoard.BoardPositionToWorldPosition(gameBoard, selectedPiece.position), gameBoard.cellSize);
    }
}
