using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovableEnemy : Chess
{
    public struct MoveInfo
    {
        public Vector2Int position;
        public int attackPriority;
        public Vector2Int attackPoint;
        public MoveInfo(Vector2Int position, int attackPriority, Vector2Int attackPoint)
        {
            this.position = position;
            this.attackPriority = attackPriority;
            this.attackPoint = attackPoint;
        }
    }

    internal Vector2Int moveTarget;
    internal bool isAttackPhase = true;
    public int attackPower = 1;
    private void Start()
    {
        camp = 1;
        canBeRiden = true;
    }
    public override void Act()
    {
        if (frozenTurns > 0)
        {
            isActing = false;
            if (isAttackPhase)
            {
                ice.enabled = true;
                frozenTurns--;
                isAttackPhase = false;
            }
            else
            {
                isAttackPhase = true;
            }
            return;
        }
        ice.enabled = false;
        if (isAttackPhase)
        {
            isAttackPhase = false;
            Vector2Int attackTarget = new Vector2Int(x, y);
            int maxAttackPriority = 0;
            List<Vector2Int> attackRange = GetAttackRange(x, y, out int attackPriority, this);
            foreach (Vector2Int attackPoint in attackRange)
            {
                Chess targetChess = ChessBoard.GetChess(attackPoint);
                if (targetChess != null && targetChess.camp != this.camp)
                {
                    int value = targetChess.value;
                    if (value > maxAttackPriority)
                    {
                        maxAttackPriority = value;
                        attackTarget = attackPoint;
                    }
                }
            }
            A_Attack(attackTarget.x - x, attackTarget.y - y);
        }
        else
        {
            isAttackPhase = true;
            moveTarget = GetBestMoveTarget(GetMoveRange(x, y), this);
            A_Move(moveTarget.x - x, moveTarget.y - y);
        }
    }
    public virtual Vector2Int GetBestMoveTarget(List<Vector2Int> moveRange, Chess thisChess)
    {
        Vector2Int bestTarget = new Vector2Int(x, y);
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
        return bestTarget;
    }
    public void A_Attack(int dx,int dy)
    {
        if (dx == 0 && dy == 0)
            ActEnd();
        else
        {
            StartCoroutine(Attack(x + dx, y + dy));
        }
    }

    public void A_Move(int dx, int dy)
    {
        if (dx == 0 && dy == 0)
            ActEnd();
        else
        {
            StartCoroutine(Move(dx, dy));
        }
    }
    public virtual IEnumerator Attack(int x,int y)
    {
        Chess targetChess = ChessBoard.GetChess(new Vector2Int(x, y));
        targetChess.TakeDamage(attackPower, this, new Vector2Int(x - this.x, y - this.y));
        isActing = false;
        yield break;
    }

    public virtual void ChessBoardMove(int dx, int dy)
    {
        if (ChessBoard.instance[y, x] == this)
            ChessBoard.instance[y, x] = null;
        x += dx;
        y += dy;
        ChessBoard.instance[y, x] = this;
    }

    public override void ForcedMove()
    {
        if (!canMove)
        {
            isActing = false;
            return;
        }
        ChessManager.instance.PushActingChess(this);
        Vector2Int forcedMoveTarget = GetForcedMoveTarget();
        if (forcedMoveTarget.x != x || forcedMoveTarget.y != y)
        {
            ChessBoardMove(forcedMoveTarget.x - x, forcedMoveTarget.y - y);
            StartCoroutine(ForcedMovingCoroutine(forcedMoveTarget));
        }
        else
        {
            isActing = false;
        }
    }

    IEnumerator Move(int dx, int dy)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = transform.position + new Vector3(dx, dy, 0);
        Chess target = ChessBoard.instance[y + dy, x + dx];
        ChessBoardMove(dx, dy);
        for(float t = 0; t < moveDuration; t += Time.deltaTime)
        {
            while (StageManager.isPaused) // ¿ÉÔÝÍ£
                yield return null;
            transform.position = Vector3.Lerp(startPosition, endPosition, t / moveDuration);
            yield return null;
        }
        transform.position = endPosition;
        isActing = false;
        yield break;
    }
    public override List<Vector2Int> GetMoveRange()
    {
        List<MoveInfo> attackPointsCount;
        if (rider != null)
            return GetMoveRange(x, y);
        return GetMoveRange(x, y);
    }
    public override List<Vector2Int> GetAttackRange()
    {
        List<Vector2Int> rangeList = GetAttackRange(x, y, out int attackPriority, this);
        return rangeList;
    }
    public override void ShowRange()
    {
        base.ShowRange();
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
    public abstract List<Vector2Int> GetAttackRange(int targetX, int targetY, out int attackPriority, Chess thisChess);
    public abstract List<Vector2Int> GetMoveRange(int targetX, int targetY);
}
