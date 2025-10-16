using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.UI;

struct ChainCompare<T>
{
    T value;
    bool isValid;

    public ChainCompare(T value)
    {
        isValid = true;
        this.value = value;
    }

    private ChainCompare(T value, bool isValid)
    {
        this.value = value;
        this.isValid = isValid;
    }

    public static ChainCompare<T> operator ==(ChainCompare<T> left, T right)
    {
        return new ChainCompare<T>(right, left.isValid && EqualityComparer<T>.Default.Equals(left.value, right));
    }
    public static ChainCompare<T> operator !=(ChainCompare<T> left, T right)
    {
        return new ChainCompare<T>(right, left.isValid && !EqualityComparer<T>.Default.Equals(left.value, right));
    }
    public static ChainCompare<T> operator <=(ChainCompare<T> left, T right)
    {
        return new ChainCompare<T>(right, left.isValid && Comparer<T>.Default.Compare(left.value, right) <= 0);
    }
    public static ChainCompare<T> operator >=(ChainCompare<T> left, T right)
    {
        return new ChainCompare<T>(right, left.isValid && Comparer<T>.Default.Compare(left.value, right) >= 0);
    }
    public static ChainCompare<T> operator <(ChainCompare<T> left, T right)
    {
        return new ChainCompare<T>(right, left.isValid && Comparer<T>.Default.Compare(left.value, right) < 0);
    }
    public static ChainCompare<T> operator >(ChainCompare<T> left, T right)
    {
        return new ChainCompare<T>(right, left.isValid && Comparer<T>.Default.Compare(left.value, right) > 0);
    }

    public static ChainCompare<T> operator ==(T left, ChainCompare<T> right)
    {
        return new ChainCompare<T>(right.value, EqualityComparer<T>.Default.Equals(left, right.value) && right.isValid);
    }
    public static ChainCompare<T> operator !=(T left, ChainCompare<T> right)
    {
        return new ChainCompare<T>(right.value, !EqualityComparer<T>.Default.Equals(left, right.value) && right.isValid);
    }
    public static ChainCompare<T> operator <=(T left, ChainCompare<T> right)
    {
        return new ChainCompare<T>(right.value, Comparer<T>.Default.Compare(left, right.value) <= 0 && right.isValid);
    }
    public static ChainCompare<T> operator >=(T left, ChainCompare<T> right)
    {
        return new ChainCompare<T>(right.value, Comparer<T>.Default.Compare(left, right.value) >= 0 && right.isValid);
    }
    public static ChainCompare<T> operator <(T left, ChainCompare<T> right)
    {
        return new ChainCompare<T>(right.value, Comparer<T>.Default.Compare(left, right.value) < 0 && right.isValid);
    }
    public static ChainCompare<T> operator >(T left, ChainCompare<T> right)
    {
        return new ChainCompare<T>(right.value, Comparer<T>.Default.Compare(left, right.value) < 0 && right.isValid);
    }

    public static ChainCompare<T> operator ==(ChainCompare<T> left, ChainCompare<T> right)
    {
        return new ChainCompare<T>(right.value, left.isValid && EqualityComparer<T>.Default.Equals(left.value, right.value));
    }
    public static ChainCompare<T> operator !=(ChainCompare<T> left, ChainCompare<T> right)
    {
        return new ChainCompare<T>(right.value, left.isValid && !EqualityComparer<T>.Default.Equals(left.value, right.value));
    }
    public static ChainCompare<T> operator <=(ChainCompare<T> left, ChainCompare<T> right)
    {
        return new ChainCompare<T>(right.value, left.isValid && Comparer<T>.Default.Compare(left.value, right.value) <= 0);
    }
    public static ChainCompare<T> operator >=(ChainCompare<T> left, ChainCompare<T> right)
    {
        return new ChainCompare<T>(right.value, left.isValid && Comparer<T>.Default.Compare(left.value, right.value) >= 0);
    }
    public static ChainCompare<T> operator <(ChainCompare<T> left, ChainCompare<T> right)
    {
        return new ChainCompare<T>(right.value, left.isValid && Comparer<T>.Default.Compare(left.value, right.value) < 0);
    }
    public static ChainCompare<T> operator >(ChainCompare<T> left, ChainCompare<T> right)
    {
        return new ChainCompare<T>(right.value, left.isValid && Comparer<T>.Default.Compare(left.value, right.value) > 0);
    }

