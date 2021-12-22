using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Board board;

    public GameObject whiteKing;
    public GameObject whiteQueen;
    public GameObject whiteBishop;
    public GameObject whiteKnight;
    public GameObject whiteRook;
    public GameObject whitePawn;

    public GameObject blackKing;
    public GameObject blackQueen;
    public GameObject blackBishop;
    public GameObject blackKnight;
    public GameObject blackRook;
    public GameObject blackPawn;

    private GameObject[,] pieces;
    private List<GameObject> movedPawns;

    private Player White;
    private Player Black;
    public Player currentPlayer;
    public Player otherPlayer;

    public GameObject FirstPlayer;
    public GameObject SecondPlayer;

    public TextMeshProUGUI textMeshProUGUI;

    void Awake()
    {
        FirstPlayer.SetActive(true);
        SecondPlayer.SetActive(false);
        textMeshProUGUI.gameObject.SetActive(false);
        instance = this;
    }

    void Start ()
    {
        pieces = new GameObject[8, 8];
        movedPawns = new List<GameObject>();

        White = new Player("White", true);
        Black = new Player("Black", false);

        currentPlayer = White;
        otherPlayer = Black;

        InitialSetup();
    }

    private void InitialSetup()
    {
        AddPiece(whiteRook, White, 0, 0);
        AddPiece(whiteKnight, White, 1, 0);
        AddPiece(whiteBishop, White, 2, 0);
        AddPiece(whiteQueen, White, 3, 0);
        AddPiece(whiteKing, White, 4, 0);
        AddPiece(whiteBishop, White, 5, 0);
        AddPiece(whiteKnight, White, 6, 0);
        AddPiece(whiteRook, White, 7, 0);

        for (int i = 0; i < 8; i++)
        {
            AddPiece(whitePawn, White, i, 1);
        }

        AddPiece(blackRook, Black, 0, 7);
        AddPiece(blackKnight, Black, 1, 7);
        AddPiece(blackBishop, Black, 2, 7);
        AddPiece(blackQueen, Black, 3, 7);
        AddPiece(blackKing, Black, 4, 7);
        AddPiece(blackBishop, Black, 5, 7);
        AddPiece(blackKnight, Black, 6, 7);
        AddPiece(blackRook, Black, 7, 7);

        for (int i = 0; i < 8; i++)
        {
            AddPiece(blackPawn, Black, i, 6);
        }
    }

    public void AddPiece(GameObject prefab, Player player, int col, int row)
    {
        GameObject pieceObject = board.AddPiece(prefab, col, row);
        player.pieces.Add(pieceObject);
        pieces[col, row] = pieceObject;
    }

    public void SelectPieceAtGrid(Vector2Int gridPoint)
    {
        GameObject selectedPiece = pieces[gridPoint.x, gridPoint.y];
        if (selectedPiece)
        {
            board.SelectPiece(selectedPiece);
        }
    }

    public List<Vector2Int> MovesForPiece(GameObject pieceObject)
    {
        Piece piece = pieceObject.GetComponent<Piece>();
        Vector2Int gridPoint = GridForPiece(pieceObject);
        List<Vector2Int> locations = piece.MoveLocations(gridPoint);

        // Фильтр удалённых клеток
        locations.RemoveAll(gp => gp.x < 0 || gp.x > 7 || gp.y < 0 || gp.y > 7);

        // Фильтр клеток с союзниками
        locations.RemoveAll(gp => FriendlyPieceAt(gp));

        return locations;
    }

    public void Move(GameObject piece, Vector2Int gridPoint)
    {
        Piece pieceComponent = piece.GetComponent<Piece>();
        if (pieceComponent.type == PieceType.Pawn && !HasPawnMoved(piece))
        {
            movedPawns.Add(piece);
        }

        Vector2Int startGridPoint = GridForPiece(piece);
        pieces[startGridPoint.x, startGridPoint.y] = null;
        pieces[gridPoint.x, gridPoint.y] = piece;
        board.MovePiece(piece, gridPoint);
    }

    public void PawnMoved(GameObject pawn)
    {
        movedPawns.Add(pawn);
    }

    public bool HasPawnMoved(GameObject pawn)
    {
        return movedPawns.Contains(pawn);
    }

    public void CapturePieceAt(Vector2Int gridPoint)
    {
        GameObject pieceToCapture = PieceAtGrid(gridPoint);
        if (pieceToCapture.GetComponent<Piece>().type == PieceType.King)
        {
            textMeshProUGUI.gameObject.SetActive(true);
            textMeshProUGUI.text = currentPlayer.name + " wins!";
            Debug.Log(currentPlayer.name + " wins!");
            Destroy(board.GetComponent<TileSelector>());
            Destroy(board.GetComponent<MoveSelector>());
        }
        currentPlayer.capturedPieces.Add(pieceToCapture);
        pieces[gridPoint.x, gridPoint.y] = null;
        Destroy(pieceToCapture);
    }

    public void SelectPiece(GameObject piece)
    {
        board.SelectPiece(piece);
    }

    public void DeselectPiece(GameObject piece)
    {
        board.DeselectPiece(piece);
    }

    public bool DoesPieceBelongToCurrentPlayer(GameObject piece)
    {
        return currentPlayer.pieces.Contains(piece);
    }

    public GameObject PieceAtGrid(Vector2Int gridPoint)
    {
        if (gridPoint.x > 7 || gridPoint.y > 7 || gridPoint.x < 0 || gridPoint.y < 0)
        {
            return null;
        }
        return pieces[gridPoint.x, gridPoint.y];
    }

    public Vector2Int GridForPiece(GameObject piece)
    {
        for (int i = 0; i < 8; i++) 
        {
            for (int j = 0; j < 8; j++)
            {
                if (pieces[i, j] == piece)
                {
                    return new Vector2Int(i, j);
                }
            }
        }

        return new Vector2Int(-1, -1);
    }

    public bool FriendlyPieceAt(Vector2Int gridPoint)
    {
        GameObject piece = PieceAtGrid(gridPoint);

        if (piece == null) {
            return false;
        }

        if (otherPlayer.pieces.Contains(piece))
        {
            return false;
        }

        return true;
    }

    public void NextPlayer()
    {
        FirstPlayer.SetActive(!FirstPlayer.activeSelf);
        SecondPlayer.SetActive(!SecondPlayer.activeSelf);
        Player tempPlayer = currentPlayer;
        currentPlayer = otherPlayer;
        otherPlayer = tempPlayer;
    }
}
