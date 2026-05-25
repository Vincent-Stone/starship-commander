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
        chessName = "G.D.";
        chessInfo = "Grand Destroyer，BF团的指挥基地，无法对战斗员进行反击。摧毁它便可赢得胜利。";
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
        if (ChessBoard.instance[this.y, this.x] == this)
            ChessBoard.instance[this.y, this.x] = null;
        isActing = false;
        StartCoroutine(DieCoroutine());
    }
    IEnumerator DieCoroutine()
    {
        while (ChessManager.instance.haveActingChess() || isActing)
            yield return null;
        StageManager.instance.YouWin();
        gameObject.SetActive(false);
    }
}