    public static implicit operator bool(ChainCompare<T> left)
    {
        return left.isValid;
    }

    public override bool Equals(object obj)
    {
        return (obj is ChainCompare<T> compare &&
               EqualityComparer<T>.Default.Equals(value, compare.value)) ||
               (obj is T rawCompare &&
               EqualityComparer<T>.Default.Equals(value, rawCompare));
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
};

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

    public ChessPiece soccerBall;

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
        return gameBoard.WorldPositionToCellPosition(worldMousePos);
    }

    public void Initialize(ChessPiece[] playerOnePieces, ChessPiece[] playerTwoPieces)
    {
        this.playerOnePieces = playerOnePieces;
        this.playerTwoPieces = playerTwoPieces;
        //this.soccerBall = soccerBallPiece;
        //foreach(ChessPiece piece in playerOnePieces)
        //{
        //    gameBoard.RegisterPiece(piece, gameBoard.WorldPositionToCellPosition(piece.transform.position), 0);
        //}
        //foreach(ChessPiece piece in playerTwoPieces)
        //{
        //    gameBoard.RegisterPiece(piece, gameBoard.WorldPositionToCellPosition(piece.transform.position), 1);
        //}

        //gameBoard.RegisterPiece(soccerPiece, gameBoard.WorldPositionToCellPosition(soccerPiece.transform.position), 2);
        gameBoard.PlacePiece(soccerBall, gameBoard.boardSize / 2);
        soccerBall.transform.position = gameBoard.CellPositionToWorldCenterPosition(soccerBall.position) - new Vector3(0, 0, 9);
        RestartGame();
    }

    void ResetBoard()
    {
        foreach(ChessPiece piece in playerOnePieces)
        {
            if(piece.placedData.HasValue)
                gameBoard.RemovePiece(piece);
        }
        foreach(ChessPiece piece in playerTwoPieces)
        {
            if(piece.placedData.HasValue)
                gameBoard.RemovePiece(piece);
        }

        gameBoard.PlacePiece(playerOnePieces[0], new Vector2Int(1, 1));
        gameBoard.PlacePiece(playerOnePieces[1], new Vector2Int(1, 2));
        gameBoard.PlacePiece(playerOnePieces[2], new Vector2Int(1, 3));
        gameBoard.PlacePiece(playerOnePieces[3], new Vector2Int(1, 4));
        gameBoard.PlacePiece(playerOnePieces[4], new Vector2Int(1, 5));
        gameBoard.PlacePiece(playerOnePieces[5], new Vector2Int(2, 5));
        gameBoard.PlacePiece(playerOnePieces[6], new Vector2Int(2, 2));

        gameBoard.PlacePiece(playerTwoPieces[0], new Vector2Int(10 - 1, 5));
        gameBoard.PlacePiece(playerTwoPieces[1], new Vector2Int(10 - 1, 4));
        gameBoard.PlacePiece(playerTwoPieces[2], new Vector2Int(10 - 1, 3));
        gameBoard.PlacePiece(playerTwoPieces[3], new Vector2Int(10 - 1, 2));
        gameBoard.PlacePiece(playerTwoPieces[4], new Vector2Int(10 - 1, 1));
        gameBoard.PlacePiece(playerTwoPieces[5], new Vector2Int(10 - 2, 1));
        gameBoard.PlacePiece(playerTwoPieces[6], new Vector2Int(10 - 2, 4));
        foreach(ChessPiece piece in playerOnePieces)
        {
            piece.transform.position = gameBoard.CellPositionToWorldCenterPosition(piece.position);
            //gameBoard.PlacePiece(piece, piece.initialPosition);
        }
        foreach(ChessPiece piece in playerTwoPieces)
        {
            piece.transform.position = gameBoard.CellPositionToWorldCenterPosition(piece.position);
            //gameBoard.PlacePiece(piece, piece.initialPosition);
        }

        playerOneField.ClearField();
        playerTwoField.ClearField();

        gameBoard.RemovePiece(gameBoard.boardSize / 2);
        gameBoard.PlacePiece(soccerBall, gameBoard.boardSize / 2);
        soccerBall.transform.position = gameBoard.CellPositionToWorldCenterPosition(gameBoard.boardSize / 2) - new Vector3(0, 0, 9);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if(selectedPiece)
            Gizmos.DrawCube(gameBoard.CellPositionToWorldCenterPosition(selectedPiece.position), gameBoard.cellSize);
    }

    public bool IsInField(Vector2Int targetPos)
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
        return IsInField(targetPos);

    }

    public bool IsInGoal(Vector2Int targetPos)
    {
        int startingGoalHeight = gameBoard.boardSize.y / 2 - goalSize / 2;

        return targetPos.y >= startingGoalHeight && targetPos.y < startingGoalHeight + goalSize && (targetPos.x == 0 || targetPos.x == gameBoard.boardSize.x - 1);
    }

    public void HandleGoal()
    {

        Vector2Int ballBoardPos = gameBoard.WorldPositionToCellPosition(soccerBall.transform.position);
        if(ballBoardPos.x == gameBoard.boardSize.x - 1)
        {
            Debug.Log("Player One Scored");
            playerOneScore++;

            blueScoreText.text = playerOneScore.ToString();

            currentPlayerTurn = 0;
        }
        else
        {
            Debug.Log("Player Two Scored");
            playerTwoScore++;
            redScoreText.text = playerTwoScore.ToString();
            currentPlayerTurn = 1;
        }

        if(playerTwoScore >= 3 || playerOneScore >= 3)
        {
            if(SoundManager.Instance)
            {
                SoundManager.Instance.Play(End);
            }
            GameObject WinParticleGameObject = GameObject.Instantiate(WinParticleObject);
            GameObject.Destroy(WinParticleGameObject, WinParticleGameObject.GetComponent<ParticleSystem>().main.duration);

            if(playerOneScore >= 3)
                UIManager.Instance.Win(true, false);

            if(playerTwoScore >= 3)
                UIManager.Instance.Win(false, true);
        }
        else
        {
            Debug.Log("Goal!");
            if(SoundManager.Instance)
            {
                SoundManager.Instance.Play(Goal);
            }
        }

        ResetBoard();

        currentSubState = new ReturnPiecesState();
    }

    public void RestartGame()
    {
        UIManager.Instance.Rematch();
        ResetBoard();
        playerOneScore = 0;
        playerTwoScore = 0;
        currentPlayerTurn = 1;
        blueScoreText.text = playerOneScore.ToString();
        redScoreText.text = playerTwoScore.ToString();
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
    public abstract void OnEnter();
    public abstract void OnExit();
}

