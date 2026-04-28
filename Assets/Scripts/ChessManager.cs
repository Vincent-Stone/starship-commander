using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChessManager : MonoBehaviour
{
    public static ChessManager instance;
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    //public enum GameState
    //{
    //    MainPhase,
    //    BattlePhase
    //}
    //public GameState gameState = GameState.MainPhase;
    [SerializeField] List<Chess> chessList;
    List<Chess> chessPrepareToActList;
    Stack<Chess> actingChessStack;
    [Header("ÆåÅÌ")]
    [SerializeField] Vector2Int boardSize = new Vector2Int(9, 10);
    ChessBoard chessBoard = null;
    [SerializeField] Vector3 mousePosition;
    [SerializeField] Vector3Int cellPosition;
    public Tilemap tilemap;
    Vector2 shootDirection;
    [SerializeField] string chessDataPath = "Assets/LO/01.txt";
    [SerializeField] Player player = null;
    [SerializeField] Chess select = null;
    [Header("»ùµØ")]
    [SerializeField] Base baseChess;
    public Vector2Int basePosition { get { return new Vector2Int(baseChess.x, baseChess.y); } }
    int highestRow = 9;
    [SerializeField] UI_Cursor cursor;
    //[SerializeField] UI_AdjustPanels adjustPanels;
    [SerializeField] string testMassage;
    void Start()
    {
        ChessManager.instance = this;
        ChessFactory.Init();
        chessBoard = new ChessBoard();
        chessList = new List<Chess>();
        actingChessStack = new Stack<Chess>();
        char chessType = 'p';

        Chess chess = null;
        StreamReader reader = new StreamReader(chessDataPath);
        string line = reader.ReadLine();
        boardSize = new Vector2Int(0, 0);
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (line[i] != ' ')
            {
                if(c < '0' || c > '9')
                {
                    Debug.LogError("Invalid character in board size definition: " + c);
                    reader.Close();
                    return;
                }
                boardSize.x *= 10;
                boardSize.x += c - '0';
            }
            else
            {
                for (i++; i < line.Length; i++)
                {
                    c = line[i];
                    if (c < '0' || c > '9')
                    {
                        Debug.LogError("Invalid character in board size definition: " + c);
                        reader.Close();
                        return;
                    }
                    boardSize.y *= 10;
                    boardSize.y += c - '0';
                }
                break;
            }
        }
        chessBoard.Init(boardSize.y, boardSize.x, this);
        for (int i = boardSize.y - 1; i >= 0; i--)
        {
            line = reader.ReadLine();
            //Debug.Log(line);
            for (int j = 0; j < boardSize.x; j++)
            {
                chess = null;
                if (j < line.Length)
                    chessType = line[j];
                else
                    chessType = ' ';
                switch (chessType)
                {
                    case 'p':
                        chess = ChessFactory.CreateChess(chessType, this.transform);
                        break;
                    case 'k':
                        chess = ChessFactory.CreateChess(chessType, this.transform);
                        break;
                    case 'r':
                        chess = ChessFactory.CreateChess(chessType, this.transform);
                        break;
                    default:
                        chessBoard[i, j] = null;
                        break;
                }
                if (chess != null)
                {
                    chess.x = j;
                    chess.y = i;
                    if (i <= highestRow)
                    {
                        chessList.Add(chess);
                    }
                    chessBoard[i, j] = chess;
                    chess.name = chessType.ToString() + (chessList.Count - 1).ToString();
                    chess.transform.position = ChessBoard.tilemap.GetCellCenterWorld(new Vector3Int(j, i, 0));
                }
                else
                {
                    chessBoard[i, j] = null;
                }
            }
        }
        reader.Close();
        PlaceBase();
        //StartCoroutine(MainPhase());
    }
    void PlaceBase()
    {
        if (baseChess != null)
        {
            baseChess.InitBaseAndPlayer();
            player = Player.instance;
            player.PlayerTurnStart();
        }
    }
    public void UpdateChessList()
    {
        for (int i = 0; i < chessList.Count; i++)
        {
            Chess chess = chessList[i];
            if (chess.gameObject.activeSelf == false)
            {
                chessList.Remove(chess);
                i--;
            }
        }
        int newHighestRow = Mathf.Min(basePosition.y + 9, ChessBoard.instance.rowNum);
        if (newHighestRow > highestRow)
        {
            for(int i = highestRow + 1; i <= newHighestRow; i++)
            {
                for(int j = 0; j < ChessBoard.instance.colNum; j++)
                {
                    Chess chess = chessBoard[i, j];
                    if (chess != null)
                    {
                        chessList.Add(chess);
                    }
                }
            }
            highestRow = newHighestRow;
        }
    }
    public void PushActingChess(Chess actingChess)
    {
        actingChess.isActing = true;
        actingChessStack.Push(actingChess);
    }
    public void PopActingChess()
    {
        if (actingChessStack.Count == 0)
        {
            Debug.LogError("Trying to pop from an empty acting chess stack.");
            return;
        }
        Chess actingChess = actingChessStack.Pop();
    }
    public Chess PeekActingChess()
    {
        if (actingChessStack.Count == 0)
        {
            Debug.LogError("Trying to peek from an empty acting chess stack.");
            return null;
        }
        return actingChessStack.Peek();
    }
    void PopActingChess(Chess actingChess)
    {
        if (actingChessStack.Count == 0)
        {
            Debug.LogError("Trying to pop from an empty acting chess stack.");
            return;
        }
        if (actingChessStack.Peek() != actingChess)
        {
            Debug.LogError("Trying to pop a chess that is not on top of the acting chess stack.");
            return;
        }
        actingChess.isActing = false;
        actingChessStack.Pop();
    }
    public bool haveActingChess()
    {
        return actingChessStack.Count > 0;
    }

    Player.ActionType lastActionType = Player.ActionType.Move;

    public void EnemyTurnStart()
    {
        chessPrepareToActList = new List<Chess>();
        for(int i=0;i<chessList.Count;i++)
        {
            Chess chess = chessList[i];
            if (chess.gameObject.activeSelf == true)
            {
                chessPrepareToActList.Add(chess);
            }
            else
            {
                chessList.Remove(chess);
                i--;
            }
        }
        StartCoroutine(EnemyTurnUpdate());
    }
    IEnumerator EnemyTurnUpdate()
    {
        foreach (Chess chess in chessPrepareToActList)
        {
            PushActingChess(chess);
            chess.Act();
            while (haveActingChess())
            {
                while(haveActingChess() && !PeekActingChess().isActing)
                    PopActingChess();
                yield return null;
            }
        }
        player.PlayerTurnStart();
    }
    //void MainPhaseUpdate()
    //{
    //    if (player.actionType != lastActionType)
    //    {
    //        OnChangePlayerActionType();
    //    }
    //    //adjustPanels.GetMouseInput(out int area, out Vector2 mouseWorldPosition);
    //    int area = 1;
    //    mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //    OnUpdateMousePosition(area, mousePosition);
    //    Vector3Int lastCellPosition = cellPosition;
    //    cellPosition = tilemap.WorldToCell(mousePosition);
    //    if(lastCellPosition != cellPosition)
    //    {
    //        OnUpdateMouseCellPosition();
    //    }
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        OnClicked(area, mousePosition);
    //    }
    //}



    //IEnumerator MainPhase()
    //{
    //    player.ShowRange();
    //    OnChangePlayerActionType();
    //    while (gameState == GameState.MainPhase)
    //    {
    //        MainPhaseUpdate();
    //        yield return null;
    //    }
    //    StartCoroutine(BattlePhase());
    //}

    //IEnumerator BattlePhase()
    //{
    //    Chess currentActingChess;
    //    foreach (Chess chess in chessList)
    //    {
    //        if(chess.gameObject.activeSelf == false)
    //        {
    //            continue;
    //        }
    //        chess.isActing = true;
    //        actingChessStack.Push(chess);
    //        chess.Act();
            
    //        while (actingChessStack.Count>0)
    //        {
    //            currentActingChess = actingChessStack.Peek();
    //            if(!currentActingChess.isActing)
    //                actingChessStack.Pop();
    //            yield return null;
    //        }
    //    }
    //    gameState = GameState.MainPhase;
    //    StartCoroutine(MainPhase());
    //}

    [Header("²âÊÔ")]
    [SerializeField] string chessBoardStr;
    private void Update()
    {
        chessBoardStr = "";
        string chessName = "";
        for(int i = chessBoard.rowNum-1; i >= 0; i--)
        {
            for(int j = 0; j < chessBoard.colNum; j++)
            {
                if (!ChessBoard.IsOnBoard(j, i))
                {
                    Debug.LogError(i + "," + j + " is not on board");
                    continue;
                }
                if (chessBoard[i, j] != null)
                    chessName = chessBoard[i, j].name;
                else
                    chessName = "";
                chessBoardStr += chessName;
                if(chessName.Length < 10)
                    for(int k = chessName.Length; k < 10; k++)
                    {
                        chessBoardStr += " ";
                    }
            }
            chessBoardStr += "\n";
        }
    }
}