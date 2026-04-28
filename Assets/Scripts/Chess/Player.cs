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
    [Header("行动类型")]
    [SerializeField] TextMeshProUGUI actionTypeName;
    public Vector2Int actionTarget = Vector2Int.zero;
    public ActionType actionType = ActionType.Move;
    Chess rideOn = null;
    public static Player instance;
    public Base baseChess;
    //public Weapon weapon;
    public bool isDead = true;
    bool isInPlayerTurn = false;
    string defaultActionTypeName { get {
            if (actionPoints > 0)
                return "Select A Chess";
            else
                return "No Energy";
        }
    }
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
        actionTypeName.text = defaultActionTypeName;
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
        //weapon = new Weapon(new ActionType[] { ActionType.Punch});
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
        if(selectedChess != null)
        {
            selectedChess.actionTypeIndex = (selectedChess.actionTypeIndex + 1) % selectedChess.actionTypeList.Count;
            if (selectedChess.actionTypeIndex < selectedChess.actionTypeList.Count)
            {
                actionType = (ActionType)selectedChess.actionTypeList[selectedChess.actionTypeIndex];
            }
            actionTypeName.text = actionType.ToString();
            selectedChess.ShowRange();
            return;
        }
        actionTypeName.text = defaultActionTypeName;
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
        SetSelectedChess(null);
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
        if(selectedChess != null)
        {
            selectedChess.HideRange();
            SetSelectedChess(null);
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
            while (ChessManager.instance.haveActingChess())
            {
                while (ChessManager.instance.haveActingChess() && !ChessManager.instance.PeekActingChess().isActing)
                    ChessManager.instance.PopActingChess();
                yield return null;
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
        if (selectedChess == null)
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
        if (ChessBoard.IsOnBoard(mouseCellPosition.x, mouseCellPosition.y))
        {
            Chess chessClickedOn = ChessBoard.instance[mouseCellPosition.y, mouseCellPosition.x];
            Debug.Log("点击");
            if (selectedChess.IsInRange(mouseCellPosition) && selectedChess.camp == this.camp)
            {
                if (actionType == ActionType.Move && cellPosition != mouseCellPosition)
                {
                    if (selectedChess == this)
                    {
                        //Debug.Log("移动");
                        ConsumeActionPoints();
                        ChessManager.instance.PushActingChess(this);
                        A_Move(mouseCellPosition.x - x, mouseCellPosition.y - y);
                    }else if(selectedChess == baseChess)
                    {
                        //Debug.Log("移动");
                        ConsumeActionPoints();
                        ChessManager.instance.PushActingChess(baseChess);
                        baseChess.A_Move(mouseCellPosition.x - baseChess.x, mouseCellPosition.y - baseChess.y, actionPoints);
                        //A_Move(mouseCellPosition.x - x, mouseCellPosition.y - y);
                    }
                }
                else if (actionType == ActionType.Punch)
                {
                    if (chessClickedOn != null && chessClickedOn.camp != camp)
                    {
                        //Debug.Log("攻击");
                        ConsumeActionPoints();
                        ChessManager.instance.PushActingChess(this);
                        A_Punch(mouseCellPosition.x - x, mouseCellPosition.y - y);
                    }
                }
                else if (actionType == ActionType.Ride)
                {
                    if (chessClickedOn != null && chessClickedOn.canBeRiden)
                    {
                        //Debug.Log("骑乘");
                        ConsumeActionPoints();
                        ChessManager.instance.PushActingChess(this);
                        A_Ride(mouseCellPosition.x - x, mouseCellPosition.y - y);
                    }
                }
            }
            else
            {
                SetSelectedChess(chessClickedOn);
            }
        }
    }
    void SetSelectedChess(Chess selected)
    {
        if(selectedChess != null)
            selectedChess.HideRange();
        selectedChess = selected;
        if (selected != null)
        {
            selected.ShowRange();
            selected.actionTypeIndex += selectedChess.actionTypeList.Count - 1;
            ChangeActionType();
        }
        else
        {
            actionTypeName.text = defaultActionTypeName;
        }
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
        else
            HideRange();
    }

    public override List<Vector2Int> GetMoveRange()
    {
        List<Vector2Int> rangeList = new List<Vector2Int>();
        if(rideOn != null && rideOn.canBeRiden && rideOn != baseChess)
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
    public override List<Vector2Int> GetAttackRange()
    {
        List<Vector2Int> rangeList = new List<Vector2Int>();
        if (rideOn != null && rideOn.canBeRiden)
        {
            rangeList = rideOn.GetAttackRange();
            if (rangeList.Count > 0)
                return rangeList;
        }
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
    public List<Vector2Int> GetRideRange()
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

    List<Vector2Int> GetShootRange()
    {
        return new List<Vector2Int>();
    }

    List<Vector2Int> GetThrowRange()
    {
        return GetShootRange();
    }

    public void A_Move(int dx,int dy)
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
        if (actionPoints > 0)
            ShowRange();
        isActing = false;
    }
    public void A_Punch(int dx, int dy)
    {
        Chess target = ChessBoard.instance[y + dy, x + dx];
        if (target != null && target.camp != camp)
        {
            target.TakeDamage(10, this, new Vector2Int(dx, dy));
        }
        if (actionPoints > 0)
            ShowRange();
        isActing = false;
    }
    public void A_Ride(int dx, int dy)
    {
        Chess target = ChessBoard.instance[y + dy, x + dx];
        Vector3 startPosition = transform.position;
        Vector3 endPosition = transform.position + new Vector3(dx, dy, 0);
        if (rideOn != null)
        {
            rideOn.x = x;
            rideOn.y = y;
            if (ChessBoard.instance[y, x] == this)
                ChessBoard.instance[y, x] = rideOn;
            DropRideOn();
        }
        else
        {
            if (ChessBoard.instance[y, x] == this)
                ChessBoard.instance[y, x] = null;
        }
            x += dx;
        y += dy;
        ChessBoard.instance[y, x] = this;
        transform.position = endPosition;
        if (target != null && target.canBeRiden)
        {
            SetRideOn(target);
        }
        if (actionPoints > 0)
            ShowRange();
        isActing = false;
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
        //if(!isDead)
        //    isDead = true;
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
