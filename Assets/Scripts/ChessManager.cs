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
    [Header("棋盘")]
    [SerializeField] Vector2Int boardSize = new Vector2Int(9, 10);
    ChessBoard chessBoard = null;
    public GameObject chessBoardRange;
    public GameObject attackRange;
    public GameObject moveRange;
    [SerializeField] Vector3 mousePosition;
    [SerializeField] Vector3Int cellPosition;
    public Tilemap tilemap;
    Vector2 shootDirection;
    [SerializeField] string chessDataPath = "Assets/LO/01.txt";
    [SerializeField] Player player = null;
    [SerializeField] Chess select = null;
    [Header("基地和卷轴")]
    [SerializeField] Base baseChess;
    public Vector2Int basePosition { get { return new Vector2Int(baseChess.x, baseChess.y); } }
    public int highestRow = 9;
    [SerializeField] UI_Cursor cursor;
    [SerializeField] string testMassage;
    public void Init()
    {
        Debug.Log("Init ChessManager");
        if (chessBoardRange == null || attackRange == null || moveRange == null)
        {
            Debug.LogError("Please assign chessBoardRange, attackRange, moveRange and playerRange in the inspector.");
            return;
        }

        ChessManager.instance = this;
        ChessFactory.Init();
        
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
        if(chessBoard == null)
        {
            chessBoard = new ChessBoard();
            chessBoard.moveRangeParent = moveRange.transform;
            chessBoard.attackRangeParent = attackRange.transform;
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
                if(chessType == ' ' || chessType == 'x')
                {
                    chessBoard[i, j] = null;
                    continue;
                }
                chess = ChessFactory.CreateChess(chessType, this.transform);
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
    }
    void PlaceBase()
    {
        Debug.Log("PlaceBase");
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
        int newHighestRow = Mathf.Min(basePosition.y + 9, ChessBoard.instance.rowNum - 1);
        if (newHighestRow > highestRow)
        {
            for(int i = highestRow + 1; i <= newHighestRow; i++)
            {
                for(int j = 0; j < ChessBoard.instance.colNum; j++)
                {
                    Chess chess = chessBoard[i, j];
                    if (chess != null && chess.camp != player.camp)
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

    public bool haveActingChess()
    {
        return actingChessStack.Count > 0;
    }

    Player.ActionType lastActionType = Player.ActionType.Move;

    public void EnemyTurnStart()
    {
        Debug.Log("Enemy Turn Start");
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
        while(chessPrepareToActList.Count > 0)
        {
            Chess chess = null;
            int index = -1;
            int maxSpeed = -1;
            for (int i = 0; i < chessPrepareToActList.Count; i++)
            {
                if (chessPrepareToActList[i].gameObject.activeSelf == false)
                {
                    chessPrepareToActList.RemoveAt(i);
                    i--;
                    continue;
                }
                if (chessPrepareToActList[i].speed > maxSpeed)
                {
                    chess = chessPrepareToActList[i];
                    maxSpeed = chess.speed;
                    index = i;
                }
            }
            if (index == -1 || chess == null)
                continue;
            chessPrepareToActList.RemoveAt(index);

            //每个敌方棋子行动两次
            //第一次行动
            PushActingChess(chess);
            chess.Act();
            while (haveActingChess())
            {
                while(haveActingChess() && !PeekActingChess().isActing)
                    PopActingChess();
                yield return null;
            }
            if (!chess.gameObject.activeSelf)
            {
                continue;
            }
            //第二次行动
            PushActingChess(chess);
            chess.Act();
            while (haveActingChess())
            {
                while (haveActingChess() && !PeekActingChess().isActing)
                    PopActingChess();
                yield return null;
            }
        }
        player.PlayerTurnStart();
    }

    [Header("测试")]
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