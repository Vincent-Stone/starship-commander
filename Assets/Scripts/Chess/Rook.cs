using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Rook : Ship
{
    private void Start()
    {
        camp = 1;
        canBeRiden = true;
        moveDuration = 0.1f;
        chessTypeName = "Rook";
    }
    public override List<Vector2Int> GetMoveRange(int targetX,int targetY, out List<Vector2Int> attackPoints, Chess thisChess)
    {
        attackPoints = new List<Vector2Int>();
        List<Vector2Int> rangeList = new List<Vector2Int>();
        int up = 0, down = 0, left = 0, right = 0;
        bool end = false;
        Chess cur = ChessBoard.instance[targetY, targetX], next;

        while (!end)
        {
            end = true;
            if (targetY + up + 1 < ChessBoard.instance.rowNum)
            {
                cur = ChessBoard.instance[targetY + up, targetX];
                if (cur == null || cur == thisChess)
                {
                    next = ChessBoard.instance[targetY + up + 1, targetX];
                    if (next == null || next.camp != thisChess.camp || next.camp != this.camp && next.canBeRiden)
                    {
                        up++;
                        rangeList.Add(new Vector2Int(targetX, targetY + up));
                        if (next != null && !next.canMove)
                            attackPoints.Add(new Vector2Int(targetX, targetY + up));
                        end = false;
                    }
                }
            }
            if (targetY - down - 1 >= 0)
            {
                cur = ChessBoard.instance[targetY - down, targetX];
                if (cur == null || cur == thisChess)
                {
                    next = ChessBoard.instance[targetY - down - 1, targetX];
                    if (next == null || next.camp != thisChess.camp || next.camp != this.camp && next.canBeRiden)
                    {
                        down++;
                        rangeList.Add(new Vector2Int(targetX, targetY - down));
                        if (next != null && !next.canMove)
                            attackPoints.Add(new Vector2Int(targetX, targetY - down));
                        end = false;
                    }
                }
            }
            if (targetX - left - 1 >= 0)
            {
                cur = ChessBoard.instance[targetY, targetX - left];
                if (cur == null || cur == thisChess)
                {
                    next = ChessBoard.instance[targetY, targetX - left - 1];
                    if (next == null || next.camp != thisChess.camp || next.camp != this.camp && next.canBeRiden)
                    {
                        left++;
                        rangeList.Add(new Vector2Int(targetX - left, targetY));
                        if (next != null && !next.canMove)
                            attackPoints.Add(new Vector2Int(targetX - left, targetY));
                        end = false;
                    }
                }
            }
            if (targetX + right + 1 < ChessBoard.instance.colNum)
            {
                cur = ChessBoard.instance[targetY, targetX + right];
                if (cur == null || cur == thisChess)
                {
                    next = ChessBoard.instance[targetY, targetX + right + 1];
                    if (next == null || next.camp != thisChess.camp || next.camp != this.camp && next.canBeRiden)
                    {
                        right++;
                        rangeList.Add(new Vector2Int(targetX + right, targetY));
                        if (next != null && !next.canMove)
                            attackPoints.Add(new Vector2Int(targetX + right, targetY));
                        end = false;
                    }
                }
            }
        }
        Debug.Log("left=" + left + " right=" + right + " up=" + up + " down=" + down);
        return rangeList;
    }
    //public override List<Vector2Int> GetMoveRange()
    //{
    //    int attackPoints;
    //    if (rider != null)
    //        return GetMoveRange(x, y, out attackPoints ,rider);
    //    return GetMoveRange(x, y, out attackPoints, this);
    //}
    //public override void TakeDamage(int damage, Chess attacker = null, Vector2Int attackDirection = new Vector2Int())
    //{
    //    if (ChessBoard.instance[this.targetY, this.targetX] == this)
    //        ChessBoard.instance[this.targetY, this.targetX] = null;
    //    Debug.Log("Damaged! attacker = " + attacker);
    //    this.gameObject.SetActive(false);
    //}
}
