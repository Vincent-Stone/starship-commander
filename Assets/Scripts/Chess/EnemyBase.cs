using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : Chess
{
    [SerializeField] int maxHp = 10;
    int hp = 0;
    private void Start()
    {
        hp = maxHp;
        chessTypeName = "EnemyBase";
        camp = 1;
    }
    public override void Act()
    {
        isActing = false;
    }

    public override List<Vector2Int> GetAttackRange()
    {
        return new List<Vector2Int>();
    }

    public override List<Vector2Int> GetMoveRange()
    {
        return new List<Vector2Int>();
    }

    public override void TakeDamage(int damage, Chess attacker = null, Vector2Int attackDirection = default)
    {
        ChessManager.instance.PushActingChess(this);
        hp -= damage;
        if (hp == 0)
        {
            Debug.Log("Enemy Base Destroyed!");
        }
        isActing = false;
    }

}
