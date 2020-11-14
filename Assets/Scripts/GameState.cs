using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState instance { get; private set; }
    public GameSubState currentSubState;

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

    public int goalSize = 3;

    public int playerOneScore { get; private set; }
    public int playerTwoScore { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        RegisterPieces();
        currentPlayerTurn = 1;
        currentSubState = new ReturnPiecesState();
    }

    private void Update()
    {
        //TrackMouse();

        currentSubState.Update();

        //if(Input.GetMouseButtonDown(0))
        //    RaycastBoardTarget();
        //if(Input.GetKeyDown(KeyCode.R))
        //{
        //    ResetBoard();
        //}
    }

    public Vector2Int RaycastToBoardPosition()
    {
        Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        //Convert the world space mouse position to a board position
        return GameBoard.WorldPositionToBoardPosition(gameBoard, worldMousePos);
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

    public bool ValidPlayerPosition(Vector2Int targetPos)
    {
        return (targetPos.x > 0 && targetPos.x < gameBoard.boardSize.x - 1) &&
         (targetPos.y >= 0 && targetPos.y < gameBoard.boardSize.y);
    }

    public bool ValidBallPosition(Vector2Int targetPos)
    {
        if((targetPos.x == 0 || targetPos.x == gameBoard.boardSize.x - 1) &&
         (targetPos.y >= 0 && targetPos.y < gameBoard.boardSize.y))
        {
            return IsInGoal(targetPos);
        }
        return ValidPlayerPosition(targetPos);

    }

    public bool IsInGoal(Vector2Int targetPos)
    {
        int startingGoalHeight = gameBoard.boardSize.y / 2 - goalSize / 2;

        return targetPos.y >= startingGoalHeight && targetPos.y < startingGoalHeight + goalSize && (targetPos.x == 0 || targetPos.x == gameBoard.boardSize.x - 1);
    }

    public void HandleGoal()
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

        currentSubState = new ReturnPiecesState();
    }

}

public abstract class GameSubState
{
    protected GameState gameState { get; private set; }
    public GameSubState()
    {
        gameState = GameState.instance;
    }
    public abstract void Update();
}

class PlayerMoveInputState : GameSubState
{

    const float outlineWidth = 0.03f;
    Color overlap = Color.white;
    Color select = Color.red;

    ChessPiece m_overlappedPiece;
    ChessPiece m_selectedPiece;


    public override void Update()
    {
        TrackMouse();
        if(Input.GetMouseButtonDown(0))
            HandleClick();
    }


    void TrackMouse()
    {
        ChessPiece piece = gameState.gameBoard.GetBoardPieceAt(gameState.RaycastToBoardPosition()) as ChessPiece;

        if(piece)
        {
            if(piece.team != gameState.currentPlayerTurn)
                return;

            if(m_overlappedPiece == null && piece != m_selectedPiece)
            {
                m_overlappedPiece = piece;
                ShowOutline(m_overlappedPiece, overlap);
            }
            else if(piece != m_selectedPiece)
            {
                HideOutline(m_overlappedPiece);

                m_overlappedPiece = piece;
                ShowOutline(m_overlappedPiece, overlap);
            }
            else
            {
                if(m_overlappedPiece)
                {
                    HideOutline(m_overlappedPiece);
                    m_overlappedPiece = null;
                }
            }
        }
        else
        {
            if(m_overlappedPiece)
            {
                HideOutline(m_overlappedPiece);
                m_overlappedPiece = null;
            }
        }
    }

    void HandleClick()
    {
        if(m_overlappedPiece)
        {
            if(m_selectedPiece)
                HideOutline(m_selectedPiece);

            m_selectedPiece = m_overlappedPiece;
            ShowOutline(m_selectedPiece, select);
            m_overlappedPiece = null;
        }
        else if(m_selectedPiece)
        {
            Vector2Int selectedBoardPosition = gameState.RaycastToBoardPosition();
            if(gameState.ValidPlayerPosition(selectedBoardPosition))
            {
                //if the move is invalid, or have selected our current position, do nothing and deselect our selected piece
                if(m_selectedPiece.position != selectedBoardPosition && m_selectedPiece.CanMove(gameState.gameBoard, selectedBoardPosition))
                {
                    List<BoardPiece> collidedPieces = m_selectedPiece.ProjectMovement(gameState.gameBoard, selectedBoardPosition);

                    if(collidedPieces.Count == 0)
                    {
                        gameState.currentSubState = new PlayerMoveState(m_selectedPiece, selectedBoardPosition);
                        HideOutline(m_selectedPiece);
                    }
                    else if(collidedPieces[0].team != m_selectedPiece.team && collidedPieces[0].position == selectedBoardPosition)
                    {
                        gameState.currentSubState = new PlayerMoveState(m_selectedPiece, selectedBoardPosition);
                        HideOutline(m_selectedPiece);
                    }
                }
                else
                {
                    HideOutline(m_selectedPiece);
                    m_selectedPiece = null;
                }
            }
            else
            {
                HideOutline(m_selectedPiece);
                m_selectedPiece = null;
            }
        }
    }

    void HideOutline(ChessPiece piece)
    {
        piece.spriteRenderer.material.SetFloat("_OutlineWidth", 0);
    }

