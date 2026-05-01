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
        Enemy
    }
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
    [Header("UI标志")]
    public Transform shootArrowTransform;
    [SerializeField] SpriteRenderer blockSign;
    [Header("数值")]
    public int maxActionPoints = 2;
    [SerializeField] int actionPoints = 0;
    [SerializeField] int maxLifePoints = 10;
    [SerializeField] int lifePoints = 0;
    [SerializeField] int playerValue = 10;
    [SerializeField] float qteTimeWindow = 0.5f;
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
    Vector2Int lastMouseCellPosition;
    public Chess selectedChess = null;
    public void InitPlayer()
    {
        Debug.Log("Init Player");
        StopAllCoroutines();
        actionTypeName.text = defaultActionTypeName;
        shootArrowTransform.gameObject.SetActive(false);
        actionPoints = maxActionPoints;
        actionTypeList = new List<ActionType>() { ActionType.Move, ActionType.Ride, ActionType.Punch, ActionType.Shoot };
        lifePoints = maxLifePoints;
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
        value = playerValue;
        bullet.gameObject.SetActive(false);
        chessTypeName = "Player";
        blockSign.color = Color.clear;
        hitPoints = maxLifePoints;
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
            if (actionType == ActionType.Shoot)
            {
                shootArrowTransform.gameObject.SetActive(true);
            }
            else
            {
                shootArrowTransform.gameObject.SetActive(false);
            }
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
        isActing = false;
        //Debug.Log("玩家的活动状态为：" + isActing);
    }

    void ConsumeActionPoints()
    {
        actionPoints--;
        if (actionPoints <= 0)
        {
            SetSelectedChess(null);
        }
    }

    public void PlayerTurnStart()
    {
        Debug.Log("Player Turn Start");
        if (!isInPlayerTurn)
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
    public void PlayerTurnEnd()
    {
        Debug.Log("Player Turn End");
        if (isInPlayerTurn)
        {
            isInPlayerTurn = false;

            if (selectedChess != null)
            {
                ChessBoard.instance.HideRange();
                SetSelectedChess(null);
            }
            ChessManager.instance.EnemyTurnStart();
        }
    }
    IEnumerator PlayerTurnUpdate()
    {
        while (isInPlayerTurn && actionPoints> 0)
        {
            mouseWorldPosition = sceneCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseCellPosition = ChessBoard.GetCell(mouseWorldPosition);
            if(lastMouseCellPosition != mouseCellPosition)
            {
                lastMouseCellPosition = mouseCellPosition;
                OnUpdateMouseCellPosition();
            }
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

    public IEnumerator BossPrepareCoroutine()
    {
        ChessBoard.instance.HideRange();
        if (ChessBoard.instance[y, x] == this)
            ChessBoard.instance[y, x] = null;
        if(rideOn == baseChess)
        {
            DropRideOn();
            ChessBoard.instance[y, x] = baseChess;
        }
        while (true)
        {
            mouseWorldPosition = sceneCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseCellPosition = ChessBoard.GetCell(mouseWorldPosition);
            transform.position = ChessBoard.GetCellCenterWorld(mouseCellPosition);
            if(Input.GetMouseButtonDown(0))
            {
                if(ChessBoard.GetChess(mouseCellPosition) == null)
                {
                    x = mouseCellPosition.x;
                    y = mouseCellPosition.y;
                    ChessBoard.instance[y, x] = this;
                    break;
                }
            }
            yield return null;
        }
        PlayerTurnEnd();
        yield return new WaitForSeconds(1f);
    }

    public void Reborn()
    {
        SetSelectedChess(null);
        if (ChessBoard.instance[y, x] == this)
            ChessBoard.instance[y, x] = null;
        x = baseChess.x;
        y = baseChess.y;
        transform.position = baseChess.transform.position;
        ChessBoard.instance[y, x] = this;
        this.SetRideOn(baseChess);
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
    {
        if(actionType == ActionType.Shoot && ChessBoard.IsInView(mouseCellPosition.x, mouseCellPosition.y))
        {
            shootArrowTransform.up = (Vector2)(mouseCellPosition - cellPosition);
        }
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
        if (ChessBoard.IsInView(mouseCellPosition.x, mouseCellPosition.y))
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
                    }
                    else if (selectedChess == baseChess)
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
                    if (chessClickedOn == null || chessClickedOn.canBeRiden)
                    {
                        //Debug.Log("骑乘");
                        ConsumeActionPoints();
                        ChessManager.instance.PushActingChess(this);
                        A_Ride(mouseCellPosition.x - x, mouseCellPosition.y - y);
                    }
                }
            }
            else if(actionType == ActionType.Shoot)
            {
                //Debug.Log("射击");
                ConsumeActionPoints();
                ChessManager.instance.PushActingChess(this);
                StartCoroutine(Shoot(mouseCellPosition.x - x, mouseCellPosition.y - y));
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
            ChessBoard.instance.HideRange();
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
    //Player.ActionType lastActionType = Player.ActionType.Move;
    //void OnChangePlayerActionType()
    //{
    //    lastActionType = actionType;
    //    ShowRange();
    //    switch (actionType)
    //    {
    //        case Player.ActionType.Shoot:
    //            shootArrowTransform.gameObject.SetActive(true);
    //            break;
    //        case Player.ActionType.Ride:
    //        default:
    //            shootArrowTransform.gameObject.SetActive(false);
    //            break;
    //    }
    //}




    public override void ShowRange()
    {
        ChessBoard.instance.HideRange();
        if (actionType == ActionType.Move)
            ChessBoard.instance.ShowRange(GetMoveRange(), moveRangeColor, true);
        else if (actionType == ActionType.Punch)
            ChessBoard.instance.ShowRange(GetAttackRange(), attackRangeColor, false);
        else if (actionType == ActionType.Ride)
            ChessBoard.instance.ShowRange(GetRideRange(), rideRangeColor, false);
        else
            ChessBoard.instance.HideRange();
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
        if (y > 0 && (ChessBoard.GetChess(new Vector2Int(x, y - 1)) == null || ChessBoard.GetChess(new Vector2Int(x, y - 1)).canBeRiden))
            rangeList.Add(new Vector2Int(x, y - 1));
        if (x > 0 && (ChessBoard.GetChess(new Vector2Int(x - 1, y)) == null || ChessBoard.GetChess(new Vector2Int(x - 1, y)).canBeRiden))
            rangeList.Add(new Vector2Int(x - 1, y));
        if (x < ChessBoard.instance.colNum - 1 && (ChessBoard.GetChess(new Vector2Int(x + 1, y)) == null || ChessBoard.GetChess(new Vector2Int(x + 1, y)).canBeRiden))
            rangeList.Add(new Vector2Int(x + 1, y));
        if (y < ChessBoard.instance.rowNum - 1 && (ChessBoard.GetChess(new Vector2Int(x, y + 1)) == null || ChessBoard.GetChess(new Vector2Int(x, y + 1)).canBeRiden))
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
    public void A_ActEnd()
    {
        if (actionPoints > 0)
            ShowRange();
        isActing = false;
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
        A_ActEnd();
    }
    
    public void A_Punch(int dx, int dy)
    {
        Chess target = ChessBoard.instance[y + dy, x + dx];
        if (target != null && target.camp != camp)
        {
            target.TakeDamage(10, this, new Vector2Int(dx, dy));
        }
        A_ActEnd();
    }

    public void A_Ride(int dx, int dy)
    {
        Chess target = ChessBoard.instance[y + dy, x + dx];
        Vector3 startPosition = transform.position;
        Vector3 endPosition = transform.position + new Vector3(dx, dy, 0);
        Chess dropedRideOn = null;
        if (rideOn != null)
        {
            rideOn.x = x;
            rideOn.y = y;
            if (ChessBoard.instance[y, x] == this)
                ChessBoard.instance[y, x] = rideOn;
            dropedRideOn = rideOn;
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
        if(dropedRideOn != null && dropedRideOn != baseChess)
            dropedRideOn.TakeDamage(1, this, new Vector2Int(dx, dy));
        A_ActEnd();
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
        ChessManager.instance.PushActingChess(this);
        StartCoroutine(Damaged(attackDirection));
    }

    IEnumerator Damaged(Vector2Int attackDirection)//受击效果
    {
        bool isBlocked = false;
        blockSign.color = Color.yellow;
        for (float t = 0; t < qteTimeWindow; t += Time.deltaTime)
        {
            if(Input.GetMouseButtonDown(0))
            {
                isBlocked = true;
                blockSign.color = Color.green;
                break;
            }
            yield return null;
        }
        if (isBlocked) 
        {
            Debug.Log("格挡成功");
            //yield return new WaitForSeconds(0.2f);
            AddForce(attackDirection);
            ForcedMove();
            blockSign.color = Color.clear;
            yield break;
        }
        else
        {
            blockSign.color = Color.red;
            Debug.Log("格挡失败");
            yield return new WaitForSeconds(0.2f);
            blockSign.color = Color.clear;
        }
        for (float i = 0; i <= 2 * Mathf.PI; i += 1)
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
