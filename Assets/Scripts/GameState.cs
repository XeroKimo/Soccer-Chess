using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PieceMovement
{
    public BoardPiece piece;
    public Vector2 targetWorldPos;

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
    public AudioClip Goal;
    public AudioClip End;
    public AudioClip[] audioClipWalkingArray;
    public AudioClip[] audioClipKickingArray;

    public ChessPiece[] playerOnePieces;
    public ChessPiece[] playerTwoPieces;
    public SoccerPiece soccerPiece;
    public int currentPlayerTurn = 0;

    bool isBallPossessed;

    public int goalSize = 3;

    public int playerOneScore { get; private set; }
    public int playerTwoScore { get; private set; }


    const float outlineWidth = 0.03f;
    Color overlap = Color.white;
    Color select = Color.red;
    Color ballPossession = Color.blue;


    //The overlapped piece of the mouse's position
    ChessPiece m_overlappedPiece;
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
        TrackMouse();

        if(Input.GetMouseButtonDown(0))
            RaycastBoardTarget();
        if(Input.GetKeyDown(KeyCode.R))
        {
            ResetBoard();
        }
    }

    private void FixedUpdate()
    {
    }

    Vector2Int RaycastToBoard()
    {
        Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        //Convert the world space mouse position to a board position
        return GameBoard.WorldPositionToBoardPosition(gameBoard, worldMousePos);
    }

    void TrackMouse()
    {
        ChessPiece piece = gameBoard.GetBoardPieceAt(RaycastToBoard()) as ChessPiece;

        if(piece)
        {
            if(piece.team != currentPlayerTurn)
                return;
            if(m_overlappedPiece == null && piece != selectedPiece)
            {
                m_overlappedPiece = piece;
                m_overlappedPiece.spriteRenderer.material.SetFloat("_OutlineWidth", outlineWidth);
                m_overlappedPiece.spriteRenderer.material.SetColor("_OutlineColor", overlap);
            }
            else if(piece != selectedPiece)
            {
                m_overlappedPiece.spriteRenderer.material.SetFloat("_OutlineWidth", 0);

                m_overlappedPiece = piece;
                m_overlappedPiece.spriteRenderer.material.SetFloat("_OutlineWidth", outlineWidth);
                m_overlappedPiece.spriteRenderer.material.SetColor("_OutlineColor", overlap);

            }
        }
        else
        {
            if(m_overlappedPiece)
            {
                m_overlappedPiece.spriteRenderer.material.SetFloat("_OutlineWidth", 0);
            }
        }

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
        if(isBallPossessed)
        {
            if(selectedPiece.CanMove(gameBoard, selectedBoardPosition) && ValidBallPosition(selectedBoardPosition))
            {
                List<BoardPiece> collidedPieces = selectedPiece.ProjectMovement(gameBoard, selectedBoardPosition);
                if(collidedPieces.Count == 0)
                {
                    isBallPossessed = false;
                    soccerPiece.transform.position = GameBoard.BoardPositionToWorldPosition(gameBoard, selectedBoardPosition);
                    selectedPiece.spriteRenderer.material.SetFloat("_OutlineWidth", 0);
                    selectedPiece = null;
                    if(IsInGoal(selectedBoardPosition))
                    {
                        if(SoundManager.Instance)
                        {
                            SoundManager.Instance.Play(Goal);
                        }
                        HandleGoal();
                    }
                    else
                    {
                        currentPlayerTurn = (currentPlayerTurn + 1) % 2;
                    }
                }
                else
                {
                    if (SoundManager.Instance)
                    {
                        SoundManager.Instance.RandomSoundEffect(audioClipKickingArray);
                    }
                    selectedPiece.spriteRenderer.material.SetFloat("_OutlineWidth", 0);
                    soccerPiece.transform.position = GameBoard.BoardPositionToWorldPosition(gameBoard, collidedPieces[0].position);
                    selectedPiece = collidedPieces[0] as ChessPiece;
                    selectedPiece.spriteRenderer.material.SetFloat("_OutlineWidth", outlineWidth);
                    selectedPiece.spriteRenderer.material.SetColor("_OutlineColor", ballPossession);
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
        else if(!gameBoard.IsInBoardRange(selectedBoardPosition))
        {
            if(selectedPiece)
            {
                selectedPiece.spriteRenderer.material.SetFloat("_OutlineWidth", 0);
            }
            selectedPiece = null;
        }
        else
        {
            if(m_overlappedPiece)
            {
                if(selectedPiece)
                {
                    selectedPiece.spriteRenderer.material.SetFloat("_OutlineWidth", 0);
                }

                selectedPiece = m_overlappedPiece;
                selectedPiece.spriteRenderer.material.SetFloat("_OutlineWidth", outlineWidth);
                selectedPiece.spriteRenderer.material.SetColor("_OutlineColor", select);
                m_overlappedPiece = null;
            }
            //If we've selected a piece, the next input will determine what action will take,
            else if(selectedPiece)
            {
                if(ValidPlayerPosition(selectedBoardPosition))
                {
                    //if the move is invalid, or have selected our current position, do nothing and deselect our selected piece
                    if(selectedPiece.position != selectedBoardPosition && selectedPiece.CanMove(gameBoard, selectedBoardPosition))
                    {
                        List<BoardPiece> collidedPieces = selectedPiece.ProjectMovement(gameBoard, selectedBoardPosition);

                        if(collidedPieces.Count == 0)
                        {
                            gameBoard.MovePiece(selectedPiece, selectedBoardPosition);

                            MovePieceWorldPos(selectedPiece, selectedBoardPosition);
                            if(selectedPiece.position == GameBoard.WorldPositionToBoardPosition(gameBoard, soccerPiece.transform.position))
                            {
                                isBallPossessed = true;
                                selectedPiece.spriteRenderer.material.SetColor("_OutlineColor", ballPossession);
                            }
                            if(!isBallPossessed)
                                currentPlayerTurn = (currentPlayerTurn + 1) % 2;
                        }
                        else if(collidedPieces[0].team != selectedPiece.team)
                        {

                            gameBoard.RemovePiece(collidedPieces[0]);
                            gameBoard.MovePiece(selectedPiece, selectedBoardPosition);

                            MovePieceWorldPos(selectedPiece, selectedBoardPosition);
                            currentPlayerTurn = (currentPlayerTurn + 1) % 2;
                        }
                    }
                }
                if(!isBallPossessed)
                {
                    selectedPiece.spriteRenderer.material.SetFloat("_OutlineWidth", 0);
                    selectedPiece = null;

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


        foreach(ChessPiece piece in playerOnePieces)
        {
            gameBoard.PlacePiece(piece, piece.initialPosition);
        }
        foreach(ChessPiece piece in playerTwoPieces)
        {
            gameBoard.PlacePiece(piece, piece.initialPosition);
        }

        soccerPiece.transform.position = GameBoard.BoardPositionToWorldPosition(gameBoard, soccerPiece.initialPosition);
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
        return ValidPlayerPosition(targetPos);

    }

    bool IsInGoal(Vector2Int targetPos)
    {
        int startingGoalHeight = gameBoard.boardSize.y / 2 - goalSize / 2;

        return targetPos.y >= startingGoalHeight && targetPos.y < startingGoalHeight + goalSize && (targetPos.x == 0 || targetPos.x == gameBoard.boardSize.x - 1);
    }

    void HandleGoal()
    {
        ResetBoard();
        if(currentPlayerTurn == 0)
        {
            playerOneScore++;
        }
        else
        {
            playerTwoScore++;
        }
    }

    void MovePieceWorldPos(ChessPiece piece, Vector2Int selectedBoardPosition)
    {
        if (SoundManager.Instance)
        {
            SoundManager.Instance.RandomSoundEffect(audioClipWalkingArray);
        }
        Vector3 newWorldPos = GameBoard.BoardPositionToWorldPosition(gameBoard, selectedBoardPosition);
        newWorldPos.z = -newWorldPos.y;
        selectedPiece.transform.position = newWorldPos;
    }
}
