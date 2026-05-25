using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Rook : MovableEnemy
{
    [Header("影子")]
    public bool isInShadow;
    public SpriteRenderer shadow;
    [Header("战车信息")]
    public Sprite rookChessPicture;
    public string rookChessName = "战车";
    public string rookChessInfo = "BF团唯一的超高速战斗飞船，可瞬间移动至正前方任意距离进行攻击。尽管从外形上抛弃了重力和空气动力，但名称依旧十分复古。";

    private void Start()
    {
        camp = 1;
        canBeRiden = true;
        chessTypeName = "Rook";
        hitPoints = maxHitPoints;
        hpUI.UpdateHP(hitPoints);
        isInShadow = true;
        shadow.enabled = true;
        chessPicture = null;
        chessName = chessInfo = "???";
    }

    public override void Act()
    {
        if (isInShadow)
        {
            if (StageManager.isBossStage)
            {
                isInShadow = false;
                shadow.enabled = false;
                chessPicture = rookChessPicture;
                chessName = rookChessName;
                chessInfo = rookChessInfo;
            }
            isActing = false;
            return;
        }
        if (frozenTurns > 0)
        {
            frozenTurns--;
            ice.enabled = true;
            isActing = false;
            return;
        }
        ice.enabled = false;
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
        ChessManager.instance.PushActingChess(this);
        A_Attack(attackTarget.x - x, attackTarget.y - y);
        moveTarget = GetBestMoveTarget(GetMoveRange(x, y), this);
        ChessManager.instance.PushActingChess(this);
        A_Move(moveTarget.x - x, moveTarget.y - y);
    }

    public override Vector2Int GetBestMoveTarget(List<Vector2Int> moveRange, Chess thisChess)
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
        if(bestTarget != cellPosition)
        {
            return bestTarget;
        }

        List<Vector2Int> nextStepList;
        int nextStepPriority = 0;
        foreach (Vector2Int target in moveRange)
        {
            nextStepList = GetMoveRange(target.x, target.y);
            nextStepPriority = 0;
            foreach (Vector2Int nextStep in nextStepList)
            {
                GetAttackRange(nextStep.x, nextStep.y, out int attackPriority, thisChess);
                nextStepPriority += attackPriority;
            }
            if (nextStepPriority > maxAttackPriority)
            {
                maxAttackPriority = nextStepPriority;
                bestTarget = target;
            }
        }
        return bestTarget;
    }

    public override List<Vector2Int> GetAttackRange(int targetX, int targetY, out int attackPriority, Chess thisChess)
    {
        attackPriority = 0;
        List<Vector2Int> rangeList = new List<Vector2Int>();
        if (targetY > 0)
        {
            rangeList.Add(new Vector2Int(targetX, targetY - 1));
            if (ChessBoard.instance[targetY - 1, targetX] != null && ChessBoard.instance[targetY - 1, targetX].camp != thisChess.camp)
            {
                int value = ChessBoard.instance[targetY - 1, targetX].value;
                attackPriority += value;
            }
        }
        if (targetX > 0)
        {
            rangeList.Add(new Vector2Int(targetX - 1, targetY));
            if (ChessBoard.instance[targetY, targetX - 1] != null && ChessBoard.instance[targetY, targetX - 1].camp != thisChess.camp)
            {
                int value = ChessBoard.instance[targetY, targetX - 1].value;
                attackPriority += value;
            }
        }
        if (targetX < ChessBoard.instance.colNum - 1)
        {
            rangeList.Add(new Vector2Int(targetX + 1, targetY));
            if (ChessBoard.instance[targetY, targetX + 1] != null && ChessBoard.instance[targetY, targetX + 1].camp != thisChess.camp)
            {
                int value = ChessBoard.instance[targetY, targetX + 1].value;
                attackPriority += value;
            }
        }
        if (targetY < ChessBoard.instance.rowNum - 1)
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
