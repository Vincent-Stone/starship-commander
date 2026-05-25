using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class FireMan : MovableEnemy
{
    Vector2Int baseCellPosition => Base.instance.cellPosition;
    Vector2Int playerCellPosition => Player.instance.cellPosition;
    Vector2Int attackDirection;
    Vector2Int moveRelativePosition;
    public Fireball fireball;
    [Header("影子")]
    public bool isInShadow;
    public SpriteRenderer shadow;
    [Header("火人信息")]
    public Sprite rookChessPicture;
    public string rookChessName = "火人";
    public string rookChessInfo = "BF团的干部之一。其体型与人类相似，由<b>火</b>组成，用火球攻击。";
    void Start()
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
        fireball.gameObject.SetActive(false);
    }
    public void Shoot(Vector2 shootDirection)
    {
        fireball.gameObject.SetActive(true);
        fireball.shooter = this;
        fireball.Shoot(shootDirection.normalized);
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
            isActing = false;
            frozenTurns--;
            ice.enabled = true;
            return;
        }
        ice.enabled = false;

        if (OnTheSameDiagonalLine(cellPosition, baseCellPosition) || OnTheSameDiagonalLine(cellPosition, playerCellPosition))
        {
            isActing = false;
            StartCoroutine(AttackCoroutine());
            return;
        }
        // 移动
        moveRelativePosition = Vector2Int.zero;
        Vector2Int moveTragetPosition = cellPosition;
        int stepsToTarget = (baseCellPosition.x + baseCellPosition.y) - (x + y), minStepCount = ChessBoard.instance.rowNum + ChessBoard.instance.colNum;
        moveTragetPosition = GetMoveTargetPosition(stepsToTarget, 1, 1);
        if (moveTragetPosition != cellPosition)
        {
            minStepCount = Mathf.Abs(stepsToTarget);
            moveRelativePosition = moveTragetPosition - cellPosition;
        }
        stepsToTarget = (baseCellPosition.x - baseCellPosition.y) - (x - y);
        moveTragetPosition = GetMoveTargetPosition(stepsToTarget, 1, -1);
        if (Mathf.Abs(stepsToTarget) < minStepCount && moveTragetPosition != cellPosition)
        {
            minStepCount = Mathf.Abs(stepsToTarget);
            moveRelativePosition = moveTragetPosition - cellPosition;
        }
        stepsToTarget = (playerCellPosition.x + playerCellPosition.y) - (x + y);
        moveTragetPosition = GetMoveTargetPosition(stepsToTarget, 1, 1);
        if (Mathf.Abs(stepsToTarget) < minStepCount && moveTragetPosition != cellPosition)
        {
            minStepCount = Mathf.Abs(stepsToTarget);
            moveRelativePosition = moveTragetPosition - cellPosition;
        }
        stepsToTarget = (playerCellPosition.x - playerCellPosition.y) - (x - y);
        moveTragetPosition = GetMoveTargetPosition(stepsToTarget, 1, -1);
        if (Mathf.Abs(stepsToTarget) < minStepCount && moveTragetPosition != cellPosition)
        {
            minStepCount = Mathf.Abs(stepsToTarget);
            moveRelativePosition = moveTragetPosition - cellPosition;
        }
        if (moveRelativePosition != Vector2Int.zero)
        {
            int dx = 0, dy = 0;
            List<Vector2Int> moveRange = GetMoveRange(x, y);
            int minStep = ChessBoard.instance.colNum + ChessBoard.instance.rowNum, step;
            foreach(Vector2Int range in moveRange)
            {
                step = Mathf.Abs((moveRelativePosition + cellPosition - range).x) + Mathf.Abs((moveRelativePosition + cellPosition - range).y);
                if (step < minStep)
                {
                    minStep = step;
                    dx = (range - cellPosition).x;
                    dy = (range - cellPosition).y;
                }
            }
            StartCoroutine(Move(dx, dy));
        }
        else
        {
            isActing = false;
        }
        StartCoroutine(AttackCoroutine());
    }
    IEnumerator AttackCoroutine()
    {
        while (isActing)
        {
            yield return null;
        }
        ChessManager.instance.PushActingChess(this);
        attackDirection = Vector2Int.zero;
        if (OnTheSameDiagonalLine(cellPosition, baseCellPosition))
        {
            attackDirection = baseCellPosition - cellPosition;
        }
        else if (OnTheSameDiagonalLine(cellPosition, playerCellPosition))
        {
            attackDirection = playerCellPosition - cellPosition;
        }
        if (attackDirection != Vector2Int.zero) // 攻击
        {
            Shoot(attackDirection);
        }
        else
        {
            isActing = false;
        }
    }
    Vector2Int GetMoveTargetPosition(int stepsToTarget,int dx,int dy)
    {
        Vector2Int moveTragetPosition = cellPosition + new Vector2Int(dx * (stepsToTarget / 2), dy * (stepsToTarget - stepsToTarget / 2));
        if (ChessBoard.IsInView(moveTragetPosition) && ChessBoard.GetChess(moveTragetPosition) == null)
            return moveTragetPosition;
        moveTragetPosition = cellPosition + new Vector2Int(dx * stepsToTarget, 0);
        if (ChessBoard.IsInView(moveTragetPosition) && ChessBoard.GetChess(moveTragetPosition) == null)
            return moveTragetPosition;
        moveTragetPosition = cellPosition + new Vector2Int(0, dy * stepsToTarget);
        if (ChessBoard.IsInView(moveTragetPosition) && ChessBoard.GetChess(moveTragetPosition) == null)
            return moveTragetPosition;
        moveTragetPosition = cellPosition;
        return moveTragetPosition;
    }
    bool OnTheSameDiagonalLine(Vector2Int point1, Vector2Int point2)
    {
        return (point1.x - point1.y == point2.x - point2.y)
            || (point1.x + point1.y == point2.x + point2.y);
    }
    public override List<Vector2Int> GetAttackRange(int targetX, int targetY, out int attackPriority, Chess thisChess)
    {
        attackPriority = 0;
        Vector2Int targetPos = new(targetX,targetY);
        List<Vector2Int> directions = new List<Vector2Int> {
            new(1,1),new(1,-1), new(-1,1), new(-1,-1)
        };
        List<Vector2Int> rangeList = new List<Vector2Int>();
        foreach (Vector2Int range in directions)
        {
            if (ChessBoard.IsInView(range + targetPos))
            {
                rangeList.Add(range + targetPos);
            }
        }
        return rangeList;
    }

    public override List<Vector2Int> GetMoveRange(int targetX, int targetY)
    {
        Vector2Int targetPos = new(targetX, targetY);
        List<Vector2Int> directions = new List<Vector2Int> {
            new(1,0),new(-1,0), new(0,1), new(0,-1)
        };
        List<Vector2Int> rangeList = new List<Vector2Int>();
        foreach (Vector2Int range in directions)
        {
            if (ChessBoard.IsInView(range + targetPos) && ChessBoard.GetChess(range + targetPos) == null)
            {
                rangeList.Add(range + targetPos);
            }
        }
        return rangeList;
    }
}
