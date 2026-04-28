using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ship : Chess
{
    internal Vector2Int moveTarget;
    private void Start()
    {
        camp = 1;
        canBeRiden = true;
        moveDuration = 0.1f;
    }
    public override void Act()
    {
        if (frozenTurns > 0)
        {
            frozenTurns--;
            isActing = false;
            return;
        }
        List<Vector2Int> moveRange = GetMoveRange(x, y, out List<Vector2Int> AP, this);
        List<Vector2Int> nextStepRange;
        int maxAttackPriority = 0, attackPriority = 0;
        moveTarget = new Vector2Int(x, y);
        foreach (Vector2Int pos in AP)
        {
            attackPriority = GetAttackPriority(pos.x, pos.y);
            if (attackPriority > maxAttackPriority)
            {
                maxAttackPriority = attackPriority;
                moveTarget = pos;
            }
        }
        if(moveTarget.x == x && moveTarget.y == y)
        {
            foreach (Vector2Int pos in moveRange)
            {
                attackPriority = GetAttackPriority(pos.x, pos.y);
                if (attackPriority > maxAttackPriority)
                {
                    maxAttackPriority = attackPriority;
                    moveTarget = pos;
                }
            }
        }
        if (moveTarget.x != x || moveTarget.y != y)
        {
            StartCoroutine(Move(moveTarget.x - x, moveTarget.y - y));
        }
        else
        {
            isActing = false;
        }
    }

    internal virtual int GetAttackPriority(int targetX, int targetY)
    {
        int priority = 0;
        Chess thisChess = this;
        if (rider != null)
            thisChess = rider;
        Chess target = ChessBoard.instance[targetY, targetX];
        if (target != null)
        {
            if (target.camp != thisChess.camp)
            {
                priority += target.value;
            }
        }
        GetMoveRange(targetX, targetY, out List<Vector2Int> AP, thisChess);
        priority += AP.Count;
        return priority;
    }

    public override List<Vector2Int> GetMoveRange()
    {
        List<Vector2Int> attackPointsCount;
        if (rider != null)
            return GetMoveRange(x, y, out attackPointsCount, rider);
        return GetMoveRange(x, y, out attackPointsCount, this);
    }
    public override List<Vector2Int> GetAttackRange()
    {
        throw new System.NotImplementedException();
    }
    internal bool CanMoveTo(int targetX, int targetY, Chess thisChess)
    {
        if (ChessBoard.IsOnBoard(targetX, targetY) && (ChessBoard.instance[targetY, targetX] == null
            || ChessBoard.instance[targetY, targetX].camp != thisChess.camp))
        {
            return true;
        }
        return false;
    }

    public abstract List<Vector2Int> GetMoveRange(int targetX, int targetY, out List<Vector2Int> attackPoints, Chess thisChess);
}
