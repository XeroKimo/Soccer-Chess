using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct BoardCell
{
    public Vector2Int position;
    public BoardPiece piece;
}

public class GameBoard : MonoBehaviour
{
    const int PieceCount = 13;

    [SerializeField]
    Vector2Int m_boardSize = new Vector2Int(9, 7);
    [SerializeField]
    Vector2 m_cellSize = new Vector2(1, 1);

    BoardCell[,] m_boardCells;

    List<BoardPiece> m_boardPieces = new List<BoardPiece>(PieceCount);


    public Vector2Int boardSize { get => m_boardSize; }
    public Vector2 cellSize { get => m_cellSize; }
    public List<BoardPiece> boardPieces { get => m_boardPieces; }

    public ChessPiece debugPiece;

    private void Awake()
    {
        m_boardCells = new BoardCell[boardSize.y, boardSize.x];
        for(int y = 0; y < boardSize.y; y++)
        {
            for(int x = 0; x < boardSize.x; x++)
            {
                m_boardCells[y, x].position = new Vector2Int(x, y);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        RegisterPiece(debugPiece, new Vector2Int(2, 3));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        for(int y = 0; y < boardSize.y; y++)
        {
            for(int x = 0; x < boardSize.x; x++)
            {
                Gizmos.DrawWireCube(BoardPositionToWorldPosition(this, new Vector2Int(x, y)), cellSize);
            }
        }

        Gizmos.color = Color.white;
        foreach(BoardPiece piece in boardPieces)
        {
            Gizmos.DrawCube(BoardPositionToWorldPosition(this, piece.position), cellSize);
        }
    }

    public bool MovePiece(BoardPiece piece, Vector2Int newPosition)
    {
        if(IsPositionOccupied(newPosition))
            return false;

        SetBoardCellPieceAt(newPosition, piece);
        return true;
    }

    public void RegisterPiece(BoardPiece piece, Vector2Int position)
    {
        m_boardPieces.Add(piece);

        Debug.Assert(IsInBoardRange(position),
            piece.name + ": Position is not within the board's range");

        foreach(BoardPiece boardPiece in boardPieces)
        {
            Debug.Assert(boardPiece.position != position, piece.name + ": Position is not within the board's range");
        }

        piece.position = position;
        piece.raycastCollider.size = cellSize;
        piece.transform.position = BoardPositionToWorldPosition(this, piece.position);

        m_boardCells[position.y, position.x].piece = piece;
    }

    public BoardPiece GetBoardPieceAt(Vector2Int position)
    {
        if(!IsInBoardRange(position))
            return null;

        return m_boardCells[position.y, position.x].piece;
    }

    public bool IsPositionOccupied(Vector2Int position)
    {
        return GetBoardPieceAt(position) != null;
    }

    public bool IsInBoardRange(Vector2Int position)
    {
        return position.x >= 0 && position.x < boardSize.x &&
               position.y >= 0 && position.y < boardSize.y;
    }

    public static Vector2 BoardPositionToWorldPosition(GameBoard board, Vector2Int position)
    {
        Vector3 startingPos = -new Vector3(board.boardSize.x * board.cellSize.x , board.boardSize.y * board.cellSize.y, 0) / 2;
        Vector3 cellCentreOffset = new Vector3(board.cellSize.x, board.cellSize.y) / 2;
        Vector3 cellOffset = new Vector3(position.x * board.cellSize.x, position.y * board.cellSize.y, 0);

        return startingPos + cellOffset + cellCentreOffset;
    }

    public static Vector2Int WorldPositionToBoardPosition(GameBoard board, Vector2 position)
    {
        Vector2 startingPos = -new Vector3(board.boardSize.x * board.cellSize.x, board.boardSize.y * board.cellSize.y) / 2;

        Vector2 boardPosition = position - startingPos;

        Vector2Int cellOffset = new Vector2Int((int)(boardPosition.x / board.cellSize.x), (int)(boardPosition.y / board.cellSize.y));


        return cellOffset;
    }

    private void SetBoardCellPieceAt(Vector2Int cellPosition, BoardPiece piece)
    {
        m_boardCells[piece.position.y, piece.position.x].piece = null;

        piece.position = cellPosition;

        m_boardCells[piece.position.y, piece.position.x].piece = piece;
    }
}