class PlayerMoveInputState : GameSubState
{
    const float outlineWidth = 0.03f;
    Color overlapColor = Color.white;
    Color selectColor = Color.red;

    ChessPiece m_overlappedPiece;
    ChessPiece m_selectedPiece;

    ChessPiece overlappedPiece
    {
        get => m_overlappedPiece;
        set
        {
            if(m_overlappedPiece == value)
                return;

            if(m_overlappedPiece && m_overlappedPiece != selectedPiece)
                m_overlappedPiece.DisableOutline();

            m_overlappedPiece = value;

            if(m_overlappedPiece && m_overlappedPiece != selectedPiece)
                m_overlappedPiece.EnableOutline(overlapColor);
        }
    }

    ChessPiece selectedPiece
    {
        get => m_selectedPiece;
        set
        {
            if(m_selectedPiece)
            {
                gameState.movementIndicators.DeactivateAll();
                m_selectedPiece.DisableOutline();
            }

            m_selectedPiece = m_selectedPiece == value ? null : value;

            if(m_selectedPiece)
            {
                m_selectedPiece.EnableOutline(selectColor);
                DisplayMoves();
            }
        }
    }

    public override void Update()
    {
        Vector2Int hoveredCell = gameState.RaycastToBoardPosition();
        overlappedPiece = GetOverlappedPiece(hoveredCell);
        if(Input.GetMouseButtonDown(0))
        {
            if(selectedPiece && IsValidMove(hoveredCell))
            {
                gameState.currentSubState = new PlayerMoveState(selectedPiece, hoveredCell);
            }

            selectedPiece = overlappedPiece;
        }

    }

    private ChessPiece GetOverlappedPiece(Vector2Int position)
    {
#if NO_EXCEPTIONS
        //if (!gameState.gameBoard.InRange(position.Value))
        //    return null;

        //ChessPiece piece = gameState.gameBoard.GetPiece(position.Value);
        //return piece.team == gameState.currentPlayerTurn ? piece : null;
#else
        try
        {
            ChessPiece piece = gameState.gameBoard.GetPiece(position);
            return piece.team == gameState.currentPlayerTurn ? piece : null;
        }
        catch(Exception)
        {
            return null;
        }
#endif
    }

