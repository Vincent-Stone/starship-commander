using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : Chess
{
    //[SerializeField] int maxHp = 10;
    //int hp = 0;
    private void Start()
    {
        hitPoints = maxHitPoints;
        chessTypeName = "EnemyBase";
        camp = 1;
        canBeHitByBullet = false;
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

    public override void Die()
    {
        Debug.Log("- Enemy Base has been destroyed -");
        base.Die();
    }
}
