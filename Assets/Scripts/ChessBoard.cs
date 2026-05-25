using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;


public class ChessBoard
{
    static Chess [,] chessBoard = null;
    public int rowNum = 0;
    public int colNum = 0;
    public Transform moveRangeParent = null;
    public Transform attackRangeParent = null;
    public GameObject rangePrefab = null;
    public GameObject attackRangePrefab = null;
    public GameObject moveRangePrefab = null;
    SpriteRenderer[,] rangeSprites = null;
    SpriteRenderer[,] attackRangeSprites = null;
    SpriteRenderer[,] moveRangeSprites = null;
    public static ChessBoard instance = null;
    public ChessManager chessManager = null;
    public static Tilemap tilemap;
    public int bossAreaLine => rowNum - 5;
    public void Init(int rowNum, int colNum, ChessManager chessManager)
    {
        Debug.Log("Init ChessBoard");
        instance = this;
        ClearChessBoard();
        if (chessManager.tilemap == null)
        {
            Debug.LogError("Tilemap is not assigned in ChessBoard.");
        }
        else
        {
            tilemap = chessManager.tilemap;
        }
        this.chessManager = chessManager;
        if (rangeSprites == null)
            this.rangePrefab = Resources.Load("Prefabs/Range") as GameObject;
        if(attackRangeSprites == null)
            this.attackRangePrefab = Resources.Load("Prefabs/AttackRange") as GameObject;
        if (moveRangeSprites == null)
            this.moveRangePrefab = Resources.Load("Prefabs/MoveRange") as GameObject;
        this.rowNum = rowNum;
        this.colNum = colNum;
        if(chessBoard != null && (chessBoard.GetLength(0) != rowNum || chessBoard.GetLength(1) != colNum))
        {
            chessBoard = new Chess[rowNum, colNum];
        }
        if (chessBoard == null)
            chessBoard = new Chess[rowNum,colNum];
        InitRangeSprites();
    }

    void InitRangeSprites()
    {
        Debug.Log("Init Range Sprites");
        rangeSprites = new SpriteRenderer[rowNum, colNum];
        attackRangeSprites = new SpriteRenderer[rowNum, colNum];
        moveRangeSprites = new SpriteRenderer[rowNum, colNum];
        for (int i = 0; i < rowNum; i++)
        {
            for (int j = 0; j < colNum; j++)
            {
                if (rangeSprites[i, j] != null)
                    continue;
                rangeSprites[i, j] = GameObject.Instantiate(rangePrefab, GetCellCenterWorld(i, j), Quaternion.identity).GetComponent<SpriteRenderer>();
                attackRangeSprites[i, j] = GameObject.Instantiate(attackRangePrefab, GetCellCenterWorld(i, j), Quaternion.identity).GetComponent<SpriteRenderer>();
                moveRangeSprites[i, j] = GameObject.Instantiate(moveRangePrefab, GetCellCenterWorld(i, j), Quaternion.identity).GetComponent<SpriteRenderer>();
                rangeSprites[i, j].transform.parent = chessManager.chessBoardRange.transform;
                attackRangeSprites[i, j].transform.parent = chessManager.attackRange.transform;
                moveRangeSprites[i, j].transform.parent = chessManager.moveRange.transform;
                rangeSprites[i, j].color = new Color(1, 1, 1, 0.5f); // Set initial transparency to 0
                attackRangeSprites[i, j].color = new Color(1, 1, 1, 0); // Set initial transparency to 0
                moveRangeSprites[i, j].color = new Color(1, 1, 1, 0); // Set initial transparency to 0
            }
        }
    }
    public void ClearChessBoard()
    {
        Debug.Log("Clear ChessBoard");
        if (chessBoard == null)
            return;
        foreach(Chess chess in chessBoard)
        {
            if (chess != null && (chess.chessTypeName != "Base" && chess.chessTypeName != "Player"))
            {
                GameObject.Destroy(chess.gameObject);
            }
        }
        if (rangeSprites == null || attackRangeSprites == null || moveRangeSprites == null)
            return;
        foreach (SpriteRenderer sprite in rangeSprites)
        {
            GameObject.Destroy(sprite.gameObject);
        }
        foreach (SpriteRenderer sprite in attackRangeSprites)
        {
            GameObject.Destroy(sprite.gameObject);
        }
        foreach (SpriteRenderer sprite in moveRangeSprites)
        {
            GameObject.Destroy(sprite.gameObject);
        }
    }


    public static bool IsInMoveRange(Vector2Int pos)
    {
        if (IsOnBoard(pos.x, pos.y))
        {
            return instance.moveRangeSprites[pos.y, pos.x].color.a > 0;
        }
        return false;
    }
    public void HideRange()
    {
        ChessManager.instance.moveRange.SetActive(false);
        ChessManager.instance.attackRange.SetActive(false);
    }
    public void ShowRange(List<Vector2Int> rangeList, Color highlightColor, bool showMoveRange)
    {
        if(showMoveRange)
        {
            foreach(SpriteRenderer sprite in instance.moveRangeSprites)
            {
                sprite.color = Color.clear;
            }
            foreach (Vector2Int pos in rangeList)
            {
                if (IsOnBoard(pos.x, pos.y))
                {
                    instance.moveRangeSprites[pos.y, pos.x].color = highlightColor;
                }
            }
            ChessManager.instance.moveRange.SetActive(true);
        }
        else
        {
            foreach (SpriteRenderer sprite in instance.attackRangeSprites)
            {
                sprite.color = Color.clear;
            }
            foreach (Vector2Int pos in rangeList)
            {
                if (IsOnBoard(pos.x, pos.y))
                {
                    instance.attackRangeSprites[pos.y, pos.x].color = highlightColor;
                }
            }
            ChessManager.instance.attackRange.SetActive(true);
        }
    }
    public static bool IsInAttackRange(Vector2Int pos)
    {
        if (IsOnBoard(pos.x, pos.y))
        {
            return instance.attackRangeSprites[pos.y, pos.x].color.a > 0;
        }
        return false;
    }

