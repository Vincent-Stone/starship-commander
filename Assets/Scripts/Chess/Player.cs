using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static ChessManager;
using static UnityEngine.GraphicsBuffer;

public class Player : Chess
{
    public enum ActionType
    {
        Move,
        Ride,
        Punch,
        //HeavyPunch,
        Shoot,
        //LongShot
    }
    public Transform shootArrowTransform;
    [SerializeField] Bullet bullet;
    [SerializeField] UI_DataPanel dataPanel;
   // [SerializeField] TextMeshProUGUI rideOnName;
    [SerializeField] TextMeshProUGUI actionTypeName;
    public Vector2Int actionTarget = Vector2Int.zero;
    int actionIndex = 0;
    public ActionType actionType = ActionType.Move;
    Chess rideOn = null;
    public static Player instance;
    public Base baseChess;
    public Weapon weapon;
    public bool isDead = true;
    bool isInPlayerTurn = false;
    List<ActionType> actionTypeList = new List<ActionType>() { ActionType.Move, ActionType.Ride, ActionType.Punch, ActionType.Shoot };
    [Header("数值")]
    [SerializeField] int maxActionPoints = 2;
    [SerializeField] int actionPoints = 0;
    [Header("范围")]
    [SerializeField] List<Vector2Int> moveRangeList;
    [SerializeField] List<Vector2Int> attackRangeList;
    [SerializeField] Color moveRangeColor;
    [SerializeField] Color attackRangeColor;
    [SerializeField] Color rideRangeColor;
    [Header("输入")]
    [SerializeField] Camera sceneCamera;
    public Vector3 mouseWorldPosition;
    public Vector2Int mouseCellPosition;
    public Chess selectedChess = null;
    public void InitPlayer()
    {
        actionTypeName.text = "Move";
        //hitPoints = maxHitPoints;
        if (dataPanel == null)
        {
            dataPanel = FindFirstObjectByType<UI_DataPanel>();
        }
        if(sceneCamera == null)
        {
            sceneCamera = Camera.main;
        }
        //if (shootArrowTransform == null)
        //    Debug.LogError("Shoot arrow is null!");
        //dataPanel.UpdateSlider(UI_DataPanel.SliderType.Hp, 1);
        camp = 0;
        value = 10;
        bullet.gameObject.SetActive(false);
        //if(baseChess != null)
        //    rideOnName.text = baseChess.chessTypeName;
        weapon = new Weapon(new ActionType[] { ActionType.Punch});
        SetSelectedChess(null);
    }

