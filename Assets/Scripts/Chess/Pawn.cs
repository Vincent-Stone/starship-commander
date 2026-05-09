using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MovableEnemy
{
    private void Start()
    {
        camp = 1;
        canBeRiden = true;
        moveDuration = 0.1f;
        chessTypeName = "Pawn";
    }
    public override List<Vector2Int> GetAttackRange(int targetX, int targetY, out int attackPriority, Chess thisChess)
    {
        throw new System.NotImplementedException();
    }

    public override List<Vector2Int> GetMoveRange(int targetX, int targetY)
    {
        Vector2Int[] moveDirection = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0) };
        List<Vector2Int> rangeList = new List<Vector2Int>();
        foreach(Vector2Int dir in moveDirection)
        {
            Vector2Int next = new Vector2Int(targetX, targetY) + dir;
            if(ChessBoard.IsOnBoard(next.x,next.y) && ChessBoard.GetChess(next) == null)
            {
                rangeList.Add(next);
            }
        }
        return rangeList;
    }
}
