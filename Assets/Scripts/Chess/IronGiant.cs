using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IronGiant : MovableEnemy
{
    List<Vector2Int> volume = new List<Vector2Int> { 
        new(-1, -1), 
        new(-1, 0), 
        new(-1, 1), 
        new(0, -1),
        new(0, 0),
        new(0, 1), 
        new(1, -1), 
        new(1, 0), 
        new(1, 1),
    };
    [Header("影子")]
    public bool isInShadow;
    public SpriteRenderer shadow;
    [Header("铁人信息")]
    public Sprite imChessPicture;
    public string imChessName = "铁人";
    public string imChessInfo = "BF团使用不可思议金属铸造的巨人士兵，攻击力强大。";
    public bool isInList = false;
    void Start()
    {
        camp = 1;
        canBeRiden = true;
        chessTypeName = "IronGiant";
        hitPoints = maxHitPoints;
        hpUI.UpdateHP(hitPoints);
        isInShadow = true;
        shadow.enabled = true;
        chessPicture = null;
        chessName = chessInfo = "???";
        isInList = false;
    }
    public override void Act()
    {
        ChessBoardMove(0, 0);
        if (isInShadow)
        {
            if (StageManager.isBossStage)
            {
                isInShadow = false;
                shadow.enabled = false;
                chessPicture = imChessPicture;
                chessName = imChessName;
                chessInfo = imChessInfo;
            }
            isActing = false;
            return;
        }
        if (frozenTurns > 0)
        {
            isActing = false;
            frozenTurns--;
            ice.enabled = true;
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

    public override List<Vector2Int> GetAttackRange(int targetX, int targetY, out int attackPriority, Chess thisChess)
    {
        Vector2Int startCellPosition = new Vector2Int(targetX, targetY);
        List<Vector2Int> fullRange = new List<Vector2Int> { 
            new Vector2Int(-2,-2) + startCellPosition, 
            new Vector2Int(-2,-1) + startCellPosition,
            new Vector2Int(-2,0) + startCellPosition,
            new Vector2Int(-2,1) + startCellPosition,
            new Vector2Int(-2,2) + startCellPosition,
            new Vector2Int(-1,-2) + startCellPosition,
            new Vector2Int(-1,2) + startCellPosition,
            new Vector2Int(0,-2) + startCellPosition,
            new Vector2Int(0,2) + startCellPosition,
            new Vector2Int(1,-2) + startCellPosition,
            new Vector2Int(1,2) + startCellPosition,
            new Vector2Int(2,-2) + startCellPosition,
            new Vector2Int(2,-1) + startCellPosition,
            new Vector2Int(2,0) + startCellPosition,
            new Vector2Int(2,1) + startCellPosition,
            new Vector2Int(2,2) + startCellPosition,
        };
        attackPriority = 0;
        List<Vector2Int> rangeList = new List<Vector2Int>();
        Chess targetChess;
        foreach(Vector2Int rangePos in fullRange)
        {
            if (ChessBoard.IsInView(rangePos))
            {
                rangeList.Add(rangePos);
                targetChess = ChessBoard.GetChess(rangePos);
                if(targetChess!= null && targetChess.camp != thisChess.camp)
                {
                    attackPriority += targetChess.value;
                }
            }
        }
        return rangeList;
    }

    internal override bool CanForcedMoveTo(int targetX, int targetY)
    {
        Vector2Int dTarget;
        bool canMove = true;
        foreach(Vector2Int pos in volume)
        {
            dTarget = pos + new Vector2Int(targetX, targetY);
            if(!ChessBoard.IsInView(dTarget) || ChessBoard.GetChess(dTarget) != null && ChessBoard.GetChess(dTarget) != this)
            {
                canMove = false;
                break;
            }
        }
        return canMove;
    }

    public override List<Vector2Int> GetMoveRange(int targetX, int targetY)
    {
        List<Vector2Int> rangeList = new List<Vector2Int>();
        Func<int, int, bool> canMoveTo = (int dx, int dy) =>
        {
            return ChessBoard.IsInView(dx + targetX, dy + targetY) && ChessBoard.GetChess(dx + targetX, dy + targetY) == null;
        };
        if (canMoveTo(-1, -2) && canMoveTo(0, -2) && canMoveTo(1, -2))
        {
            rangeList.Add(new Vector2Int(targetX, targetY - 1));
        }
        if (canMoveTo(-1, 2) && canMoveTo(0, 2) && canMoveTo(1, 2))
        {
            rangeList.Add(new Vector2Int(targetX, targetY + 1));
        }
        if (canMoveTo(-2, -1) && canMoveTo(-2, 0) && canMoveTo(-2, 1))
        {
            rangeList.Add(new Vector2Int(targetX - 1, targetY));
        }
        if (canMoveTo(2, -1) && canMoveTo(2, 0) && canMoveTo(2, 1))
        {
            rangeList.Add(new Vector2Int(targetX + 1, targetY));
        }
        return rangeList;
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
        if(bestTarget == cellPosition)
        {
            //Vector2Int relativepos = Player.instance.cellPosition - cellPosition;
            //Vector2Int pos;
            int minStep = ChessBoard.instance.colNum + ChessBoard.instance.rowNum, step;
            foreach(Vector2Int target in moveRange)
            {
                step = Mathf.Abs(target.x - x) + Mathf.Abs(target.y - y);
                if(step < minStep)
                {
                    minStep = step;
                    bestTarget = target;
                }
            }
        }
        return bestTarget;
    }

    public override void ChessBoardMove(int dx, int dy)
    {
        foreach(Vector2Int pos in volume)
        {
            if (ChessBoard.GetChess(pos + cellPosition) == this)
                ChessBoard.instance[pos.y + y, pos.x + x] = null;
        }
        x += dx;
        y += dy;
        foreach (Vector2Int pos in volume)
        {
            ChessBoard.instance[pos.y + y, pos.x + x] = this;
            //Debug.LogError(pos+" "+ ChessBoard.GetChess(pos + cellPosition));
        }
    }
}
