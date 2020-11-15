using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
    public static GameState instance { get; private set; }
    public GameSubState currentSubState;

    public MovementIndicators movementIndicators;

    public GameBoard gameBoard;
    public ChessPiece selectedPiece;
    public Camera mainCamera;

    public AudioClip Goal;
    public AudioClip End;
    public AudioClip Hit;
    public AudioClip[] audioClipWalkingArray;
    public AudioClip[] audioClipKickingArray;

    public ChessPiece[] playerOnePieces;
    public ChessPiece[] playerTwoPieces;

    public CapturedField playerOneField;
    public CapturedField playerTwoField;

    public SoccerPiece soccerBall;

    public GameObject WinParticleObject;

    public Text blueScoreText;
    public Text redScoreText;

    public int currentPlayerTurn = 0;

    public int goalSize = 3;

    public int playerOneScore { get; private set; }
    public int playerTwoScore { get; private set; }

    public bool displayMoveIndicators = true;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        RegisterPieces();
        RestartGame();
    }

    private void Update()
    {
        //TrackMouse();

        if(!UIManager.Instance.GetPaused())
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
        soccerBall.initialPosition = GameBoard.WorldPositionToBoardPosition(gameBoard, soccerBall.transform.position);
        soccerBall.transform.position = GameBoard.BoardPositionToWorldPosition(gameBoard, soccerBall.initialPosition);
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

        soccerBall.transform.position = GameBoard.BoardPositionToWorldPosition(gameBoard, soccerBall.initialPosition);
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
            blueScoreText.text = playerOneScore.ToString();
        }
        else
        {
            playerTwoScore++;
            redScoreText.text = playerOneScore.ToString();
        }

        if(playerTwoScore >= 3 || playerOneScore >= 3)
        {
            if(SoundManager.Instance)
            {
                SoundManager.Instance.Play(End);
            }
            GameObject WinParticleGameObject = GameObject.Instantiate(WinParticleObject);
            GameObject.Destroy(WinParticleGameObject, WinParticleGameObject.GetComponent<ParticleSystem>().main.duration);

            if (playerOneScore >= 3)
                UIManager.Instance.Win(true, false);

            if (playerTwoScore >= 3)
                UIManager.Instance.Win(false,true);
        }
        else
        {
            Debug.Log("Goal!");
            if(SoundManager.Instance)
            {
                SoundManager.Instance.Play(Goal);
            }
        }

        currentSubState = new ReturnPiecesState();
    }

    public void RestartGame()
    {
        UIManager.Instance.Rematch();
        ResetBoard();
        playerOneScore = playerTwoScore = 0;
        currentPlayerTurn = 1;

        currentSubState = new ReturnPiecesState();
    }

    public void ToggleMoveIndicators()
    {
        displayMoveIndicators = !displayMoveIndicators;
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

            gameState.movementIndicators.DeactivateAll();

            m_selectedPiece = m_overlappedPiece;
            ShowOutline(m_selectedPiece, select);
            m_overlappedPiece = null;

            DisplayMoves();
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
                    gameState.movementIndicators.DeactivateAll();
                }
            }
            else
            {
                HideOutline(m_selectedPiece);
                m_selectedPiece = null;
                gameState.movementIndicators.DeactivateAll();
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

    void DisplayMoves()
    {
        if(!gameState.displayMoveIndicators)
            return;
        Color color = Color.white;
        color.a = 0.5f;

        gameState.movementIndicators.SetColor(color);

        switch(m_selectedPiece.type)
        {
        case ChessType.Knight:
            DisplayKnight();
            break;
        case ChessType.Bishop:
            DisplayBishop();
            break;
        case ChessType.Rook:
            DisplayRook();
            break;
        case ChessType.Queen:
            DisplayQueen();
            break;
        case ChessType.King:
            DisplayKing();
            break;
        }
    }

    void DisplayKnight()
    {
        MovementIndicators indicators = gameState.movementIndicators;

        Vector2Int boardSize = gameState.gameBoard.boardSize;

        int tileDisplayIndex = 0;

        Vector2Int checkPos;
        checkPos = new Vector2Int(1, 2) + m_selectedPiece.position;
        if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
            tileDisplayIndex++;
        }
        
        checkPos = new Vector2Int(1, -2) + m_selectedPiece.position;
        if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
            tileDisplayIndex++;
        }
        
        checkPos = new Vector2Int(-1, 2) + m_selectedPiece.position;
        if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
            tileDisplayIndex++;
        }
        
        checkPos = new Vector2Int(-1, -2) + m_selectedPiece.position;
        if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
            tileDisplayIndex++;
        }
        
        checkPos = new Vector2Int(2, 1) + m_selectedPiece.position;
        if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
            tileDisplayIndex++;
        }
        
        checkPos = new Vector2Int(-2, 1) + m_selectedPiece.position;
        if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
            tileDisplayIndex++;
        }
        
        checkPos = new Vector2Int(2, -1) + m_selectedPiece.position;
        if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
            tileDisplayIndex++;
        }
        
        checkPos = new Vector2Int(-2, -1) + m_selectedPiece.position;
        if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
            tileDisplayIndex++;
        }

    }

    void DisplayKing()
    {
        MovementIndicators indicators = gameState.movementIndicators;

        Vector2Int boardSize = gameState.gameBoard.boardSize;

        int tileDisplayIndex = 0;
        for(int y = -1; y < 2; y++)
        {
            for(int x = -1; x < 2; x++)
            {
                Vector2Int checkPos = new Vector2Int(x, y) + m_selectedPiece.position;

                if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
                {
                    indicators.tiles[tileDisplayIndex].enabled = true;
                    indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                    tileDisplayIndex++;
                }
            }
        }
    }

    void DisplayQueen()
    {
        MovementIndicators indicators = gameState.movementIndicators;

        Vector2Int boardSize = gameState.gameBoard.boardSize;

        int tileDisplayIndex = 0;

        for(int x = m_selectedPiece.position.x + 1; x < boardSize.x; x++)
        {
            Vector2Int checkPos = new Vector2Int(x, m_selectedPiece.position.y);

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        for(int x = m_selectedPiece.position.x - 1; x > 0; x--)
        {
            Vector2Int checkPos = new Vector2Int(x, m_selectedPiece.position.y);

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        for(int y = m_selectedPiece.position.y + 1; y < boardSize.y; y++)
        {
            Vector2Int checkPos = new Vector2Int(m_selectedPiece.position.x, y);

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        for(int y = m_selectedPiece.position.y - 1; y >= 0; y--)
        {
            Vector2Int checkPos = new Vector2Int(m_selectedPiece.position.x, y);

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        Vector2Int startingOffset = new Vector2Int(-1, -1);
        for(Vector2Int offset = m_selectedPiece.position + startingOffset; offset.x > 0 && offset.y >= 0; offset += startingOffset)
        {
            Vector2Int checkPos = offset;

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }

        startingOffset = new Vector2Int(1, -1);
        for(Vector2Int offset = m_selectedPiece.position + startingOffset; offset.x < boardSize.x && offset.y >= 0; offset += startingOffset)
        {
            Vector2Int checkPos = offset;

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }

        startingOffset = new Vector2Int(-1, 1);
        for(Vector2Int offset = m_selectedPiece.position + startingOffset; offset.x > 0 && offset.y <= boardSize.y; offset += startingOffset)
        {
            Vector2Int checkPos = offset;

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }

        startingOffset = new Vector2Int(1, 1);
        for(Vector2Int offset = m_selectedPiece.position + startingOffset; offset.x < boardSize.x && offset.y <= boardSize.y; offset += startingOffset)
        {
            Vector2Int checkPos = offset;

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }

    }

    void DisplayRook()
    {
        MovementIndicators indicators = gameState.movementIndicators;

        Vector2Int boardSize = gameState.gameBoard.boardSize;

        int tileDisplayIndex = 0;

        for(int x = m_selectedPiece.position.x + 1; x < boardSize.x; x++)
        {
            Vector2Int checkPos = new Vector2Int(x, m_selectedPiece.position.y);

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        for(int x = m_selectedPiece.position.x - 1; x > 0; x --)
        {
            Vector2Int checkPos = new Vector2Int(x, m_selectedPiece.position.y);

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        for(int y = m_selectedPiece.position.y + 1; y < boardSize.y; y++)
        {
            Vector2Int checkPos = new Vector2Int(m_selectedPiece.position.x, y);

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        for(int y = m_selectedPiece.position.y - 1; y >= 0; y--)
        {
            Vector2Int checkPos = new Vector2Int(m_selectedPiece.position.x, y);

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }

    }

    void DisplayBishop()
    {
        MovementIndicators indicators = gameState.movementIndicators;

        Vector2Int boardSize = gameState.gameBoard.boardSize;

        int tileDisplayIndex = 0;

        Vector2Int startingOffset = new Vector2Int(-1, -1);
        for(Vector2Int offset = m_selectedPiece.position + startingOffset; offset.x > 0 && offset.y >= 0; offset += startingOffset)
        {
            Vector2Int checkPos = offset;

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        
        startingOffset = new Vector2Int(1, -1);
        for(Vector2Int offset = m_selectedPiece.position + startingOffset; offset.x < boardSize.x && offset.y >= 0; offset += startingOffset)
        {
            Vector2Int checkPos = offset;

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        
        startingOffset = new Vector2Int(-1, 1);
        for(Vector2Int offset = m_selectedPiece.position + startingOffset; offset.x > 0 && offset.y <= boardSize.y; offset += startingOffset)
        {
            Vector2Int checkPos = offset;

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        
        startingOffset = new Vector2Int(1, 1);
        for(Vector2Int offset = m_selectedPiece.position + startingOffset; offset.x < boardSize.x && offset.y <= boardSize.y; offset += startingOffset)
        {
            Vector2Int checkPos = offset;

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }


    }
}


class PlayerMoveState : GameSubState
{
    ChessPiece m_possessingPiece;
    Vector2Int m_targetBoardPos;

    Vector2 m_targetWorldPos;

    float time = 0;

    float timeStretch;
    Vector3 startingPos;

    const float movementSpeed = 3;
    public PlayerMoveState(ChessPiece possessingPiece, Vector2Int targetBoardPos)
    {
        m_possessingPiece = possessingPiece;
        m_targetBoardPos = targetBoardPos;

        Vector2Int posDiff = targetBoardPos - m_possessingPiece.position;

        if(Mathf.Abs(posDiff.x) > Mathf.Abs(posDiff.y))
            timeStretch = Mathf.Abs(posDiff.x);
        else
            timeStretch = Mathf.Abs(posDiff.y);

        startingPos = m_possessingPiece.transform.position;

        gameState.movementIndicators.DeactivateAll();
        m_targetWorldPos = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, targetBoardPos);
    }

    public override void Update()
    {
        MoveUnit();
    }

    void MoveUnit()
    {
        time += Time.deltaTime / timeStretch * movementSpeed;

        m_possessingPiece.transform.position = Vector3.Lerp(startingPos, m_targetWorldPos, time);

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
        Vector2Int ballBoardPos = GameBoard.WorldPositionToBoardPosition(gameState.gameBoard, gameState.soccerBall.transform.position);
        ChessPiece chessPiece = gameState.gameBoard.GetBoardPieceAt(m_targetBoardPos) as ChessPiece;

        bool collidedWithBall = false;
        if(ballBoardPos == m_targetBoardPos)
        {
            collidedWithBall = true;
        }
        else if(chessPiece)
        {
            //Add piece to the remove list
            if (SoundManager.Instance)
            {
                SoundManager.Instance.Play(gameState.Hit);
            }

            gameState.gameBoard.RemovePiece(chessPiece);
            if(gameState.currentPlayerTurn == 0)
            {
                gameState.playerTwoField.CaptureTarget(chessPiece);
            }
            else
            {
                gameState.playerOneField.CaptureTarget(chessPiece);
            }
            gameState.currentSubState = new ReturnPiecesState();
        }
        else
        {
            gameState.currentSubState = new ReturnPiecesState();
        }

        gameState.gameBoard.MovePiece(m_possessingPiece, m_targetBoardPos);

        if(collidedWithBall)
            gameState.currentSubState = new BallMoveInputState(m_possessingPiece);
    }
}

class BallMoveInputState : GameSubState
{
    ChessPiece m_selectedPiece;
    Color ballPossession = Color.blue;

    const float outlineWidth = 0.03f;

    public BallMoveInputState(ChessPiece possessingPiece)
    {
        m_selectedPiece = possessingPiece;

        m_selectedPiece.spriteRenderer.material.SetFloat("_OutlineWidth", outlineWidth);
        m_selectedPiece.spriteRenderer.material.SetColor("_OutlineColor", ballPossession);

        DisplayMoves();
    }

    public override void Update()
    {
        if(Input.GetMouseButtonDown(0))
            HandleClick();
    }

    void HandleClick()
    {
        Vector2Int selectedBoardPosition = gameState.RaycastToBoardPosition();
        if(m_selectedPiece.CanMove(gameState.gameBoard, selectedBoardPosition) && gameState.ValidBallPosition(selectedBoardPosition))
        {
            List<BoardPiece> collidedPieces = m_selectedPiece.ProjectMovement(gameState.gameBoard, selectedBoardPosition);
            if(collidedPieces.Count == 0)
            {
                m_selectedPiece.spriteRenderer.material.SetFloat("_OutlineWidth", 0);
                gameState.currentSubState = new BallMoveState(m_selectedPiece, selectedBoardPosition);

                if(SoundManager.Instance)
                {
                    SoundManager.Instance.RandomSoundEffect(gameState.audioClipKickingArray);
                }
            }
            else
            {
                m_selectedPiece.spriteRenderer.material.SetFloat("_OutlineWidth", 0);
                gameState.currentSubState = new BallMoveState(m_selectedPiece, selectedBoardPosition);

                if(SoundManager.Instance)
                {
                    SoundManager.Instance.RandomSoundEffect(gameState.audioClipKickingArray);
                }
            }
        }
    }



    void DisplayMoves()
    {
        if(!gameState.displayMoveIndicators)
            return;
        Color color = Color.blue;
        color.a = 0.5f;

        gameState.movementIndicators.SetColor(color);
        switch(m_selectedPiece.type)
        {
        case ChessType.Knight:
            DisplayKnight();
            break;
        case ChessType.Bishop:
            DisplayBishop();
            break;
        case ChessType.Rook:
            DisplayRook();
            break;
        case ChessType.Queen:
            DisplayQueen();
            break;
        case ChessType.King:
            DisplayKing();
            break;
        }
    }

    void DisplayKnight()
    {
        MovementIndicators indicators = gameState.movementIndicators;

        Vector2Int boardSize = gameState.gameBoard.boardSize;

        int tileDisplayIndex = 0;

        Vector2Int checkPos;
        checkPos = new Vector2Int(1, 2) + m_selectedPiece.position;
        if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
            tileDisplayIndex++;
        }

        checkPos = new Vector2Int(1, -2) + m_selectedPiece.position;
        if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
            tileDisplayIndex++;
        }

        checkPos = new Vector2Int(-1, 2) + m_selectedPiece.position;
        if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
            tileDisplayIndex++;
        }

        checkPos = new Vector2Int(-1, -2) + m_selectedPiece.position;
        if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
            tileDisplayIndex++;
        }

        checkPos = new Vector2Int(2, 1) + m_selectedPiece.position;
        if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
            tileDisplayIndex++;
        }

        checkPos = new Vector2Int(-2, 1) + m_selectedPiece.position;
        if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
            tileDisplayIndex++;
        }

        checkPos = new Vector2Int(2, -1) + m_selectedPiece.position;
        if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
            tileDisplayIndex++;
        }

        checkPos = new Vector2Int(-2, -1) + m_selectedPiece.position;
        if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
            tileDisplayIndex++;
        }

    }

    void DisplayKing()
    {
        MovementIndicators indicators = gameState.movementIndicators;

        Vector2Int boardSize = gameState.gameBoard.boardSize;

        int tileDisplayIndex = 0;
        for(int y = -1; y < 2; y++)
        {
            for(int x = -1; x < 2; x++)
            {
                Vector2Int checkPos = new Vector2Int(x, y) + m_selectedPiece.position;

                if(!gameState.gameBoard.IsPositionOccupied(checkPos) && gameState.ValidPlayerPosition(checkPos))
                {
                    indicators.tiles[tileDisplayIndex].enabled = true;
                    indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                    tileDisplayIndex++;
                }
            }
        }
    }

    void DisplayQueen()
    {
        MovementIndicators indicators = gameState.movementIndicators;

        Vector2Int boardSize = gameState.gameBoard.boardSize;

        int tileDisplayIndex = 0;

        for(int x = m_selectedPiece.position.x + 1; x < boardSize.x; x++)
        {
            Vector2Int checkPos = new Vector2Int(x, m_selectedPiece.position.y);

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        for(int x = m_selectedPiece.position.x - 1; x > 0; x--)
        {
            Vector2Int checkPos = new Vector2Int(x, m_selectedPiece.position.y);

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        for(int y = m_selectedPiece.position.y + 1; y < boardSize.y; y++)
        {
            Vector2Int checkPos = new Vector2Int(m_selectedPiece.position.x, y);

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        for(int y = m_selectedPiece.position.y - 1; y >=0; y--)
        {
            Vector2Int checkPos = new Vector2Int(m_selectedPiece.position.x, y);

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        Vector2Int startingOffset = new Vector2Int(-1, -1);
        for(Vector2Int offset = m_selectedPiece.position + startingOffset; offset.x > 0 && offset.y >= 0; offset += startingOffset)
        {
            Vector2Int checkPos = offset;

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }

        startingOffset = new Vector2Int(1, -1);
        for(Vector2Int offset = m_selectedPiece.position + startingOffset; offset.x < boardSize.x && offset.y >= 0; offset += startingOffset)
        {
            Vector2Int checkPos = offset;

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }

        startingOffset = new Vector2Int(-1, 1);
        for(Vector2Int offset = m_selectedPiece.position + startingOffset; offset.x > 0 && offset.y <= boardSize.y; offset += startingOffset)
        {
            Vector2Int checkPos = offset;

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }

        startingOffset = new Vector2Int(1, 1);
        for(Vector2Int offset = m_selectedPiece.position + startingOffset; offset.x < boardSize.x && offset.y <= boardSize.y; offset += startingOffset)
        {
            Vector2Int checkPos = offset;

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }

    }

    void DisplayRook()
    {
        MovementIndicators indicators = gameState.movementIndicators;

        Vector2Int boardSize = gameState.gameBoard.boardSize;

        int tileDisplayIndex = 0;

        for(int x = m_selectedPiece.position.x + 1; x < boardSize.x; x++)
        {
            Vector2Int checkPos = new Vector2Int(x, m_selectedPiece.position.y);

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        for(int x = m_selectedPiece.position.x - 1; x > 0; x--)
        {
            Vector2Int checkPos = new Vector2Int(x, m_selectedPiece.position.y);

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        for(int y = m_selectedPiece.position.y + 1; y < boardSize.y; y++)
        {
            Vector2Int checkPos = new Vector2Int(m_selectedPiece.position.x, y);

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }
        for(int y = m_selectedPiece.position.y - 1; y >=0; y--)
        {
            Vector2Int checkPos = new Vector2Int(m_selectedPiece.position.x, y);

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }

    }

    void DisplayBishop()
    {
        MovementIndicators indicators = gameState.movementIndicators;

        Vector2Int boardSize = gameState.gameBoard.boardSize;

        int tileDisplayIndex = 0;

        Vector2Int startingOffset = new Vector2Int(-1, -1);
        for(Vector2Int offset = m_selectedPiece.position + startingOffset; offset.x > 0 && offset.y >= 0; offset += startingOffset)
        {
            Vector2Int checkPos = offset;

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }

        startingOffset = new Vector2Int(1, -1);
        for(Vector2Int offset = m_selectedPiece.position + startingOffset; offset.x < boardSize.x && offset.y >= 0; offset += startingOffset)
        {
            Vector2Int checkPos = offset;

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }

        startingOffset = new Vector2Int(-1, 1);
        for(Vector2Int offset = m_selectedPiece.position + startingOffset; offset.x > 0 && offset.y <= boardSize.y; offset += startingOffset)
        {
            Vector2Int checkPos = offset;

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }

        startingOffset = new Vector2Int(1, 1);
        for(Vector2Int offset = m_selectedPiece.position + startingOffset; offset.x < boardSize.x && offset.y <= boardSize.y; offset += startingOffset)
        {
            Vector2Int checkPos = offset;

            if(gameState.gameBoard.IsPositionOccupied(checkPos))
                break;

            if(gameState.ValidPlayerPosition(checkPos))
            {
                indicators.tiles[tileDisplayIndex].enabled = true;
                indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, checkPos);
                tileDisplayIndex++;
            }
        }


    }
}

class BallMoveState : GameSubState
{
    ChessPiece m_originalKicker;
    Vector2Int m_targetBoardPos;

    Vector2 m_targetWorldPos;
    float time = 0;
    Vector3 startingPos;

    public BallMoveState(ChessPiece kicker, Vector2Int targetBoardPos)
    {
        m_originalKicker = kicker;
        m_targetBoardPos = targetBoardPos;

        m_targetWorldPos = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, targetBoardPos);
        startingPos = gameState.soccerBall.transform.position;

        gameState.movementIndicators.DeactivateAll();
    }

    public override void Update()
    {
        MoveBall();
    }

    void MoveBall()
    {
        time += Time.deltaTime;

        gameState.soccerBall.transform.position = Vector3.Lerp(startingPos, m_targetWorldPos, time);

        Vector2Int currentBallPos = GameBoard.WorldPositionToBoardPosition(GameState.instance.gameBoard, gameState.soccerBall.transform.position);
        ChessPiece chessPiece = gameState.gameBoard.GetBoardPieceAt(currentBallPos) as ChessPiece;

        if(chessPiece)
        {
            if(chessPiece != m_originalKicker && m_originalKicker.type != ChessType.Knight)
            {
                gameState.currentSubState = new BallMoveInputState(chessPiece);
                gameState.soccerBall.transform.position = chessPiece.transform.position;
            }
        }

        if((Vector2)gameState.soccerBall.transform.position == m_targetWorldPos)
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
    ChessPiece returningPiece;


    Color returningColor = Color.red;
    float width = 0.03f;

    float time;

    int colorDirection = 1;
    public ReturnPiecesState()
    {
        gameState.currentPlayerTurn = (gameState.currentPlayerTurn + 1) % 2;

        if(gameState.currentPlayerTurn == 0)
        {
            foreach(var piece in gameState.playerOneField.capturedObjects)
            {
                if(piece.turnsLeft == 0 && piece.piece != null)
                {
                    returningPiece = piece.piece;

                    returningPiece.spriteRenderer.material.SetFloat("_OutlineWidth", width);
                    returningPiece.spriteRenderer.material.SetColor("_OutlineColor", returningColor);
                    break;
                }
            }
        }
        else
        {
            foreach(var piece in gameState.playerTwoField.capturedObjects)
            {
                if(piece.turnsLeft == 0 && piece.piece != null)
                {
                    returningPiece = piece.piece;

                    returningPiece.spriteRenderer.material.SetFloat("_OutlineWidth", width);
                    returningPiece.spriteRenderer.material.SetColor("_OutlineColor", returningColor);
                    break;
                }
            }
        }

        if(returningPiece)
            DisplayMoves();
    }

    public override void Update()
    {
        if(returningPiece)
        {
            if(Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
        }
        else
        {
            if(gameState.currentPlayerTurn == 0)
            {
                gameState.playerOneField.TickTurn();
            }
            else
            {
                gameState.playerTwoField.TickTurn();
            }

            gameState.movementIndicators.DeactivateAll();
            gameState.currentSubState = new PlayerMoveInputState();
        }

        Color color = Color.red;
        color.a = time;
        gameState.movementIndicators.SetColor(color);


        time += Time.deltaTime * colorDirection;
        if(time >= 1)
            colorDirection = -1;
        else if(time <= 0)
            colorDirection = 1;
    }

    void HandleClick()
    {
        Vector2Int selectedBoardPos = gameState.RaycastToBoardPosition();

        if(gameState.currentPlayerTurn == 0)
        {
            if(selectedBoardPos.x < 3 &&
                gameState.ValidPlayerPosition(selectedBoardPos) &&
                !gameState.gameBoard.IsPositionOccupied(selectedBoardPos))
            {
                gameState.gameBoard.PlacePiece(returningPiece, selectedBoardPos);
                gameState.playerOneField.ReleaseTarget(returningPiece);
                returningPiece.spriteRenderer.material.SetFloat("_OutlineWidth", 0);
                returningPiece = null;
            }
        }
        else
        {
            if(selectedBoardPos.x >= gameState.gameBoard.boardSize.x - 3
                && gameState.ValidPlayerPosition(selectedBoardPos) &&
                !gameState.gameBoard.IsPositionOccupied(selectedBoardPos))
            {
                gameState.gameBoard.PlacePiece(returningPiece, selectedBoardPos);
                gameState.playerTwoField.ReleaseTarget(returningPiece);
                returningPiece.spriteRenderer.material.SetFloat("_OutlineWidth", 0);
                returningPiece = null;
            }
        }

    }

    void DisplayMoves()
    {
        if(!gameState.displayMoveIndicators)
            return;
        MovementIndicators indicators = gameState.movementIndicators;

        Vector2Int boardSize = gameState.gameBoard.boardSize;

        int tileDisplayIndex = 0;
        indicators.SetColor(Color.red - new Color(0, 0, 0, 1));


        if(gameState.currentPlayerTurn == 0)
        {
            for(int y = 0; y < boardSize.y; y++)
            {
                for(int x = 0; x < 3; x++)
                {
                    Vector2Int boardPos = new Vector2Int(x, y);
                    if(gameState.ValidPlayerPosition(boardPos))
                    {
                        indicators.tiles[tileDisplayIndex].enabled = true;
                        indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, boardPos);
                        tileDisplayIndex++;
                    }
                }
            }
        }
        else
        {
            for(int y = 0; y < boardSize.y; y++)
            {
                for(int x = boardSize.x - 3; x < boardSize.x; x++)
                {
                    Vector2Int boardPos = new Vector2Int(x, y);
                    if(gameState.ValidPlayerPosition(boardPos))
                    {
                        indicators.tiles[tileDisplayIndex].enabled = true;
                        indicators.tiles[tileDisplayIndex].transform.position = GameBoard.BoardPositionToWorldPosition(gameState.gameBoard, boardPos);
                        tileDisplayIndex++;
                    }
                }
            }
        }
    }
}