    public Chess this[int row,int col]
    {
        get
        {
            if(IsOnBoard(col, row))
                return chessBoard[row, col]; // 返回指定行列的值
            else
                return null;
        }
        set
        {
            if(IsOnBoard(col, row))
                chessBoard[row, col] = value; // 设置指定行列的值
        }
    }

    /// <summary>
    /// 获取指定位置的棋子。
    /// </summary>
    /// <param name="cellPos">棋盘上的单元格位置。</param>
    /// <returns>如果单元格在棋盘内且上单元格位置有棋子则返回该棋子，否则返回 null。</returns>
    public static Chess GetChess(Vector2Int cellPos)
    {
        if (IsOnBoard(cellPos.x, cellPos.y))
        {
            return chessBoard[cellPos.y, cellPos.x];
        }
        return null;
    }
    /// <summary>
    /// 获取指定位置的棋子。
    /// </summary>
    /// <param name="x">单元格位置的列索引（x坐标）。</param>
    /// <param name="y">单元格位置的行索引（y坐标）。</param>
    /// <returns>如果单元格在棋盘内且上单元格位置有棋子则返回该棋子，否则返回 null。</returns>
    public static Chess GetChess(int x,int y)
    {
        if (IsOnBoard(x, y))
        {
            return chessBoard[y, x];
        }
        return null;
    }
    public static Vector3 GetCellCenterWorld(int row, int col)
    {
        return tilemap.GetCellCenterWorld(new Vector3Int(col, row, 0));
    }
    public static Vector3 GetCellCenterWorld(Vector2Int cellPosition)
    {
        return tilemap.GetCellCenterWorld(new Vector3Int(cellPosition.x, cellPosition.y, 0));
    }
    public static Vector2Int GetCell(Vector3 worldPosition)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
        return new Vector2Int(cellPosition.x, cellPosition.y);
    }
    public static Vector2Int GetChessboardPosition(Vector3 worldPosition)
    {
        Vector2Int cellPosition = GetCell(worldPosition);
        if (IsOnBoard(cellPosition.x,cellPosition.y))
        {
            return cellPosition;
        }
        else
        {
            return new Vector2Int(-1, -1); // Return an invalid position if out of bounds
        }
    }
    /// <summary>
    /// 判断指定位置是否在棋盘上。
    /// </summary>
    /// <param name="x">单元格位置的列索引（x坐标）。</param>
    /// <param name="y">单元格位置的行索引（y坐标）。</param>
    /// <returns>如果单元格在棋盘上则返回 true，否则返回 false。</returns>
    public static bool IsOnBoard(int x, int y)
    {
        return x >= 0 && x < instance.colNum && y >= 0 && y < instance.rowNum;
    }
    /// <summary>
    /// 判断指定位置是否在棋盘上。
    /// </summary>
    /// <param name="cellPos">单元格位置。</param>
    /// <returns>如果单元格在棋盘上则返回 true，否则返回 false。</returns>
    public static bool IsOnBoard(Vector2Int cellPos)
    {
        return IsOnBoard(cellPos.x, cellPos.y);
    }
    /// <summary>
    /// 判断指定位置是否在可视区域内。
    /// </summary>
    /// <param name="x">单元格位置的列索引（x坐标）。</param>
    /// <param name="y">单元格位置的行索引（y坐标）。</param>
    /// <returns>如果单元格在可视区域内则返回 true，否则返回 false。</returns>
    public static bool IsInView(int x,int y)
    {
        return x >= 0 && x < instance.colNum && y >= ChessManager.instance.basePosition.y && y <= ChessManager.instance.highestRow;
    }
    /// <summary>
    /// 判断指定位置是否在可视区域内。
    /// </summary>
    /// <param name="cellPos">单元格位置。</param>
    /// <returns>如果单元格在可视区域内则返回 true，否则返回 false。</returns>
    public static bool IsInView(Vector2Int cellPos)
    {
        return IsInView(cellPos.x, cellPos.y);
    }
    public static Vector2Int ClampInView(Vector2Int cellPos)
    {
        return new Vector2Int(Mathf.Clamp(cellPos.x, 0, instance.colNum - 1), Mathf.Clamp(cellPos.y, ChessManager.instance.basePosition.y, ChessManager.instance.highestRow));
    }
    public static Vector2Int ClampInBorder(Vector2Int cellPos, Vector2Int maxBorder, Vector2Int minBorder)
    {
        return new Vector2Int(Mathf.Clamp(cellPos.x, minBorder.x, maxBorder.x), Mathf.Clamp(cellPos.y, minBorder.y, maxBorder.y));
    }
    public static Vector2Int FromTargetToOneStep(Vector2Int target, Vector2Int start)
    {
        Vector2Int dpos = target - start;
        if (dpos.x != 0)
        {
            return new(dpos.x / Mathf.Abs(dpos.x), 0);
        }else if (dpos.y != 0)
        {
            return new(0, dpos.y / Mathf.Abs(dpos.y));
        }
        else
        {
            return new(0, 0);
        }
    }
}