    private bool IsValidMove(Vector2Int position)
    {
        bool TraceIsValidMove()
        {
            Vector2Int direction = position - selectedPiece.position;
            direction.Clamp(new Vector2Int(-1, -1), new Vector2Int(1, 1));
            var (selectedPosition, blocked) = selectedPiece.GetValidPositions(false, direction)
                .First(p => p.Item1 == position);
#if NO_EXCEPTIONS
            ChessPiece piece = gameState.gameBoard.GetPiece(selectedPosition);
            return !blocked && !(piece && piece.team == gameState.currentPlayerTurn);
#else
            try
            {
                return !blocked && gameState.gameBoard.GetPiece(selectedPosition).team != gameState.currentPlayerTurn;
            }
            catch(Exception)
            {
                return true;
            }
#endif
        }

        return gameState.IsInField(position)
            && selectedPiece.IsValidMove(position)
            && TraceIsValidMove();

    }

    void DisplayMoves()
    {
        if(!gameState.displayMoveIndicators)
            return;
        Color color = Color.white;
        color.a = 0.5f;

        gameState.movementIndicators.SetColor(color);

        int tileDisplayIndex = 0;
        MovementIndicators indicators = gameState.movementIndicators;

        foreach(var (position, blocked) in m_selectedPiece.GetValidPositions(true).Where(p => gameState.IsInField(p.Item1)))
        {
            ChessPiece piece = gameState.gameBoard.GetPiece(position);
            indicators.tiles[tileDisplayIndex].enabled = !piece || piece.team != m_selectedPiece.team;
            indicators.tiles[tileDisplayIndex].transform.position = gameState.gameBoard.CellPositionToWorldCenterPosition(position);
            if(piece)
                indicators.tiles[tileDisplayIndex].color = Color.red - new Color(0, 0, 0, 0.5f);
            else if(position == gameState.soccerBall.position)
                indicators.tiles[tileDisplayIndex].color = Color.blue - new Color(0, 0, 0, 0.5f);

            tileDisplayIndex++;
        }
    }

    public override void OnEnter()
    {
    }

    public override void OnExit()
    {
        selectedPiece = null;
        overlappedPiece = null;
    }
}


class PlayerMoveState : GameSubState
{
    ChessPiece m_possessingPiece;
    Vector2Int m_targetBoardPos;

    Vector2 m_targetWorldPos;

    float distanceTravelled = 0;
    float totalDistance;
    Vector3 startingPos;

    const float movementSpeed = 21;
    public PlayerMoveState(ChessPiece possessingPiece, Vector2Int targetBoardPos)
    {
        m_possessingPiece = possessingPiece;
        m_targetBoardPos = targetBoardPos;

        Vector2Int posDiff = targetBoardPos - m_possessingPiece.position;

        startingPos = m_possessingPiece.transform.position;

        gameState.movementIndicators.DeactivateAll();
        m_targetWorldPos = (Vector3)gameState.gameBoard.CellPositionToWorldCenterPosition(targetBoardPos);// - new Vector3(0, 0, gameState.gameBoard.boardSize.y - m_targetBoardPos.y);
        totalDistance = Vector3.Distance(m_targetWorldPos, startingPos);
    }

    public override void Update()
    {
        distanceTravelled += movementSpeed * Time.deltaTime;

        m_possessingPiece.transform.position = Vector3.Lerp(startingPos, m_targetWorldPos, Mathf.InverseLerp(0, totalDistance, distanceTravelled));

        SoundManager.Instance.RandomSoundEffect(gameState.audioClipWalkingArray);
        if(distanceTravelled >= totalDistance)
        {
            ChessPiece removedPiece = gameState.gameBoard.MovePiece(m_possessingPiece, m_targetBoardPos);

            if(removedPiece)
            {
                if(removedPiece.type == ChessType.Ball)
                {
                    gameState.currentSubState = new BallMoveInputState(m_possessingPiece);
                }
                else
                {
                    SoundManager.Instance.Play(gameState.Hit);
                    (gameState.currentPlayerTurn == 0 ? gameState.playerTwoField : gameState.playerOneField).CaptureTarget(removedPiece);
                    gameState.currentSubState = new ReturnPiecesState();
                }
            }
            else
            {
                gameState.currentSubState = new ReturnPiecesState();
            }
        }
    }

