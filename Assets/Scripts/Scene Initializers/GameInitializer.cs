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
        p1[0] = board.PlacePiece(Instantiate(bluePiecePrefabs[0]), new Vector2Int(1, 1));
        p1[1] = board.PlacePiece(Instantiate(bluePiecePrefabs[3]), new Vector2Int(1, 2));
        p1[2] = board.PlacePiece(Instantiate(bluePiecePrefabs[1]), new Vector2Int(1, 3));
        p1[3] = board.PlacePiece(Instantiate(bluePiecePrefabs[0]), new Vector2Int(1, 4));
        p1[4] = board.PlacePiece(Instantiate(bluePiecePrefabs[4]), new Vector2Int(1, 5));
        p1[5] = board.PlacePiece(Instantiate(bluePiecePrefabs[2]), new Vector2Int(2, 5));
        p1[6] = board.PlacePiece(Instantiate(bluePiecePrefabs[2]), new Vector2Int(2, 2));

        ChessPiece[] p2 = new ChessPiece[7];
        p2[0] = board.PlacePiece(Instantiate(redPiecePrefabs[0]), new Vector2Int(10 - 1, 5));
        p2[1] = board.PlacePiece(Instantiate(redPiecePrefabs[3]), new Vector2Int(10 - 1, 4));
        p2[2] = board.PlacePiece(Instantiate(redPiecePrefabs[1]), new Vector2Int(10 - 1, 3));
        p2[3] = board.PlacePiece(Instantiate(redPiecePrefabs[0]), new Vector2Int(10 - 1, 2));
        p2[4] = board.PlacePiece(Instantiate(redPiecePrefabs[4]), new Vector2Int(10 - 1, 1));
        p2[5] = board.PlacePiece(Instantiate(redPiecePrefabs[2]), new Vector2Int(10 - 2, 1));
        p2[6] = board.PlacePiece(Instantiate(redPiecePrefabs[2]), new Vector2Int(10 - 2, 4));

        state.Initialize(p1, p2);
    }
}
