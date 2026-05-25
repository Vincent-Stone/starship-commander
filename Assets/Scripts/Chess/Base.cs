using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using ActionType = Player.ActionType;

public class Base : Chess
{
    [SerializeField] int shieldsNum = 0;
    [SerializeField] int maxShieldsNum = 10;
    [SerializeField] UI_Tokens baseHitPointTokens;
    [SerializeField] int baseValue = 100;
    [SerializeField] SpriteRenderer shieldSpriteRenderer;
    public static Base instance = null;
    public void InitBaseAndPlayer()
    {
        if (instance == null)
            instance = this;
        Debug.Log("Init Base And Player");
        isActing = false;
        camp = 0;
        canMove = false;
        canBeRiden = true;
        chessTypeName = "Base";
        chessName = "基地";
        chessInfo = "全称<b>基里尼亚加近地防卫飞船</b>，简称“<b>基地</b>”。\n<size=20>\n</size>以个性开朗、风趣幽默著称。";
        shieldsNum = maxShieldsNum;
        shieldSpriteRenderer.enabled = true;
        hpUI.UpdateHP(shieldsNum);
        hitPoints = maxHitPoints;
        canBeHitByBullet = false;
        value = baseValue;
        actionTypeList = new List<ActionType>() { ActionType.Move };
        if(baseHitPointTokens == null)
        {
            Debug.LogError("baseHitPointToken is null");
            return;
        }
        baseHitPointTokens.Init(maxHitPoints);
        if (ChessBoard.IsOnBoard(x, y))
        {
            y = 0;
            transform.position = ChessBoard.GetCellCenterWorld(new Vector2Int(x, y));
            ChessManager.instance.UpdateChessList();
            Player.CreateInstance();
            Player player = Player.instance;
            player.x = x;
            player.y = y;
            player.transform.position = transform.position;
            ChessBoard.instance[y, x] = player;
            player.SetRideOn(this);
            player.baseChess = this;
            player.isBossStart = y + 10 >= ChessBoard.instance.rowNum;
            player.InitPlayer();
        }
    }
    public override void Act()
    {
    }
    public void A_Move(int dx,int dy,int actionPoints)
    {
        bool intoBossArea = dy + y + 10 >= ChessBoard.instance.rowNum;
        if (intoBossArea)//准备进入BOSS战
        {
            dy = ChessBoard.instance.rowNum - 10 - y;
        }
        Vector3 startPosition = transform.position;
        Vector3 endPosition = transform.position + new Vector3(dx, dy, 0);
        Chess target = ChessBoard.instance[y + dy, x + dx];
        if (ChessBoard.instance[y, x] == this)
            ChessBoard.instance[y, x] = null;
        x += dx;
        y += dy;
        ChessBoard.instance[y, x] = this;
        ChessManager.instance.UpdateChessList();
        StartCoroutine(MoveCoroutine(startPosition, endPosition, actionPoints, intoBossArea));
        if (actionPoints > 0)
            ShowRange();
    }

    public IEnumerator MoveCoroutine(Vector3 startPosition, Vector3 endPosition, int actionPoints, bool intoBossArea = false)
    {
        for (float t = 0; t < 1f; t += (Time.deltaTime / moveDuration))
        {
            while (StageManager.isPaused) // 可暂停
                yield return null;
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }
        transform.position = endPosition;
        if (actionPoints > 0)
            ShowRange();
        if (intoBossArea)
        {
            StageManager.instance.BossStart();
        }
        isActing = false;
    }

    public override void TakeDamage(int damage, Chess attacker = null, Vector2Int attackDirection = new Vector2Int())
    {
        if(shieldsNum > 0)
        {
            shieldsNum--;
            hpUI.UpdateHP(shieldsNum);
            if (shieldsNum == 0)
            {
                ChessManager.instance.PushActingChess(this);
                StartCoroutine(ShieldClose());
            }
            isActing = false;
            return;
        }
        hitPoints -= damage;
        baseHitPointTokens.UpdateTokens(hitPoints);
        ChessManager.instance.PushActingChess(this);
        StartCoroutine(Damaged());
        if (hitPoints <= 0)
        {
            Die();
        }
    }

    public override void Die()
    {
        Debug.Log("- Enemy Base has been destroyed -");
        if (ChessBoard.instance[this.y, this.x] == this)
            ChessBoard.instance[this.y, this.x] = null;
        StartCoroutine(DieCoroutine());
    }
    IEnumerator DieCoroutine()
    {
        StageManager.instance.YouLose();
        isActing = false;
        yield break;
    }

    IEnumerator ShieldClose()
    {
        Color originalColor = shieldSpriteRenderer.color;
        Color color = originalColor;
        for (float i = 1; i > 0; i -= Time.deltaTime * 10)
        {
            color = originalColor;
            color.a *= i;
            shieldSpriteRenderer.color = color;
            yield return null;
        }
        shieldSpriteRenderer.enabled = false;
        shieldSpriteRenderer.color = originalColor;
        isActing = false;
    }

    IEnumerator Damaged()
    {
        Vector3 startPosition = transform.position;
        for (float i = 0; i <= 20; i++)
        {
            transform.position = startPosition + Vector3.up * (i % 2) * 0.1f;
            yield return null;
        }
        transform.position = startPosition;
        isActing = false;
    }

    public override List<Vector2Int> GetMoveRange()
    {
        List<Vector2Int> rangeList = new List<Vector2Int>();
        bool canMoveUp = true;
        for (int i = y; i < ChessBoard.instance.rowNum; i++)
        {
            for(int j = 0; j < ChessBoard.instance.colNum; j++)
            {
                if(ChessBoard.instance[i, j] != null && ChessBoard.instance[i, j] != this)   
                {
                    canMoveUp = false; 
                    break;
                }
            }
            if (canMoveUp)
            {
                if (i != y)
                    rangeList.Add(new Vector2Int(x, i));
            }
            else
                break;
        }
        return rangeList;
    }
    public override List<Vector2Int> GetAttackRange()
    {
        return new List<Vector2Int>();
    }
}
