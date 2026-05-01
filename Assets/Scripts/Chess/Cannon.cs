using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : Chess
{
    [SerializeField] int fullEnergy = 2;
    int energy = 0;
    bool isAttackPhase = true; // 用于区分攻击阶段和移动阶段
    Chess attackTarget; // 记录当前攻击目标
    void Start()
    {
        camp = 1; // 设置为敌人阵营
        canBeRiden = false;
        canMove = false; // Cannon不能移动
        value = 3; // 设置一个合适的value值
        hitPoints = maxHitPoints; // Cannon只有一点血量
    }

    public override void Act()
    {
        if (frozenTurns > 0)
        {
            frozenTurns--;
            isActing = false;
            return;
        }

        if (isAttackPhase)
        {
            // 攻击阶段
            isAttackPhase = false;

            if (energy < fullEnergy)
            {
                // 蓄力阶段
                energy++;
                Debug.Log($"Cannon at ({x}, {y}) is charging: {energy}/{fullEnergy}");
                isActing = false;
            }
            else
            {
                // 蓄力完成，进行攻击
                energy = 0; // 重置蓄力
                AttackForward();
            }
        }
        else
        {
            // 移动阶段 - Cannon不会移动，直接结束行动
            isAttackPhase = true;
            isActing = false;
        }
    }

    void AttackForward()
    {
        GetAttackRange(); // 获取攻击范围并确定攻击目标
        StartCoroutine(AttackCoroutine());
    }

    IEnumerator AttackCoroutine()
    {
        isActing = true;
        // 这里可以添加攻击动画效果
        yield return new WaitForSeconds(0.2f);
        if(attackTarget != null)
            attackTarget.TakeDamage(1, this, new Vector2Int(0, -1));
        isActing = false;
    }

    void ShowAttackEffect(List<Vector2Int> path)
    {
        // 这里可以添加视觉效果，比如显示攻击路径
        // 实际项目中可以在这里实例化特效
        foreach (var pos in path)
        {
            Debug.DrawLine(ChessBoard.GetCellCenterWorld(y, x),
                          ChessBoard.GetCellCenterWorld(pos.y, pos.x),
                          Color.red, 1f);
        }
    }

    public override List<Vector2Int> GetAttackRange()
    {
        List<Vector2Int> range = new List<Vector2Int>();
        attackTarget = null; // 重置攻击目标
        int targetY = y - 1;
        while (ChessBoard.IsOnBoard(x, targetY))
        {
            range.Add(new Vector2Int(x, targetY));

            Chess target = ChessBoard.GetChess(new Vector2Int(x, targetY));
            if (target != null)
            {
                attackTarget = target; // 记录攻击目标
                break; // 碰到第一个棋子就停止
            }
                targetY--;
        }
        return range;
    }

    public override List<Vector2Int> GetMoveRange()
    {
        // Cannon不会移动，返回空列表
        return new List<Vector2Int>();
    }

    public override void Die()
    {
        if (ChessBoard.instance[this.y, this.x] == this)
            ChessBoard.instance[this.y, this.x] = null;
        StartCoroutine(DeathExplosion());
    }

    IEnumerator DeathExplosion()
    {
        yield return new WaitForSeconds(0.1f);
        // 死亡时对上下左右四个方向造成1点伤害
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, -1),  // 下
            new Vector2Int(0, 1),   // 上
            new Vector2Int(1, 0),   // 右
            new Vector2Int(-1, 0)   // 左
        };

        foreach (var dir in directions)
        {
            int targetX = x + dir.x;
            int targetY = y + dir.y;

            if (ChessBoard.IsOnBoard(targetX, targetY))
            {
                Chess target = ChessBoard.GetChess(new Vector2Int(targetX, targetY));
                if (target != null)
                {
                    Debug.Log($"Cannon explosion damages {target.chessTypeName} at ({targetX}, {targetY})");
                    target.TakeDamage(1, this, dir);
                }
            }
        }
        yield return new WaitForSeconds(0.2f); // 等待爆炸效果结束
        // 最后移除Cannon
        gameObject.SetActive(false);
        yield return null;
    }
}