    public override void OnEnter()
    {
        throw new NotImplementedException();
    }

    public override void OnExit()
    {
        throw new NotImplementedException();
    }
}

class BallMoveInputState : GameSubState
{
    ChessPiece m_selectedPiece;
    Color ballPossession = Color.blue;

    const float outlineWidth = 0.03f;
    bool m_oldMoveDisplay;
    public BallMoveInputState(ChessPiece possessingPiece)
    {
        m_selectedPiece = possessingPiece;

        m_selectedPiece.EnableOutline(ballPossession, outlineWidth);

        DisplayMoves();

        m_oldMoveDisplay = gameState.displayMoveIndicators;
    }

    public override void Update()
    {
        if(Input.GetMouseButtonDown(0))
            HandleClick();


        if(m_oldMoveDisplay != gameState.displayMoveIndicators)
        {
            if(gameState.displayMoveIndicators)
                DisplayMoves();
            else
                gameState.movementIndicators.DeactivateAll();
        }

        m_oldMoveDisplay = gameState.displayMoveIndicators;

    }

    void HandleClick()
    {
        Vector2Int selectedBoardPosition = gameState.RaycastToBoardPosition();
        if(m_selectedPiece.IsValidMove(selectedBoardPosition) && gameState.ValidBallPosition(selectedBoardPosition))
        {
            //List<ChessPiece> collidedPieces = m_selectedPiece.ProjectMovement(gameState.gameBoard, selectedBoardPosition);
            m_selectedPiece.DisableOutline();
            gameState.currentSubState = new BallMoveState(m_selectedPiece, selectedBoardPosition);

            if(SoundManager.Instance)
            {
                SoundManager.Instance.RandomSoundEffect(gameState.audioClipKickingArray);
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

        int tileDisplayIndex = 0;
        MovementIndicators indicators = gameState.movementIndicators;
        foreach(var (position, occupied) in m_selectedPiece.GetValidPositions(false).Where(p => gameState.ValidBallPosition(p.Item1)))
        {
            indicators.tiles[tileDisplayIndex].enabled = true;
            indicators.tiles[tileDisplayIndex].transform.position = gameState.gameBoard.CellPositionToWorldCenterPosition(position);
            tileDisplayIndex++;
        }
    }

    public override void OnEnter()
    {
        throw new NotImplementedException();
    }

    public override void OnExit()
    {
        throw new NotImplementedException();
    }
}

class BallMoveState : GameSubState
{
    ChessPiece m_originalKicker;
    Vector2Int m_targetBoardPos;

    Vector3 m_targetWorldPos;
    float time = 0;
    Vector3 startingPos;

    public BallMoveState(ChessPiece kicker, Vector2Int targetBoardPos)
    {
        m_originalKicker = kicker;
        m_targetBoardPos = targetBoardPos;

        m_targetWorldPos = (Vector3)gameState.gameBoard.CellPositionToWorldCenterPosition(targetBoardPos);// + new Vector3(0, 0, gameState.soccerBall.transform.position.z);
        startingPos = gameState.soccerBall.transform.position;

        gameState.movementIndicators.DeactivateAll();

        gameState.soccerBall.GetComponentInChildren<ParticleSystem>().Play();
    }

    public override void Update()
    {
        MoveBall();
    }

    void MoveBall()
    {
        time += Time.deltaTime;

        gameState.soccerBall.transform.position = Vector3.Lerp(startingPos, m_targetWorldPos, time);

        Vector2Int currentBallPos = gameState.gameBoard.WorldPositionToCellPosition(gameState.soccerBall.transform.position);
        ChessPiece chessPiece = gameState.gameBoard.GetPiece(currentBallPos) as ChessPiece;

        if(chessPiece)
        {
            if(chessPiece != m_originalKicker && m_originalKicker.type != ChessType.Knight)
            {
                gameState.currentSubState = new BallMoveInputState(chessPiece);
                gameState.soccerBall.transform.position = (Vector3)((Vector2)chessPiece.transform.position) + new Vector3(0, 0, gameState.soccerBall.transform.position.z);
            }
        }

        if(gameState.soccerBall.transform.position == m_targetWorldPos)
        {
            HandleMoveEnd();
        }
    }

    void HandleMoveEnd()
    {
        ChessPiece chessPiece = gameState.gameBoard.GetPiece(m_targetBoardPos) as ChessPiece;

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
            gameState.gameBoard.PlacePiece(gameState.soccerBall, m_targetBoardPos);
        }
        gameState.soccerBall.GetComponentInChildren<ParticleSystem>().Stop();
    }

    public override void OnEnter()
    {
        throw new NotImplementedException();
    }

    public override void OnExit()
    {
        throw new NotImplementedException();
    }
}

class HandleGoalState : GameSubState
{
    public override void OnEnter()
    {
        throw new NotImplementedException();
    }