    public static void CreateInstance()
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<Player>();
            //instance.InitPlayer();
            if (instance == null)
            {
                Debug.LogError("Player instance not found!");
            }
        }
    }

    public void ChangeActionType()
    {
        actionIndex = (actionIndex + 1) % (2 + weapon.actionMode.Count);
        if(actionIndex < 2)
        {
            actionType = (ActionType)actionIndex;
        }
        else
            actionType = weapon.actionMode[actionIndex - 2];
        actionTypeName.text = actionType.ToString();
        ShowRange();
    }
    public void SetActionType( int type)
    {
        actionType = (ActionType)type;
    }

    public void SetRideOn( Chess newRideOn )
    {
        newRideOn.Freeze(100);
        this.rideOn = newRideOn;
        newRideOn.transform.parent = this.transform;
        newRideOn.rider = this;
        //rideOnName.text = rideOn.chessTypeName;
    }

    public void DropRideOn()
    {
        rideOn.Freeze(100);
        rideOn.rider = null;
        rideOn.transform.parent = ChessManager.instance.transform;
        rideOn = null;

        //rideOnName.text = "Space";
    }

    public override void Act()
    {
        isActing = true;
        //Debug.Log("Player Act");
        switch(actionType)
        {

        }
    }

    void ConsumeActionPoints()
    {
        actionPoints--;
        if (actionPoints <= 0)
        {
            isInPlayerTurn = false;
            SetSelectedChess(null);
        }
    }

    public void PlayerTurnStart()
    {
        if(!isInPlayerTurn)
        {
            isInPlayerTurn = true;
        }
        actionPoints = maxActionPoints;
        if (isDead)
        {
            Reborn();
        }
        StartCoroutine(PlayerTurnUpdate());
    }
    public void PlayTurnEnd()
    {
        if(isInPlayerTurn)
        {
            isInPlayerTurn = false;
        }
        ChessManager.instance.EnemyTurnStart();
    }
    IEnumerator PlayerTurnUpdate()
    {
        while (isInPlayerTurn)
        {
            mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseCellPosition = ChessBoard.GetCell(mouseWorldPosition);
            if (Input.GetMouseButtonDown(0) && !ChessManager.instance.haveActingChess() && !isActing)
            {
                OnClicked();
            }
            yield return null;
        }
    }
    public void Reborn()
    {
        x = baseChess.x;
        y = baseChess.y;
        transform.position = baseChess.transform.position;
        isDead = false;
    }
    void OnUpdateMousePosition(int area, Vector3 mousePosition)
    {/*
        switch (actionType)
        {
            case Player.ActionType.Ride:
                break;
            case Player.ActionType.Shoot:
                //testMassage = "Shoot mode: Click on the board to shoot in that direction.";
                shootDirection = new Vector2Int((int)((mousePosition.x - transform.position.x) * 10), (int)((mousePosition.y - transform.position.y) * 10));
                shootArrowTransform.up = shootDirection.normalized;
                //Debug.DrawRay(transform.position, shootDirection.normalized * 10, Color.red);
                break;
            //case Player.ActionType.Throw:
            //    //testMassage = "Throw mode: Click on a valid tile to throw an object.";
            //    break;
            default:
                testMassage = "Unknown action type.";
                break;
        }
     */
    }

    void OnUpdateMouseCellPosition()
    {/*
        //cursor.MoveTo(GetCellCenterWorld((Vector2Int)cellPosition));
        if (ChessBoard.IsOnBoard(cellPosition.x, cellPosition.y))
        {
            Chess chess = chessBoard[cellPosition.y, cellPosition.x];
            //adjustPanels.UpdateInfoPanel(chess);
            select = chess;
        }
    */
    }

    void OnClicked()
    {
        if(selectedChess == null)
        {
            if (ChessBoard.IsOnBoard(mouseCellPosition.x, mouseCellPosition.y))
            {
                Chess chessClickedOn = ChessBoard.instance[mouseCellPosition.y, mouseCellPosition.x];
                if (chessClickedOn != null)
                {
                    SetSelectedChess(chessClickedOn);
                }
            }
            return;
        }
        if (selectedChess.camp == 0)
        {
            if (ChessBoard.IsOnBoard(mouseCellPosition.x, mouseCellPosition.y))
            {
                Chess chessClickedOn = ChessBoard.instance[mouseCellPosition.y, mouseCellPosition.x];
                //Debug.Log("点击");
                if (selectedChess.IsInRange(mouseCellPosition))
                {
                    if(actionType == ActionType.Move && cellPosition != mouseCellPosition)
                    {
                        if(selectedChess == this)
                        {
                            //Debug.Log("移动");
                            //isActing = true;
                            ConsumeActionPoints();
                            ChessManager.instance.PushActingChess(this);
                            StartCoroutine(Move(mouseCellPosition.x - x, mouseCellPosition.y - y));
                        }
                    }
                    else
                    {

                    }
                }
                else if (chessClickedOn != null)
                {
                    SetSelectedChess(chessClickedOn);
                }
            }
        }
    }
    void SetSelectedChess(Chess selected)
    {
        if(selectedChess != null)
            selectedChess.HideRange();
        selectedChess = selected;
        if (selected != null)
            selected.ShowRange();
    }
    Player.ActionType lastActionType = Player.ActionType.Move;
    void OnChangePlayerActionType()
    {
        lastActionType = actionType;
        ShowRange();
        switch (actionType)
        {
            case Player.ActionType.Shoot:
                shootArrowTransform.gameObject.SetActive(true);
                break;
            case Player.ActionType.Ride:
            default:
                shootArrowTransform.gameObject.SetActive(false);
                break;
        }
    }




    public override void ShowRange()
    {
        if(rangeSprites == null)
            InitRangeSprites();
        if (actionType == ActionType.Move)
            ShowRange(GetMoveRange(), moveRangeColor);
        else if (actionType == ActionType.Punch)
            ShowRange(GetAttackRange(), attackRangeColor);
        else if (actionType == ActionType.Ride)
            ShowRange(GetRideRange(), rideRangeColor);
    }

    public override List<Vector2Int> GetMoveRange()
    {
        List<Vector2Int> rangeList = new List<Vector2Int>();
        if(rideOn != null && rideOn.canBeRiden)
        {
            rangeList = rideOn.GetMoveRange();
            if(rangeList.Count > 0)
                return rangeList;
        }
        if (y > 0 && ChessBoard.instance[y - 1, x] == null)
            rangeList.Add(new Vector2Int(x, y - 1));
        if (x > 0 && ChessBoard.instance[y, x - 1] == null)
            rangeList.Add(new Vector2Int(x - 1, y));
        if (x < ChessBoard.instance.colNum - 1 && ChessBoard.instance[y, x + 1] == null)
            rangeList.Add(new Vector2Int(x + 1, y));
        if (y < ChessBoard.instance.rowNum - 1 && ChessBoard.instance[y + 1, x] == null)
            rangeList.Add(new Vector2Int(x, y + 1));
        return rangeList;
    }
    public List<Vector2Int> GetAttackRange()
    {
        return GetMoveRange();
    }
    public List<Vector2Int> GetRideRange()
    {
        return new List<Vector2Int>();
    }

    List<Vector2Int> GetShootRange()
    {
        return new List<Vector2Int>();
    }

    List<Vector2Int> GetThrowRange()
    {
        return GetShootRange();
    }



    public IEnumerator Move(int dx,int dy)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = transform.position + new Vector3(dx, dy, 0);
        Chess target = ChessBoard.instance[y + dy, x + dx];
        if (ChessBoard.instance[y, x] == this)
            ChessBoard.instance[y, x] = null;
        if(rideOn == baseChess)
        {
            DropRideOn();
            ChessBoard.instance[y, x] = baseChess;
        }
        x += dx;
        y += dy;
        if (rideOn != null)
        {
            rideOn.x = x;
            rideOn.y = y;
        }
        transform.position = endPosition;
        if (rideOn == null && ChessBoard.instance[y, x] == baseChess)
        {
            SetRideOn(baseChess);
        }
        ChessBoard.instance[y,x] = this;
        if(target != null)
        {
            //if (actionType == ActionType.Ride && target != null && target.canBeRiden)
            //{
            //    Debug.Log("Ride on " + target.name);
            //    SetRideOn(target);
            //}
            //else if (target != null && target.camp != camp)
            //{
            //    target.TakeDamage(10);
            //}
            //else if (target == baseChess)
            //    SetRideOn(target);
        }
        yield return null;
        ChessManager.instance.PopActingChess(this);
        if (actionPoints > 0)
            ShowRange();
    }

    public IEnumerator Shoot(int targetX, int targetY)
    {
        Debug.Log("Player Shoot");
        bullet.gameObject.SetActive(true);
        bullet.shooter = this;
        bullet.Shoot(new Vector2(targetX, targetY).normalized);
        yield return null;
    }

    public void EndShoot()
    {
        Debug.Log("Player End Shoot");
        isActing = false;
    }

    public override void TakeDamage(int damage, Chess attacker = null, Vector2Int attackDirection = new Vector2Int())
    {
        if(!isDead)
            isDead = true;
        /*if (attacker != null)
        {
            ChessBoard.instance[this.y, this.x] = this;
            attacker.TakeDamage(10, this);
        }*/
        isActing = true;
        ChessManager.instance.PushActingChess(this);
        StartCoroutine(Damaged());
    }

    IEnumerator Damaged()//受击效果
    {
        for (float i = 0; i <= 2 * Mathf.PI; i++)
        {
            transform.Rotate(new Vector3(0, 0, i));
            yield return null;
        }
        transform.rotation = Quaternion.identity;
        isActing = false;
    }
    public override void ForcedMove()
    {
        Vector2Int forcedMoveTarget = GetForcedMoveTarget();
        if (forcedMoveTarget == new Vector2(x, y))
        {
            return;
        }
        if (rideOn)
        {
            ChessBoard.instance[this.y, this.x] = rideOn;
            Debug.Log("Player Stop Ride On " + rideOn+ " ,forcedMoveTarget="+forcedMoveTarget);
            DropRideOn();
        }
        base.ForcedMove();
    }
}
