using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Pawn : Chess
{
    private void Start()
    {
        camp = 1;
        chessTypeName = "Missile";
    }
    public override void Act()
    {
        if (frozenTurns > 0)
        {
            frozenTurns--;
            isActing = false;
            return;
        }
        //Debug.Log("Pawn Move");
        if (ChessBoard.instance != null)
        {
            if (this.y > (ChessBoard.instance.rowNum - 1) / 2)
            {
                //Debug.Log("chessboard[0,0]=" + ChessBoard.instance[0, 0]);
                if (CanMoveTo(x,y-1))
                {
                    StartCoroutine(Move(0, -1));
                }
                else
                {
                    isActing = false;
                }
            }
            else
            {
                int dx = 0;
                if (x > ChessManager.instance.basePosition.x)
                {
                    dx = -1;
                }
                else if (x < ChessManager.instance.basePosition.x)
                {
                    dx = 1;
                }
                if (CanMoveTo(x + dx, y - 1))
                {
                    StartCoroutine(Move(dx, -1));
                }
                else if (CanMoveTo(x, y - 1))
                {
                    StartCoroutine(Move(0, -1));
                }
                else if (CanMoveTo(x + dx, y))
                {
                    StartCoroutine(Move(dx, 0));
                }
                else
                {
                    isActing = false;
                }
            }
        }
    }

    //bool CanMoveTo(int targetX, int targetY)
    //{
    //    if (ChessBoard.IsOnBoard(targetX, targetY) && (ChessBoard.instance[targetY, targetX] == null
    //        || ChessBoard.instance[targetY, targetX].camp != camp))
    //    {
    //        return true;
    //    }
    //    return false;
    //}

    //IEnumerator Move(int dx, int dy)
    //{
    //    //Debug.Log("Pawn Move");
    //    Vector3 startPosition = transform.position;
    //    Vector3 endPosition = transform.position + new Vector3(dx, dy);

    //    for(float t = 0; t < 1f; t += (Time.deltaTime / moveDuration))
    //    {
    //        transform.position = Vector3.Lerp(startPosition, endPosition, t);
    //        yield return null;
    //    }
    //    transform.position = endPosition;
    //    Chess moveTarget = ChessBoard.instance[this.y + dy, this.x + dx];
    //    if(ChessBoard.instance[this.y, this.x] == this)
    //        ChessBoard.instance[this.y, this.x] = null;
    //    x += dx;
    //    y += dy;
    //    ChessBoard.instance[this.y, this.x] = this;
    //    if(moveTarget != null)
    //    {
    //        moveTarget.TakeDamage(1, this, new Vector2Int(dx, dy));
    //        isActing = false;
    //        yield break;
    //    }
    //    isActing = false;
    //}
    public override List<Vector2Int> GetMoveRange()
    {
        List<Vector2Int> rangeList = new List<Vector2Int>();
        if (y > 0)
            rangeList.Add(new Vector2Int(x, y - 1));
        if (x > 0)
            rangeList.Add(new Vector2Int(x - 1, y));
        if (x < ChessBoard.instance.colNum - 1)
            rangeList.Add(new Vector2Int(x + 1, y));
        if (y < ChessBoard.instance.rowNum - 1)
            rangeList.Add(new Vector2Int(x, y + 1));
        return rangeList;
    }

    //public void TakeDamage(int damage, Chess attacker = null, Vector2Int attackDirection = new Vector2Int())
    //{
    //    if (ChessBoard.instance[this.y, this.x] == this)
    //        ChessBoard.instance[this.y, this.x] = null;
    //    Debug.Log("Damaged! attacker = " + attacker);
    //    this.gameObject.SetActive(false);
    //}
}
