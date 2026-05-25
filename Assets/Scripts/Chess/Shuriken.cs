using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shuriken : Chess
{
    bool Acted = false; // 记录本回合是否行动过，一回合只能行动一次
    Vector2Int baseCellPosition => Base.instance.cellPosition;
    Vector2Int playerCellPosition => Player.instance.cellPosition;
    Vector2Int attackDirection;
    Vector2Int moveRelativePosition;
    [Header("来自Bullet")]
    public Chess shooter;
    internal Chess hitChess;
    internal Chess lastHitChess;
    [Header("速度")]
    public float flyingSpeed;
    public float startRSpeed = 0.1f;
    public float maxRSpeed = 0.1f;
    public float rAcceleration = 0.1f;
    [Header("弹跳次数")]
    public float bounceTime = 1;
    Vector2 velocity;
    Vector2 MaxBorder
    {
        get
        {
            return new Vector2(4.5f, 5 - 9 + ChessManager.instance.highestRow);
        }
    }
    Vector2 MinBorder
    {
        get
        {
            return new Vector2(-4.5f, -5 - 9 + ChessManager.instance.highestRow);
        }
    }
    void Start()
    {
        camp = 1;
        canBeRiden = false;
        chessTypeName = "Shuriken";
        chessName = "手里剑=SAN";
        chessInfo = "高速旋转，攻击所有斜线上的敌人，会被边界反弹。\n由普通合金制成会因反弹时的碰撞受损。";
        hitPoints = maxHitPoints;
        hpUI.UpdateHP(hitPoints);
        attackDirection = Vector2Int.zero;
    }
    public override void Act()
    {
        if (Acted)
        {
            Acted = false;
            isActing = false;
            return;
        }
        Acted = true;
        if (frozenTurns > 0)
        {
            isActing = false;
            frozenTurns--;
            ice.enabled = true;
            return;
        }
        ice.enabled = false;
        if (OnTheSameDiagonalLine(cellPosition, baseCellPosition))
        {
            attackDirection = baseCellPosition - cellPosition;
        }
        else if(OnTheSameDiagonalLine(cellPosition, playerCellPosition))
        {
            attackDirection = playerCellPosition - cellPosition;
        }
        if(attackDirection != Vector2Int.zero) // 攻击
        {
            Shoot(attackDirection);
            return;
        }
        // 移动
        moveRelativePosition = Vector2Int.zero;
        Vector2Int moveTragetPosition = cellPosition;
        int stepsToTarget = x + y - (baseCellPosition.x + baseCellPosition.y), minStepCount = Mathf.Abs(stepsToTarget);
        moveTragetPosition = cellPosition + new Vector2Int(stepsToTarget / 2, stepsToTarget - stepsToTarget / 2);
        if (moveTragetPosition.y >= ChessBoard.instance.bossAreaLine)
        {
            moveTragetPosition = cellPosition + new Vector2Int(stepsToTarget, 0);
        }
        if (ChessBoard.IsInView(moveTragetPosition) && ChessBoard.GetChess(moveTragetPosition) == null)
        {
            moveRelativePosition = moveTragetPosition - cellPosition;
        }
        stepsToTarget = x - y - (baseCellPosition.x - baseCellPosition.y);
        moveTragetPosition = cellPosition + new Vector2Int(stepsToTarget / 2, stepsToTarget - stepsToTarget / 2);
        if (moveTragetPosition.y >= ChessBoard.instance.bossAreaLine)
        {
            moveTragetPosition = cellPosition + new Vector2Int(stepsToTarget, 0);
        }
        if (Mathf.Abs(stepsToTarget) < minStepCount && ChessBoard.IsInView(moveTragetPosition) && ChessBoard.GetChess(moveTragetPosition) == null)
        {
            minStepCount = Mathf.Abs(stepsToTarget);
            moveRelativePosition = moveTragetPosition - cellPosition;
        }
        stepsToTarget = x + y - (playerCellPosition.x + playerCellPosition.y);
        moveTragetPosition = cellPosition + new Vector2Int(stepsToTarget / 2, stepsToTarget - stepsToTarget / 2);
        if (moveTragetPosition.y >= ChessBoard.instance.bossAreaLine)
        {
            moveTragetPosition = cellPosition + new Vector2Int(stepsToTarget, 0);
        }
        if (Mathf.Abs(stepsToTarget) < minStepCount && ChessBoard.IsInView(moveTragetPosition) && ChessBoard.GetChess(moveTragetPosition) == null)
        {
            minStepCount = Mathf.Abs(stepsToTarget);
            moveRelativePosition = moveTragetPosition - cellPosition;
        }
        stepsToTarget = x - y - (playerCellPosition.x - playerCellPosition.y);
        moveTragetPosition = cellPosition + new Vector2Int(stepsToTarget / 2, stepsToTarget - stepsToTarget / 2);
        if (moveTragetPosition.y >= ChessBoard.instance.bossAreaLine)
        {
            moveTragetPosition = cellPosition + new Vector2Int(stepsToTarget, 0);
        }
        if (Mathf.Abs(stepsToTarget) < minStepCount && ChessBoard.IsInView(moveTragetPosition) && ChessBoard.GetChess(moveTragetPosition) == null)
        {
            minStepCount = Mathf.Abs(stepsToTarget);
            moveRelativePosition = moveTragetPosition - cellPosition;
        }
        if (moveRelativePosition != Vector2Int.zero)
        {
            int dx = 0, dy = 0;
            if (moveRelativePosition.x != 0)
            {
                dx = moveRelativePosition.x / Mathf.Abs(moveRelativePosition.x);
            }
            else if (moveRelativePosition.y != 0)
            {
                dy = moveRelativePosition.y / Mathf.Abs(moveRelativePosition.y);
            }
            StartCoroutine(Move(dx, dy));
        }
        else
        {
            isActing = false;
        }
    }
    //private IEnumerator Move(int dx, int dy)
    //{
    //    Vector3 startPosition = transform.position;
    //    Vector3 endPosition = transform.position + new Vector3(dx, dy);
    //    Chess moveTarget = ChessBoard.instance[this.y + dy, this.x + dx];
    //    if (ChessBoard.instance[this.y, this.x] == this)
    //        ChessBoard.instance[this.y, this.x] = null;
    //    x += dx;
    //    y += dy;
    //    ChessBoard.instance[this.y, this.x] = this;
    //    for (float t = 0; t < 1f; t += (Time.deltaTime / moveDuration))
    //    {
    //        while (StageManager.isPaused) // 可暂停
    //            yield return null;
    //        transform.position = Vector3.Lerp(startPosition, endPosition, t);
    //        yield return null;
    //    }
    //    transform.position = endPosition;
    //    isActing = false;
    //}

    bool OnTheSameDiagonalLine(Vector2Int point1, Vector2Int point2)
    {
        return (point1.x - point1.y == point2.x - point2.y)
            || (point1.x + point1.y == point2.x + point2.y);
    }
    Vector2Int GetCrossPoint(Vector2Int point1, Vector2Int point2)
    {
        return new((point1.x + point1.y - point2.x + point2.y) / 2, (point1.x + point1.y + point2.x - point2.y) / 2);
    }
    public override List<Vector2Int> GetAttackRange()
    {
        List<Vector2Int> directions = new List<Vector2Int> {
            new(1,1),new(1,-1), new(-1,1), new(-1,-1)
        };
        List<Vector2Int> rangeList = new List<Vector2Int>();
        foreach(Vector2Int range in directions)
        {
            if (ChessBoard.IsInView(range + cellPosition))
            {
                rangeList.Add(range + cellPosition);
            }
        }
        return rangeList;
    }
    public override List<Vector2Int> GetMoveRange()
    {
        List<Vector2Int> directions = new List<Vector2Int> {
            new(1,0),new(-1,0), new(0,1), new(0,-1)
        };
        List<Vector2Int> rangeList = new List<Vector2Int>();
        foreach (Vector2Int range in directions)
        {
            if (ChessBoard.IsInView(range + cellPosition))
            {
                rangeList.Add(range + cellPosition);
            }
        }
        return rangeList;
    }
    public void Shoot(Vector2 shootDirection)
    {
        velocity = shootDirection.normalized * flyingSpeed;
        if (ChessBoard.instance[this.y, this.x] == this)
            ChessBoard.instance[y, x] = null;
        lastHitChess = null;
        StartCoroutine(Flying());
    }
    public IEnumerator Flying()
    {
        float s;
        for (s = startRSpeed; s < maxRSpeed; s += rAcceleration)
        {
            sprite.Rotate(new(0, 0, s * 360 * Time.deltaTime));
            yield return null;
        }
        int hitTime = 0;
        while (true)
        {
            transform.position += (Vector3)velocity * Time.deltaTime;
            if (transform.position.x > MaxBorder.x)
            {
                hitTime++;
                velocity.x = -velocity.x;
                transform.position = new Vector3(MaxBorder.x * 2 - transform.position.x, transform.position.y, transform.position.z);
            }
            else if (transform.position.x < MinBorder.x)
            {
                hitTime++;
                velocity.x = -velocity.x;
                transform.position = new Vector3(MinBorder.x * 2 - transform.position.x, transform.position.y, transform.position.z);
            }
            if (transform.position.y > MaxBorder.y)
            {
                hitTime++;
                velocity.y = -velocity.y;
                transform.position = new Vector3(transform.position.x, MaxBorder.y * 2 - transform.position.y, transform.position.z);
            }
            else if (transform.position.y < MinBorder.y)
            {
                hitTime++;
                velocity.y = -velocity.y;
                transform.position = new Vector3(transform.position.x, MinBorder.y * 2 - transform.position.y, transform.position.z);
            }
            cellPosition = ChessBoard.GetCell(transform.position);
            hitChess = ChessBoard.GetChess(cellPosition);
            if (hitChess != null && hitChess.camp != camp && hitChess != lastHitChess)
            {
                hitChess.TakeDamage(1, this, new Vector2Int((int)(velocity.normalized.x * 10), (int)(velocity.normalized.y * 10)));
                //if (hitChess != shooter || lastHitChess != shooter && hitChess == shooter)
                //{
                //    //hitChess.AddForce(velocity);
                //    //hitChess.isActing = true;
                //    //hitChess.Freeze(2);
                //    //ChessManager.instance.PushActingChess(hitChess);
                //    //hitChess.ForcedMove();
                //    //Debug.Log("hit chess is " + hitChess+", last hit chess is" + lastHitChess);
                //}
            }
            lastHitChess = hitChess;
            velocity *= 1.001f;
            if (hitTime >= bounceTime)
                break;
            sprite.Rotate(new(0, 0, s * 360 * Time.deltaTime));
            yield return null;
        }
        isActing = false;
        this.gameObject.SetActive(false);
    }
}
