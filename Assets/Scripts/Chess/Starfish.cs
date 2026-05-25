using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Starfish : MovableEnemy
{
    void Start()
    {
        camp = 1;
        canBeRiden = true;
        chessTypeName = "Starfish";
        chessName = "海星";
        chessInfo = "由BF团培育的神秘星形海洋生物，能斜向发动攻击";
        hitPoints = maxHitPoints;
        hpUI.UpdateHP(hitPoints);
    }
    public override List<Vector2Int> GetAttackRange(int targetX, int targetY, out int attackPriority, Chess thisChess)
    {
        attackPriority = 0;
        List<Vector2Int> directions = new List<Vector2Int> {
            new(1,1),new(1,-1), new(-1,1), new(-1,-1)
        };
        List<Vector2Int> rangeList = new List<Vector2Int>();
        foreach (Vector2Int range in directions)
        {
            Vector2Int pos = range + new Vector2Int(targetX, targetY);
            if (ChessBoard.IsInView(pos))
            {
                rangeList.Add(pos);
                if (ChessBoard.GetChess(pos) != null
                && ChessBoard.GetChess(pos).camp != camp)
                    attackPriority += ChessBoard.GetChess(pos).value;
            }
        }
        return rangeList;
    }

    public override List<Vector2Int> GetMoveRange(int targetX, int targetY)
    {
        List<Vector2Int> directions = new List<Vector2Int> {
            new(1,0),new(-1,0), new(0,1), new(0,-1)
        };
        List<Vector2Int> rangeList = new List<Vector2Int>();
        foreach (Vector2Int range in directions)
        {
            Vector2Int pos = range + new Vector2Int(targetX, targetY);
            if (ChessBoard.IsInView(pos)
                && ChessBoard.GetChess(pos) == null)
            {
                rangeList.Add(pos);
            }
        }
        return rangeList;
    }

    public override Vector2Int GetBestMoveTarget(List<Vector2Int> moveRange, Chess thisChess)
    {
        Vector2Int bestTarget = cellPosition;
        int maxAttackPriority = 0;
        foreach (Vector2Int target in moveRange)
        {
            GetAttackRange(target.x, target.y, out int attackPriority, thisChess);
            if (attackPriority > maxAttackPriority)
            {
                maxAttackPriority = attackPriority;
                bestTarget = target;
            }
        }
        if(bestTarget == cellPosition)
        {
            Vector2Int targetPos,oneStepPos;
            int minDistance = ChessBoard.instance.colNum + ChessBoard.instance.rowNum,distance;
            List<Vector2Int> directions = new List<Vector2Int> {
                new(1,1),new(1,-1), new(-1,1), new(-1,-1)
            };
            foreach(Vector2Int dir in directions)
            {
                targetPos = Base.instance.cellPosition + dir;
                distance = Mathf.Abs(targetPos.x - x) + Mathf.Abs(targetPos.y - y);
                if (distance < minDistance && ChessBoard.IsInView(targetPos))
                {
                    oneStepPos = ChessBoard.FromTargetToOneStep(targetPos, cellPosition) + cellPosition;
                    if(ChessBoard.IsInView(oneStepPos) && ChessBoard.GetChess(oneStepPos) == null)
                    {
                        minDistance = distance;
                        bestTarget = oneStepPos;
                    }
                }
            }
            foreach (Vector2Int dir in directions)
            {
                targetPos = Player.instance.cellPosition + dir;
                distance = Mathf.Abs(targetPos.x - x) + Mathf.Abs(targetPos.y - y);
                if (distance < minDistance && ChessBoard.IsInView(targetPos))
                {
                    oneStepPos = ChessBoard.FromTargetToOneStep(targetPos, cellPosition) + cellPosition;
                    if (ChessBoard.IsInView(oneStepPos) && ChessBoard.GetChess(oneStepPos) == null)
                    {
                        minDistance = distance;
                        bestTarget = oneStepPos;
                    }
                }
            }
        }
        return bestTarget;
    }
}
