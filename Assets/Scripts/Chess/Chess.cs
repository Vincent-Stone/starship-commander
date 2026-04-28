using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
    internal SpriteRenderer[,] rangeSprites = null;
    public Vector2Int cellPosition { get { return new Vector2Int(x, y); } }
    public float moveDuration = 0.001f;
    public bool isActing = false;
    public bool canBeForcedMoved = true;
    public int value = 0;
    [SerializeField] internal Vector2Int axisForce;
    internal int frozenTurns = 0;
    public Chess rider = null;
    public abstract void Act();
    public void InitRangeSprites()
    {
        if (rangeSprites == null)
        {
            rangeSprites = new SpriteRenderer[ChessBoard.instance.rowNum, ChessBoard.instance.colNum];
            for (int i = 0; i < ChessBoard.instance.rowNum; i++)
            {
                for (int j = 0; j < ChessBoard.instance.colNum; j++)
                {
                    rangeSprites[i, j] = GameObject.Instantiate(ChessBoard.instance.rangePrefab, ChessBoard.GetCellCenterWorld(i, j), Quaternion.identity).GetComponent<SpriteRenderer>();
                    rangeSprites[i, j].transform.parent = ChessManager.instance.transform;
                    rangeSprites[i, j].color = new Color(1, 1, 1, 0.1f); // Set initial transparency to 0
                }
            }
        }
    }

    public void ShowRange(List<Vector2Int> rangeList, Color highlightColor)
    {
        HideRange();
        foreach (Vector2Int pos in rangeList)
        {
            if (pos.x >= 0 && pos.x < ChessBoard.instance.colNum && pos.y >= 0 && pos.y < ChessBoard.instance.rowNum)
            {
                rangeSprites[pos.y, pos.x].color = highlightColor;
                rangeSprites[pos.y, pos.x].sortingOrder = 10;
            }
        }
    }
    public virtual void HideRange()
    {
        if (rangeSprites == null)
            InitRangeSprites();
        if (rangeSprites != null)
        {
            foreach (SpriteRenderer range in rangeSprites)
            {
                range.color = Color.clear;
            }
        }
    }
    public virtual void ShowRange()
    {
        if (rangeSprites == null)
            InitRangeSprites();
        ShowRange(GetMoveRange(), new Color(0, 1, 0, 0.2f));
    }

    public bool IsInRange(Vector2Int pos)
    {
        if (ChessBoard.IsOnBoard(pos.x, pos.y))
        {
            if(rangeSprites == null)
            {
                Debug.LogError("rangeSprites of " + name + " is null.");
                return false;
            }
            return rangeSprites[pos.y, pos.x].color != Color.clear;
        }
        else
            return false;
    }

    public abstract List<Vector2Int> GetMoveRange();
    public virtual void TakeDamage(int damage, Chess attacker = null, Vector2Int attackDirection = new Vector2Int())
    {
        if (ChessBoard.instance[this.y, this.x] == this)
            ChessBoard.instance[this.y, this.x] = null;
        Debug.Log("Damaged! attacker = " + attacker);
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
        for(float timer = 0; timer < 1; timer += Time.deltaTime / 0.1f)
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
