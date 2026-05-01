using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using ActionType = Player.ActionType;

public abstract class Chess : MonoBehaviour , IDamageable
{
    public enum ChessType
    {
        Player,
        Pawn,
        Knight,
        Rook,
        Base
    }
    public string chessTypeName = "Chess";
    public int camp = -1; //ÕóÓª
    public bool canBeRiden = false;
    public bool canMove = true;
    public int x;
    public int y;
    public int maxHitPoints = 1;
    public int hitPoints = 1;
    public Vector2Int cellPosition { get { return new Vector2Int(x, y); } }
    public float moveDuration = 0.001f;
    public int speed;
    public bool isActing = false;
    public bool canBeForcedMoved = true;
    public int value = 0;
    [SerializeField] internal Vector2Int axisForce;
    internal int frozenTurns = 0;
    public Chess rider = null;
    internal List<ActionType> actionTypeList = new List<ActionType>() { ActionType.Enemy };
    internal int actionTypeIndex = 0;
    public abstract void Act();
    
    public virtual void ShowRange()
    {
        ChessBoard.instance.HideRange();
        ChessBoard.instance.ShowRange(GetMoveRange(), new Color(0, 1, 0, 0.8f), true);
        ChessBoard.instance.ShowRange(GetAttackRange(), new Color(1, 0, 0, 0.8f), false);
    }
    
    public bool IsInRange(Vector2Int pos)
    {
        if (ChessBoard.IsOnBoard(pos.x, pos.y))
        {
            if(ChessManager.instance.moveRange.activeSelf == true)
            {
                return ChessBoard.IsInMoveRange(pos);
            }else if(ChessManager.instance.attackRange.activeSelf == true)
                return ChessBoard.IsInAttackRange(pos);
            else
                return false;
        }
        else
            return false;
    }

    public abstract List<Vector2Int> GetMoveRange();
    public abstract List<Vector2Int> GetAttackRange();
    public virtual void TakeDamage(int damage, Chess attacker = null, Vector2Int attackDirection = new Vector2Int())
    {
        if(hitPoints <= 0)
        {
            isActing = false;
            return;
        }
        ChessManager.instance.PushActingChess(this);
        Debug.Log("Damaged! attacker = " + attacker);
        hitPoints -= damage;
        if(hitPoints <= 0)
        {
            Die();
        }
        isActing = false;
    }
    public virtual void Die()
    {
        if (ChessBoard.instance[this.y, this.x] == this)
            ChessBoard.instance[this.y, this.x] = null;
        this.gameObject.SetActive(false);
    }
    public void Freeze(int duration)
    {
        if (frozenTurns < duration)
            frozenTurns = duration;
    }
    public void AddForce(Vector2 force)
    {
        force.Normalize();
        axisForce = Vector2Int.zero;
        if (force.x < -Mathf.Sin(Mathf.PI / 6))
        {
            axisForce.x = -1;
        }
        else if (force.x > Mathf.Sin(Mathf.PI / 6))
        {
            axisForce.x = 1;
        }
        if (force.y > Mathf.Sin(Mathf.PI / 6))
        {
            axisForce.y = 1;
        }
        else if (force.y < -Mathf.Sin(Mathf.PI / 6))
        {
            axisForce.y = -1;
        }

    }
    public void ActEnd()
    {
        isActing = false;
    }

    internal Vector2Int GetForcedMoveTarget()
    {
        Vector2Int forcedMoveTarget = new Vector2Int(x, y);
        if (axisForce.x == 0)
        {
            if (CanForcedMoveTo(x, y + axisForce.y))
            {
                forcedMoveTarget = new Vector2Int(x, y + axisForce.y);
            }
        }
        else
        {
            if (axisForce.y == 0)
            {
                if (CanForcedMoveTo(x + axisForce.x, y))
                {
                    forcedMoveTarget = new Vector2Int(x + axisForce.x, y);
                }
            }
            else
            {
                if (CanForcedMoveTo(x + axisForce.x, y + axisForce.y))
                {
                    forcedMoveTarget = new Vector2Int(x + axisForce.x, y + axisForce.y);
                }
                else if (CanForcedMoveTo(x + axisForce.x, y))
                {
                    forcedMoveTarget = new Vector2Int(x + axisForce.x, y);
                }
                else if (CanForcedMoveTo(x, y + axisForce.y))
                {
                    forcedMoveTarget = new Vector2Int(x, y + axisForce.y);
                }
            }
        }
        return forcedMoveTarget;
    }
    public virtual void ForcedMove()
    {
        isActing = true;
        Vector2Int forcedMoveTarget = GetForcedMoveTarget();
        if (forcedMoveTarget.x != x || forcedMoveTarget.y != y)
        {
            if (ChessBoard.instance[y, x] == this)
                ChessBoard.instance[y, x] = null;
            x = forcedMoveTarget.x;
            y = forcedMoveTarget.y;
            ChessBoard.instance[y, x] = this;
            StartCoroutine(ForcedMovingCoroutine(forcedMoveTarget));
        }
        else
        {
            isActing = false;
        }
    }

    internal virtual bool CanMoveTo(int targetX, int targetY)
    {
        if (ChessBoard.IsOnBoard(targetX, targetY) && (ChessBoard.instance[targetY, targetX] == null 
            || ChessBoard.instance[targetY, targetX].camp != this.camp))
        {
            return true;
        }
        return false;
    }

    internal virtual IEnumerator Move(int dx, int dy)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = transform.position + new Vector3(dx, dy);

        for (float t = 0; t < 1f; t += (Time.deltaTime / moveDuration))
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }
        transform.position = endPosition;
        Chess moveTarget = ChessBoard.instance[this.y + dy, this.x + dx];
        if (ChessBoard.instance[this.y, this.x] == this)
            ChessBoard.instance[this.y, this.x] = null;
        x += dx;
        y += dy;
        ChessBoard.instance[this.y, this.x] = this;
        if (moveTarget != null)
        {
            moveTarget.TakeDamage(1, this, new Vector2Int(dx, dy));
            isActing = false;
            yield break;
        }
        isActing = false;
    }

    public void ForcedMove(Vector2Int axisForce)
    {
        this.axisForce = axisForce;
        ForcedMove();
    }
    IEnumerator ForcedMovingCoroutine (Vector2Int forcedMoveTarget)
    {
        Vector3 startPosition = transform.position, endPosition = ChessBoard.GetCellCenterWorld(forcedMoveTarget);
        for(float timer = 0; timer < 1; timer += Time.deltaTime / 0.2f)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, -Mathf.Pow((timer - 1), 6) + 1);
            yield return null;
        }
        transform.position = endPosition;
        isActing = false;
    }
    bool CanForcedMoveTo(int targetX, int targetY)
    {
        if(ChessBoard.IsOnBoard(targetX, targetY) && ChessBoard.instance[targetY, targetX] == null)
        {
            return true;
        }
        return false;
    }
}