    public override void OnExit()
    {
        throw new NotImplementedException();
    }

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

    bool placedOnBall = false;
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

                    returningPiece.EnableOutline(returningColor, width);
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

                    returningPiece.EnableOutline(returningColor, width);
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
            UpdateTileColors();

            if(Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
        }
        else
        {
            StartNextTurn();
        }
    }

    void UpdateTileColors()
    {

        foreach(SpriteRenderer tile in gameState.movementIndicators.tiles)
        {
            Color color = tile.color;
            color.a = time;
            tile.color = color;
        }

        time += Time.deltaTime * colorDirection;
        if(time >= 1)
            colorDirection = -1;
        else if(time <= 0)
            colorDirection = 1;
    }

    void HandleClick()
    {
        Vector2Int selectedBoardPos = gameState.RaycastToBoardPosition();


        placedOnBall = selectedBoardPos == gameState.soccerBall.position;
        if(gameState.currentPlayerTurn == 0)
        {
            if(selectedBoardPos.x < 3 &&
                gameState.IsInField(selectedBoardPos) &&
                !gameState.gameBoard.IsOccupied(selectedBoardPos))
            {
                gameState.gameBoard.PlacePiece(returningPiece, selectedBoardPos);
                returningPiece.transform.position = gameState.gameBoard.CellPositionToWorldCenterPosition(selectedBoardPos);
                gameState.playerOneField.ReleaseTarget(returningPiece);
                returningPiece.DisableOutline();
                StartNextTurn();
            }
        }
        else
        {
            if(selectedBoardPos.x >= gameState.gameBoard.boardSize.x - 3
                && gameState.IsInField(selectedBoardPos) &&
                !gameState.gameBoard.IsOccupied(selectedBoardPos))
            {
                gameState.gameBoard.PlacePiece(returningPiece, selectedBoardPos);
                returningPiece.transform.position = gameState.gameBoard.CellPositionToWorldCenterPosition(selectedBoardPos);
                gameState.playerTwoField.ReleaseTarget(returningPiece);
                returningPiece.DisableOutline();
                StartNextTurn();
            }
        }

    }

    void StartNextTurn()
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

        if(placedOnBall)
            gameState.currentSubState = new BallMoveInputState(returningPiece);
        else
            gameState.currentSubState = new PlayerMoveInputState();
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
                    if(gameState.IsInField(boardPos) && !gameState.gameBoard.IsOccupied(boardPos))
                    {
                        indicators.tiles[tileDisplayIndex].enabled = true;
                        indicators.tiles[tileDisplayIndex].transform.position = gameState.gameBoard.CellPositionToWorldCenterPosition(boardPos);

                        if(boardPos == gameState.soccerBall.position)
                            indicators.tiles[tileDisplayIndex].color = Color.blue - new Color(0, 0, 0, 0.5f);

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
                    if(gameState.IsInField(boardPos) && !gameState.gameBoard.IsOccupied(boardPos))
                    {
                        indicators.tiles[tileDisplayIndex].enabled = true;
                        indicators.tiles[tileDisplayIndex].transform.position = gameState.gameBoard.CellPositionToWorldCenterPosition(boardPos);
                        if(boardPos == gameState.soccerBall.position)
                            indicators.tiles[tileDisplayIndex].color = Color.blue - new Color(0, 0, 0, 0.5f);

                        tileDisplayIndex++;
                    }
                }
            }
        }
    }

    public override void OnEnter()
    {
        throw new NotImplementedException();
    }

    public override void OnExit()
    {
        throw new NotImplementedException();
    }
}