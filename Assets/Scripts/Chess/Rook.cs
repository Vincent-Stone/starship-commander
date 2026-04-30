using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Rook : MovableEnemy
{
    private void Start()
    {
        camp = 1;
        canBeRiden = true;
        moveDuration = 0.1f;
        chessTypeName = "Rook";
    }

    public override List<Vector2Int> GetAttackRange(int targetX, int targetY, out int attackPriority, Chess thisChess)
    {
        attackPriority = 0;
        List<Vector2Int> rangeList = new List<Vector2Int>();
        if (y > 0)
        {
            rangeList.Add(new Vector2Int(targetX, targetY - 1));
            if (ChessBoard.instance[targetY - 1, targetX] != null && ChessBoard.instance[targetY - 1, targetX].camp != thisChess.camp)
            {
                int value = ChessBoard.instance[targetY - 1, targetX].value;
                attackPriority += value;
            }
        }
        if (x > 0)
        {
            rangeList.Add(new Vector2Int(targetX - 1, targetY));
            if (ChessBoard.instance[targetY, targetX - 1] != null && ChessBoard.instance[targetY, targetX - 1].camp != thisChess.camp)
            {
                int value = ChessBoard.instance[targetY, targetX - 1].value;
                attackPriority += value;
            }
        }
        if (x < ChessBoard.instance.colNum - 1)
        {
            rangeList.Add(new Vector2Int(targetX + 1, targetY));
            if (ChessBoard.instance[targetY, targetX + 1] != null && ChessBoard.instance[targetY, targetX + 1].camp != thisChess.camp)
            {
                int value = ChessBoard.instance[targetY, targetX + 1].value;
                attackPriority += value;
            }
        }
        if (y < ChessBoard.instance.rowNum - 1)
        {
            rangeList.Add(new Vector2Int(targetX, targetY + 1));
            if(ChessBoard.instance[targetY + 1, targetX] != null && ChessBoard.instance[targetY + 1, targetX].camp != thisChess.camp)
            {
                int value = ChessBoard.instance[targetY + 1, targetX].value;
                attackPriority += value;
            }
        }
        return rangeList;
    }

    public override List<Vector2Int> GetMoveRange(int targetX,int targetY)
    {
        List<Vector2Int> rangeList = new List<Vector2Int>();
        rangeList.Add(new Vector2Int(targetX, targetY));
        int up = 1, down = 1, left = 1, right = 1;
        bool end = false;
        Chess cur = ChessBoard.instance[targetY, targetX], next;
        for (up = 1; up + targetY < ChessBoard.instance.rowNum; up++)
        {
            next = ChessBoard.instance[targetY + up, targetX];
            if (next == null)
            {
                rangeList.Add(new Vector2Int(targetX, targetY + up));
            }
            else
                break;
        }
        for (down = 1; targetY - down >= 0; down++)
        {
            next = ChessBoard.instance[targetY - down, targetX];
            if (next == null)
            {
                rangeList.Add(new Vector2Int(targetX, targetY - down));
            }
            else
                break;
        }
        for (right = 1; right + targetX < ChessBoard.instance.colNum; right++)
        {
            next = ChessBoard.instance[targetY, targetX + right];
            if (next == null)
            {
                rangeList.Add(new Vector2Int(targetX + right, targetY));
            }
            else
                break;
        }
        for (left = 1; targetX - left >= 0; left++)
        {
            next = ChessBoard.instance[targetY, targetX - left];
            if (next == null)
            {
                rangeList.Add(new Vector2Int(targetX - left, targetY));
            }
            else
                break;
        }
        //Debug.Log("left=" + left + " right=" + right + " up=" + up + " down=" + down);
        return rangeList;
    }
}
