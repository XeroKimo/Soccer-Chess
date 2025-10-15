using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlacedPiece
{
    public Vector2Int position;
    public GameBoard board;
}

public class GameBoard : MonoBehaviour
{
    const int PieceCount = 13;

    [SerializeField]
    Vector2Int m_boardSize = new Vector2Int(9, 7);
    [SerializeField]
    Vector2 m_cellSize = new Vector2(1, 1);

    ChessPiece[,] m_cells;

    public Vector2Int boardSize => m_boardSize;
    public Vector2 cellSize => m_cellSize;

    private void Awake()
    {
        m_cells = new ChessPiece[boardSize.y, boardSize.x];
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        for (int y = 0; y < boardSize.y; y++)
        {
            for (int x = 0; x < boardSize.x; x++)
            {
                Gizmos.DrawWireCube(CellPositionToWorldCenterPosition(new Vector2Int(x, y)), cellSize);
            }
        }

        Gizmos.color = Color.white;
    }

    public ChessPiece PlacePiece(ChessPiece piece, Vector2Int position)
    {
        if (GetPiece(position))
            throw new System.Exception($"Cell {position} is already occupied");
        if (piece.placedData.HasValue)
            throw new System.Exception($"{piece.name} is already placed on board { piece.placedData.Value.board }");

        SetPiece(piece, position);
        piece.placedData = new PlacedPiece { position = position, board = this };

        return piece;
    }

    public ChessPiece RemovePiece(Vector2Int position)
    {
        ChessPiece piece = GetPiece(position);

        return piece ? RemovePiece(piece) : piece;
    }

    public ChessPiece RemovePiece(ChessPiece selectedPiece)
    {
        Debug.Assert(selectedPiece != null && selectedPiece.placedData.HasValue && selectedPiece.placedData.Value.board == this);
        SetPiece(null, selectedPiece.placedData.Value.position);
        selectedPiece.placedData = null;
        return selectedPiece;
    }

    //Returns the old piece that occupied newPosition
    public ChessPiece MovePiece(ChessPiece selectedPiece, Vector2Int newPosition)
    {
        Debug.Assert(selectedPiece != null && selectedPiece.placedData.HasValue && selectedPiece.placedData.Value.board == this);

        ChessPiece removedPiece = RemovePiece(newPosition);
        PlacePiece(RemovePiece(selectedPiece), newPosition);

        return removedPiece;

    }

    //Returns the old piece that occupied newPosition
    public ChessPiece MovePiece(Vector2Int selectedPiece, Vector2Int newPosition)
    {
        return MovePiece(GetPiece(selectedPiece), newPosition);
    }

    public ChessPiece GetPiece(Vector2Int position)
    {
        return m_cells[position.y, position.x];
    }

    public bool InRange(Vector2Int position)
    {
        return new ChainCompare<int>(0) <= position.x < boardSize.x
            && new ChainCompare<int>(0) <= position.y < boardSize.y;
    }

    public bool IsOccupied(Vector2Int position)
    {
        return GetPiece(position);
    }

    private ChessPiece SetPiece(ChessPiece piece, Vector2Int position)
    {
        return m_cells[position.y, position.x] = piece;
    }

    public Vector2Int WorldPositionToCellPosition(Vector3 position)
    {
        Vector3 startingPos = transform.position - new Vector3(boardSize.x * m_cellSize.x, boardSize.y * m_cellSize.y) / 2;

        Vector3 localCellOffset = position - startingPos;

        Vector2Int selectedCell = new Vector2Int((int)(localCellOffset.x / m_cellSize.x), (int)(localCellOffset.y / m_cellSize.y));

        return selectedCell;
    }

    public Vector3 CellPositionToWorldCenterPosition(Vector2Int position)
    {
        Vector3 startingPos = transform.position - new Vector3(boardSize.x * m_cellSize.x, boardSize.y * m_cellSize.y, 0) / 2;
        Vector3 cellCentreOffset = new Vector3(m_cellSize.x, m_cellSize.y) / 2;
        Vector3 selectedCellOffset = new Vector3(position.x * m_cellSize.x, position.y * m_cellSize.y, 0);

        return startingPos + selectedCellOffset + cellCentreOffset;
    }

}