    void ShowOutline(ChessPiece piece, Color color)
    {
        piece.spriteRenderer.material.SetFloat("_OutlineWidth", outlineWidth);
        piece.spriteRenderer.material.SetColor("_OutlineColor", color);
    }
}


class PlayerMoveState : GameSubState
{
    ChessPiece m_possessingPiece;
    Vector2Int m_targetBoardPos;

    Vector2 m_targetWorldPos;
    public PlayerMoveState(ChessPiece possessingPiece, Vector2Int targetBoardPos)
    {
        m_possessingPiece = possessingPiece;
        m_targetBoardPos = targetBoardPos;

        m_targetWorldPos = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, targetBoardPos);
    }

    public override void Update()
    {
        MoveUnit();
    }

    void MoveUnit()
    {
        m_possessingPiece.transform.position = m_targetWorldPos;
        if(SoundManager.Instance)
        {
            SoundManager.Instance.RandomSoundEffect(gameState.audioClipWalkingArray);
        }
        if((Vector2)m_possessingPiece.transform.position == m_targetWorldPos)
        {
            HandleMoveEnd();
        }
    }

    void HandleMoveEnd()
    {
        Vector2Int ballBoardPos = GameBoard.WorldPositionToBoardPosition(gameState.gameBoard, gameState.soccerPiece.transform.position);
        ChessPiece chessPiece = gameState.gameBoard.GetBoardPieceAt(m_targetBoardPos) as ChessPiece;

        if(ballBoardPos == m_targetBoardPos)
        {
            gameState.currentSubState = new BallMoveInputState(m_possessingPiece);
        }
        else if(chessPiece)
        {
            //Add piece to the remove list

            gameState.gameBoard.RemovePiece(chessPiece);
            gameState.currentSubState = new ReturnPiecesState();
        }
        else
        {
            gameState.currentSubState = new ReturnPiecesState();
        }

        gameState.gameBoard.MovePiece(m_possessingPiece, m_targetBoardPos);
    }
}

class BallMoveInputState : GameSubState
{
    ChessPiece m_possessingPiece;
    Color ballPossession = Color.blue;

    const float outlineWidth = 0.03f;

    public BallMoveInputState(ChessPiece possessingPiece)
    {
        m_possessingPiece = possessingPiece;

        m_possessingPiece.spriteRenderer.material.SetFloat("_OutlineWidth", outlineWidth);
        m_possessingPiece.spriteRenderer.material.SetColor("_OutlineColor", ballPossession);
    }

    public override void Update()
    {
        if(Input.GetMouseButtonDown(0))
            HandleClick();
    }

    void HandleClick()
    {
        Vector2Int selectedBoardPosition = gameState.RaycastToBoardPosition();
        if(m_possessingPiece.CanMove(gameState.gameBoard, selectedBoardPosition) && gameState.ValidBallPosition(selectedBoardPosition))
        {
            List<BoardPiece> collidedPieces = m_possessingPiece.ProjectMovement(gameState.gameBoard, selectedBoardPosition);
            if(collidedPieces.Count == 0)
            {
                m_possessingPiece.spriteRenderer.material.SetFloat("_OutlineWidth", 0);
                gameState.currentSubState = new BallMoveState(selectedBoardPosition);

                if(SoundManager.Instance)
                {
                    SoundManager.Instance.RandomSoundEffect(gameState.audioClipKickingArray);
                }
            }
            else
            {
                m_possessingPiece.spriteRenderer.material.SetFloat("_OutlineWidth", 0);
                gameState.currentSubState = new BallMoveState(selectedBoardPosition);

                if(SoundManager.Instance)
                {
                    SoundManager.Instance.RandomSoundEffect(gameState.audioClipKickingArray);
                }
            }
        }
    }
}

class BallMoveState : GameSubState
{
    Vector2Int m_targetBoardPos;

    Vector2 m_targetWorldPos;
    public BallMoveState(Vector2Int targetBoardPos)
    {
        m_targetBoardPos = targetBoardPos;

        m_targetWorldPos = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, targetBoardPos);
    }

    public override void Update()
    {
        MoveBall();
    }

    void MoveBall()
    {
        gameState.soccerPiece.transform.position = m_targetWorldPos;
        if((Vector2)gameState.soccerPiece.transform.position == m_targetWorldPos)
        {
            HandleMoveEnd();
        }
    }

    void HandleMoveEnd()
    {
        ChessPiece chessPiece = gameState.gameBoard.GetBoardPieceAt(m_targetBoardPos) as ChessPiece;

        if(chessPiece)
        {
            gameState.currentSubState = new BallMoveInputState(chessPiece);
        }
        else if(gameState.IsInGoal(m_targetBoardPos))
        {
            gameState.currentSubState = new HandleGoalState();
        }
        else
        {
            gameState.currentSubState = new ReturnPiecesState();
        }
    }
}

class HandleGoalState : GameSubState
{
    public override void Update()
    {
        gameState.HandleGoal();
    }
}

class ReturnPiecesState : GameSubState
{
    public ReturnPiecesState()
    {
        gameState.currentPlayerTurn = (gameState.currentPlayerTurn + 1) % 2;
    }

    public override void Update()
    {
        gameState.currentSubState = new PlayerMoveInputState();
    }
}