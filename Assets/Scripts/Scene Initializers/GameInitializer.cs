using UnityEngine;
using xksl;

public class GameInitializer : SceneInitializer
{
    public GameBoard board;
    public GameState state;

    public ChessPiece[] bluePiecePrefabs;
    public ChessPiece[] redPiecePrefabs;
    
    protected override void Initialize()
    {
        Debug.Log("Initialized");

        ChessPiece[] p1 = new ChessPiece[7];
        p1[0] = board.RegisterPiece(Instantiate(bluePiecePrefabs[0]), new Vector2Int(1, 1), 0) as ChessPiece;
        p1[1] = board.RegisterPiece(Instantiate(bluePiecePrefabs[3]), new Vector2Int(1, 2), 0) as ChessPiece;
        p1[2] = board.RegisterPiece(Instantiate(bluePiecePrefabs[1]), new Vector2Int(1, 3), 0) as ChessPiece;
        p1[3] = board.RegisterPiece(Instantiate(bluePiecePrefabs[0]), new Vector2Int(1, 4), 0) as ChessPiece;
        p1[4] = board.RegisterPiece(Instantiate(bluePiecePrefabs[4]), new Vector2Int(1, 5), 0) as ChessPiece;
        p1[5] = board.RegisterPiece(Instantiate(bluePiecePrefabs[2]), new Vector2Int(2, 5), 0) as ChessPiece;
        p1[6] = board.RegisterPiece(Instantiate(bluePiecePrefabs[2]), new Vector2Int(2, 2), 0) as ChessPiece;

        ChessPiece[] p2 = new ChessPiece[7];
        p2[0] = board.RegisterPiece(Instantiate(redPiecePrefabs[0]), new Vector2Int(10 - 1, 5), 1) as ChessPiece;
        p2[1] = board.RegisterPiece(Instantiate(redPiecePrefabs[3]), new Vector2Int(10 - 1, 4), 1) as ChessPiece;
        p2[2] = board.RegisterPiece(Instantiate(redPiecePrefabs[1]), new Vector2Int(10 - 1, 3), 1) as ChessPiece;
        p2[3] = board.RegisterPiece(Instantiate(redPiecePrefabs[0]), new Vector2Int(10 - 1, 2), 1) as ChessPiece;
        p2[4] = board.RegisterPiece(Instantiate(redPiecePrefabs[4]), new Vector2Int(10 - 1, 1), 1) as ChessPiece;
        p2[5] = board.RegisterPiece(Instantiate(redPiecePrefabs[2]), new Vector2Int(10 - 2, 1), 1) as ChessPiece;
        p2[6] = board.RegisterPiece(Instantiate(redPiecePrefabs[2]), new Vector2Int(10 - 2, 4), 1) as ChessPiece;

        state.Initialize(p1, p2);
    }
}
