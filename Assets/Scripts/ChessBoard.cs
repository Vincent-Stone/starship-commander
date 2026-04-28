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
    public GameObject rangePrefab = null;
    SpriteRenderer[,] rangeSprites = null;
    public static ChessBoard instance = null;
    public ChessManager chessManager = null;
    public static Tilemap tilemap;
    public void Init(int rowNum, int colNum, ChessManager chessManager)
    {
        instance = this;
        
        if (chessManager.tilemap == null)
        {
            Debug.LogError("Tilemap is not assigned in ChessBoard.");
        }
        else
        {
            tilemap = chessManager.tilemap;
        }
        this.chessManager = chessManager;
        this.rangePrefab = Resources.Load("Prefabs/Range") as GameObject;
        this.rowNum = rowNum;
        this.colNum = colNum;
        chessBoard = new Chess[rowNum,colNum];
        rangeSprites = new SpriteRenderer[rowNum, colNum];
        for (int i = 0; i < rowNum; i++)
        {
            for (int j = 0; j < colNum; j++)
            {
                rangeSprites[i, j] = GameObject.Instantiate(rangePrefab, GetCellCenterWorld(i, j), Quaternion.identity).GetComponent<SpriteRenderer>();
                rangeSprites[i, j].transform.parent = chessManager.transform;
                rangeSprites[i, j].color = new Color(1, 1, 1, 0.1f); // Set initial transparency to 0
            }
        }
    }
    public Chess this[int row,int col]
    {
        get
        {
            return chessBoard[row, col]; // 返回指定行列的值
        }
        set
        {
            chessBoard[row, col] = value; // 设置指定行列的值
        }
    }
    public static Chess GetChess(Vector2Int cellPos)
    {
        if (IsOnBoard(cellPos.x, cellPos.y))
        {
            return chessBoard[cellPos.y, cellPos.x];
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
    public static bool IsOnBoard(int x, int y)
    {
        return x >= 0 && x < instance.colNum && y >= 0 && y < instance.rowNum;
    }
}
