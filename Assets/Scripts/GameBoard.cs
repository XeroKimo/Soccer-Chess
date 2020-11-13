using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameBoard : MonoBehaviour
{
    const int PieceCount = 13;

    [SerializeField]
    Vector2Int m_boardSize = new Vector2Int(9, 7);
    [SerializeField]
    Vector2 m_cellSize = new Vector2(1, 1);

    List<BoardPiece> m_boardPieces = new List<BoardPiece>(PieceCount);

    public Vector2Int boardSize { get => m_boardSize; }
    public Vector2 cellSize { get => m_cellSize; }
    public List<BoardPiece> boardPieces { get => m_boardPieces; }

    public ChessPiece debugPiece;

    private void Awake()
    {
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

    public void RegisterPiece(BoardPiece piece, Vector2Int position)
    {
        m_boardPieces.Add(piece);

        Debug.Assert(InBoardRangeBoardSpace(position),
            piece.name + ": Position is not within the board's range");

        foreach(BoardPiece boardPiece in boardPieces)
        {
            Debug.Assert(boardPiece.position != position, piece.name + ": Position is not within the board's range");
        }

        piece.position = position;
        piece.raycastCollider.size = boardSize;
    }

    public BoardPiece GetBoardPieceAt(Vector2Int position)
    {
        foreach(BoardPiece piece in boardPieces)
        {
            if(piece.position == position)
                return piece;
        }
        return null;
    }

    public bool InBoardRangeBoardSpace(Vector2Int position)
    {
        return position.x >= 0 && position.x <= boardSize.x &&
               position.y >= 0 && position.x <= boardSize.y;
    }

    public bool InBoardRangeWorldSpace(Vector3 position)
    {
        return InBoardRangeBoardSpace(WorldPositionToBoardPosition(this, position));
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
        Vector2 cellCentreOffset = new Vector3(board.cellSize.x, board.cellSize.y) / 2;

        Vector2 boardPosition = position - startingPos;

        Vector2Int cellOffset = new Vector2Int((int)(boardPosition.x / board.cellSize.x), (int)(boardPosition.y / board.cellSize.y));


        return cellOffset;
    }
}
