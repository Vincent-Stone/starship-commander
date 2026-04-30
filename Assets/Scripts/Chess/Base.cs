using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using ActionType = Player.ActionType;

public class Base : Chess
{
    [SerializeField] int shieldsNum = 0;
    int hitPoints = 0;
    [SerializeField] int maxShieldsNum = 10;
    [SerializeField] int maxHitPoints = 10;
    [SerializeField] UI_DataPanel dataPanel;
    [SerializeField] int baseValue = 100;
    public void InitBaseAndPlayer()
    {
        camp = 0;
        canMove = false;
        canBeRiden = true;
        chessTypeName = "Base";
        shieldsNum = maxShieldsNum;
        hitPoints = maxHitPoints;
        canBeForcedMoved = false;
        value = baseValue;
        actionTypeList = new List<ActionType>() { ActionType.Move };
        if (dataPanel == null)
        {
            dataPanel = FindFirstObjectByType<UI_DataPanel>();
        }
        if (dataPanel != null)
            dataPanel.UpdateSlider(UI_DataPanel.SliderType.BaseHp, 1);

        if (ChessBoard.IsOnBoard(x, y))
        {
            transform.position = ChessBoard.GetCellCenterWorld(new Vector2Int(x, y));
            Player.CreateInstance();
            Player player = Player.instance;
            player.x = x;
            player.y = y;
            player.transform.position = transform.position;
            ChessBoard.instance[y, x] = player;
            player.SetRideOn(this);
            player.baseChess = this;
            player.InitPlayer();
        }
    }
    public override void Act()
    {
    }
    public void A_Move(int dx,int dy,int actionPoints)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = transform.position + new Vector3(dx, dy, 0);
        Chess target = ChessBoard.instance[y + dy, x + dx];
        if (ChessBoard.instance[y, x] == this)
            ChessBoard.instance[y, x] = null;
        x += dx;
        y += dy;
        transform.position = endPosition;
        ChessBoard.instance[y, x] = this;
        if (actionPoints > 0)
            ShowRange();
        isActing = false;
        ChessManager.instance.UpdateChessList();
    }
    public override void TakeDamage(int damage, Chess attacker = null, Vector2Int attackDirection = new Vector2Int())
    {
        if(shieldsNum > 0)
        {
            shieldsNum--;
            //dataPanel.UpdateSlider(UI_DataPanel.SliderType.BaseShield, shieldsNum / (float)maxShieldsNum);
            //if (attacker)
            //{
            //    ChessBoard.instance[y, x] = this;
            //    attacker.AddForce(-attackDirection);
            //    attacker.isActing = true;
            //    ChessManager.instance.PushActingChess(attacker);
            //    attacker.ForcedMove();
            //}
            return;
        }
        hitPoints -= damage;
        dataPanel.UpdateSlider(UI_DataPanel.SliderType.BaseHp, hitPoints / (float)maxHitPoints);
        //if (attacker != null)
        //{
        //    ChessBoard.instance[this.y, this.x] = this;
        //    attacker.TakeDamage(10, this);
        //}
        ChessManager.instance.PushActingChess(this);
        StartCoroutine(Damaged());
        if (hitPoints <= 0)
        {
            Debug.Log("Player is dead");
        }
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
