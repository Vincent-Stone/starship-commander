using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : MovableEnemy
{
    int[,] stepMap = new int[10, 9] {
        { 2 , 3 , 2 , 3 , 0 , 3 , 2 , 3 , 2 },
        { 3 , 2 , 1 , 2 , 3 , 2 , 1 , 2 , 3 },
        { 2 , 3 , 4 , 1 , 2 , 1 , 4 , 3 , 2 },
        { 3 , 2 , 3 , 2 , 3 , 2 , 3 , 2 , 3 },
        { 4 , 3 , 2 , 3 , 2 , 3 , 2 , 3 , 4 },
        { 3 , 4 , 3 , 4 , 3 , 4 , 3 , 4 , 3 },
        { 4 , 3 , 4 , 3 , 4 , 3 , 4 , 3 , 4 },
        { 5 , 4 , 5 , 4 , 5 , 4 , 5 , 4 , 5 },
        { 4 , 5 , 4 , 5 , 4 , 5 , 4 , 5 , 4 },
        { 5 , 6 , 5 , 6 , 5 , 6 , 5 , 6 , 5 }
    };
    private void Start()
    {
        camp = 1;
        canBeRiden = true;
        moveDuration = 0.1f;
        chessTypeName = "Knight";
    }
    

    internal bool CanMoveTo(int targetX, int targetY, Chess thisChess)
    {
        if (ChessBoard.IsOnBoard(targetX, targetY))
        {
            Chess target = ChessBoard.instance[targetY, targetX];
            return target == null || target.camp != thisChess.camp || target.camp != this.camp && target.canBeRiden;
        }
        return false;
    }

    //public override List<Vector2Int> GetAttackRange(int targetX, int targetY, out List<Vector2Int> movePoints, Chess thisChess)
    //{
    //    movePoints = new List<Vector2Int>();
    //    List<Vector2Int> rangeList = new List<Vector2Int>();
    //    Func<int, int, List<Vector2Int>> rangeFunc = (int x, int y) =>
    //    {
    //        List<Vector2Int> movePoints = new List<Vector2Int>();
    //        if (CanMoveTo(x, y, thisChess))
    //        {
    //            if (ChessBoard.instance[y, x] == null)
    //            {
    //                movePoints.Add(new Vector2Int(x, y));
    //            }
    //            rangeList.Add(new Vector2Int(x, y));
    //        }
    //        return movePoints;
    //    };
    //    movePoints.AddRange(rangeFunc(x + 1, y + 2));
    //    movePoints.AddRange(rangeFunc(x + 2, y + 1));
    //    movePoints.AddRange(rangeFunc(x - 1, y + 2));
    //    movePoints.AddRange(rangeFunc(x - 2, y + 1));
    //    movePoints.AddRange(rangeFunc(x + 1, y - 2));
    //    movePoints.AddRange(rangeFunc(x + 2, y - 1));
    //    movePoints.AddRange(rangeFunc(x - 1, y - 2));
    //    movePoints.AddRange(rangeFunc(x - 2, y - 1));
    //    return rangeList;
    //}

    //public override List<Vector2Int> GetMoveRange(int targetX, int targetY, out List<Vector2Int> attackPoints, Chess thisChess)
    //{
    //    attackPoints = new List<Vector2Int>();
    //    List<Vector2Int> rangeList = new List<Vector2Int>();
    //    Func<int, int, List<Vector2Int>> rangeFunc = (int x, int y) =>
    //    {
    //        List<Vector2Int> attackPoints = new List<Vector2Int>();
    //        if (CanMoveTo(x, y, thisChess))
    //        {
    //            if (ChessBoard.instance[y, x] != null)
    //            {
    //                attackPoints.Add(new Vector2Int(x, y));
    //            }
    //            rangeList.Add(new Vector2Int(x, y));
    //        }
    //        return attackPoints;
    //    };
    //    attackPoints.AddRange(rangeFunc(x + 1, y + 2));
    //    attackPoints.AddRange(rangeFunc(x + 2, y + 1));
    //    attackPoints.AddRange(rangeFunc(x - 1, y + 2));
    //    attackPoints.AddRange(rangeFunc(x - 2, y + 1));
    //    attackPoints.AddRange(rangeFunc(x + 1, y - 2));
    //    attackPoints.AddRange(rangeFunc(x + 2, y - 1));
    //    attackPoints.AddRange(rangeFunc(x - 1, y - 2));
    //    attackPoints.AddRange(rangeFunc(x - 2, y - 1));
    //    return rangeList;
    //}

    public override List<Vector2Int> GetMoveRange(int targetX, int targetY)
    {
        throw new NotImplementedException();
    }

    public override List<Vector2Int> GetAttackRange(int targetX, int targetY, out int attackPriority, Chess thisChess)
    {
        throw new NotImplementedException();
    }
}