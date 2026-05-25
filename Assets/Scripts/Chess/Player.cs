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
        Enemy,
        Null
    }
    public AnimationCurve setForcedMoveCurve;
    [SerializeField] Bullet bullet;
    //[SerializeField] UI_DataPanel dataPanel;
    // [SerializeField] TextMeshProUGUI rideOnName;
    [Header("行动类型")]
    [SerializeField] TextMeshProUGUI actionTypeName;
    public Vector2Int actionTarget = Vector2Int.zero;
    public ActionType actionType = ActionType.Move;
    public Chess rideOn = null;
    public static Player instance;
    public Base baseChess;
    //public Weapon weapon;
    public bool isDead = false;
    bool isInPlayerTurn = false;
    string defaultActionTypeName { get {
            if (actionPoints > 0)
                return "选择一个\n单位";
            else
                return "低能量";
        }
    }
    [Header("UI标志")]
    public Transform shootArrowTransform;
    [SerializeField] SpriteRenderer blockSign;
    [SerializeField] TextMeshProUGUI actionPointsText;
    public GameObject selectBox;
    public UI_Tokens lifeTokens;
    //public SpriteRenderer bossAreaCurtain;
    //public SpriteRenderer bossAreaLine;
    //public Color bossCurtainColor;
    //public Color bossLineColor;
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
    [Header("音频")]
    public AudioClip punchSound;
    public AudioClip dieSound;
    public AudioClip damagedSound;
    public AudioClip blockSound;
    public AudioClip blockSuccessSound;
    public AudioSource audioSource;
    //教程提示记录
    bool isFirstTurn = true;
    bool isThisStageFirstTurn = true;
    public bool isBossStart = false;
    bool haveSelectedPlayer = false;
    bool haveSelectedBase = false;
    bool haveSelectedEnemy = false;
    bool firstDie = true;
    public void InitPlayer()
    {
        isActing = false;
        isDead = false;
        //if(StageManager.stageIndex != 1)
        //if(StageManager.stageIndex == 1 && isFirstTurn)
        //{
        //    //isFirstTurn = true;
        //    haveSelectedPlayer = false;
        //    haveSelectedBase = false;
        //    haveSelectedEnemy = false;
        //    firstDie = true;
        //}
        //else
        //{
        //    isFirstTurn = false;
        //    haveSelectedPlayer = true;
        //    haveSelectedBase = true;
        //    haveSelectedEnemy = true;
        //}
        isThisStageFirstTurn = true;
        Debug.Log("Init Player");
        StopAllCoroutines();
        actionTypeName.text = defaultActionTypeName;
        shootArrowTransform.gameObject.SetActive(false);
        actionPoints = maxActionPoints;
        UpdateActionPointsText();
        actionTypeList = new List<ActionType>() { ActionType.Move, ActionType.Ride, ActionType.Punch, ActionType.Shoot };
        lifePoints = maxLifePoints;
        if(lifeTokens == null)
        {
            Debug.LogError(name + ":life token is null");
        }
        lifeTokens.Init(maxLifePoints);
        if(sceneCamera == null)
        {
            sceneCamera = Camera.main;
        }
        if(audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        audioSource.clip = punchSound;
        if (shootArrowTransform == null)
            Debug.LogError("Shoot arrow is null!");
        camp = 0;
        value = playerValue;
        bullet.gameObject.SetActive(false);
        chessTypeName = "Player";
        chessName = "战斗员";
        chessInfo = "<size=50><b>战斗员</b></size>是由基地内工厂自动生产出的机器人。\n<size=20>\n</size>作为完美战士，其外形完全适应战斗，因此也与建造它的伟大生物——<b>恐龙</b>的身形有较大的不同。";

        blockSign.color = Color.clear;
        hitPoints = maxHitPoints;
        hpUI.UpdateHP(hitPoints);
        sprite.gameObject.SetActive(true);
        //bossAreaCurtain.color = bossCurtainColor;
        //bossAreaLine.color = Color.clear;
        //bossAreaLine.transform.localScale = new Vector3(
        //    0,
        //    bossAreaLine.transform.localScale.y,
        //    bossAreaLine.transform.localScale.z);
        SetSelectedChess(null);
    }

    public static void CreateInstance()
    {
        if (instance == null)
        {
            instance = FindFirstObjectByType<Player>();
            if (forcedMoveCurve == null)
                forcedMoveCurve = instance.setForcedMoveCurve;
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
            if(selectedChess.camp==camp && actionPoints <= 0)
            {
                actionTypeName.text = defaultActionTypeName;
            }
            else
            {
                switch (actionType)
                {
                    case ActionType.Move:
                        actionTypeName.text = "移动";
                        break;
                    case ActionType.Ride:
                        actionTypeName.text = "搭乘";
                        break;
                    case ActionType.Punch:
                        actionTypeName.text = "攻击";
                        break;
                    case ActionType.Shoot:
                        actionTypeName.text = "冰枪";
                        break;
                    case ActionType.Enemy:
                        actionTypeName.text = "敌人";
                        break;
                    case ActionType.Null:
                        actionTypeName.text = "无";
                        break;
                    default:
                        actionTypeName.text = defaultActionTypeName;
                        break;
                }
                selectedChess.ShowRange();
            }
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
        actionType = ActionType.Null;
        actionTypeName.text = defaultActionTypeName;
    }
    public void SetActionType(int type)
    {
        actionType = (ActionType)type;
    }

    public void SetRideOn(Chess newRideOn)
    {
        this.rideOn = newRideOn;
        newRideOn.transform.parent = this.transform;
        newRideOn.rider = this;
        if (rideOn != null && rideOn != baseChess)
        {
            rideOn.hpUI.UpdateHP(0);
            rideOn.Unfreeze(0);
        }
        //rideOnName.textmeshContent = rideOn.chessTypeName;
    }

    public void DropRideOn()
    {
        if (rideOn == null)
            return;
        if (rideOn != null && rideOn != baseChess)
            rideOn.Freeze(100000);
        rideOn.hpUI.UpdateHP(rideOn.hitPoints);
        rideOn.rider = null;
        rideOn.transform.parent = ChessManager.instance.transform;
        rideOn = null;
        //rideOnName.textmeshContent = "Space";
    }
    public override void Act()
    {
        isActing = false;
        //Debug.Log("玩家的活动状态为：" + isActing);
    }

    void UpdateActionPointsText()
    {
        switch (actionPoints)
        {
            case 0:
                actionPointsText.text = "X";
                break;
            case 1:
                actionPointsText.text = "1";
                break;
            case 2:
                actionPointsText.text = "2";
                break;
            default:
                actionPointsText.text = "  ";
                break;
        }
    }

    void ConsumeActionPoints()
    {
        actionPoints--;
        UpdateActionPointsText();
        if (actionPoints <= 0)
        {
            SetSelectedChess(null);
        }
    }

    public void PlayerTurnStart()
    {
        bool isBossStartFirstTurn = isThisStageFirstTurn && isBossStart;
        isThisStageFirstTurn = false;
        if (isFirstTurn)
        {
            string[] testTexts = {
                //"<size=100>STAGE-1</size>",
                "战斗开始了，邪恶的BF团派出了新的敌人，保卫好你的<b><size=90>基地</size></b>!",
                "尽管基地有<b><size=90>护盾保护</size></b>，但当护盾被击穿，基地被敌人攻击，其<b><size=90>安全值</size></b>就会下降,"+
                "而安全值降到零便意味着你<b><size=90>输</size></b>了",
                "当然你需要使用战斗员与敌人战斗，而战斗员的生命耗尽，也意味着你<b><size=90>输</size></b>了",
                "选择一个单位吧"
            };
            StageManager.instance.OpenWindow(testTexts);
            isFirstTurn = false;
        }
        if (isBossStartFirstTurn)
        {
            isBossStart = false;
            StageManager.instance.BossStart();
        }
        Debug.Log("Player Turn Start");
        if (!isInPlayerTurn)
        {
            isInPlayerTurn = true;
        }
        actionPoints = maxActionPoints;
        UpdateActionPointsText();
        if (StageManager.isBossStage && !isBossStartFirstTurn)
            SetSelectedChess(this);
        else
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
        bool haveClicked = false;
        while (isInPlayerTurn)
        {
            while(StageManager.isPaused) // 可暂停
                yield return null;
            mouseWorldPosition = sceneCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseCellPosition = ChessBoard.GetCell(mouseWorldPosition);
            if(lastMouseCellPosition != mouseCellPosition)
            {
                lastMouseCellPosition = mouseCellPosition;
                OnUpdateMouseCellPosition();
            }
            if(!ChessManager.instance.haveActingChess() && !isActing)
            {
                if (!haveClicked && Input.GetMouseButtonDown(0) && ChessBoard.IsInView(mouseCellPosition.x, mouseCellPosition.y) && !ChessManager.instance.haveActingChess() && !isActing)
                {
                    OnClicked();
                    haveClicked = true;
                }
            }
            while (ChessManager.instance.haveActingChess())
            {
                while (ChessManager.instance.haveActingChess() && !ChessManager.instance.PeekActingChess().isActing)
                    ChessManager.instance.PopActingChess();
                yield return null;
            }
            if (ChessBoard.IsInView(mouseCellPosition) && !ChessManager.instance.haveActingChess())
            {
                selectBox.SetActive(true);
                selectBox.transform.position = ChessBoard.GetCellCenterWorld(mouseCellPosition);
            }
            else
            {
                selectBox.SetActive(false);
            }
            haveClicked = false;
            yield return null;
        }
    }

    public IEnumerator BossPrepareCoroutine()
    {
        while (ChessManager.instance.haveActingChess() || !StageManager.instance.windowIsOpen || !StageManager.instance.windowIsPlaying)
        {
            yield return null;
        }
        ChessManager.instance.PushActingChess(this);
        ChessBoard.instance.HideRange();
        if (ChessBoard.instance[y, x] == this)
            ChessBoard.instance[y, x] = null;
        if(rideOn == baseChess)
        {
            DropRideOn();
            ChessBoard.instance[y, x] = baseChess;
        }
        yield return new WaitForSeconds(0.5f);

        /*for (float t = 0; t < 1; t += Time.deltaTime / 0.1f)
        {
           //Color c = bossCurtainColor;
            //c.a *= 1 - t;
            //bossAreaCurtain.color = c;
            Color c = bossLineColor;
            c.a *= t;
            bossAreaLine.color = c;
            bossAreaLine.transform.localScale = new Vector3(
            20 * t,
            bossAreaLine.transform.localScale.y,
            bossAreaLine.transform.localScale.z);
            yield return null;
        }*/

        while (true)
        {
            mouseWorldPosition = sceneCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseCellPosition = ChessBoard.GetCell(mouseWorldPosition);
            cellPosition = ChessBoard.ClampInBorder(mouseCellPosition, new Vector2Int(ChessBoard.instance.colNum - 1, ChessBoard.instance.bossAreaLine - 1/*(ChessManager.instance.highestRow + ChessManager.instance.basePosition.y) / 2*/), new Vector2Int(0, ChessManager.instance.basePosition.y));
            if (rideOn != null)
                rideOn.cellPosition = cellPosition;
            transform.position = ChessBoard.GetCellCenterWorld(cellPosition);
            if(Input.GetMouseButtonDown(0))
            {
                if(ChessBoard.GetChess(cellPosition) == null)
                {
                    ChessBoard.instance[y, x] = this;
                    isActing = false;
                    yield return null;
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
        sprite.gameObject.SetActive(true);
        if (ChessBoard.instance[y, x] == this)
            ChessBoard.instance[y, x] = null;
        x = baseChess.x;
        y = baseChess.y;
        transform.position = baseChess.transform.position;
        ChessBoard.instance[y, x] = this;
        SetRideOn(baseChess);
        isDead = false;
        hitPoints = maxHitPoints;
        hpUI.UpdateHP(hitPoints);
        audioSource.clip = punchSound;
        audioSource.Play();
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
        if (actionType == ActionType.Shoot)
        {
            if (ChessBoard.IsInView(mouseCellPosition.x, mouseCellPosition.y) && mouseCellPosition != cellPosition)
            {
                shootArrowTransform.gameObject.SetActive(true);
                shootArrowTransform.up = (Vector2)(mouseCellPosition - cellPosition);
            }
            else
            {
                shootArrowTransform.gameObject.SetActive(false);
            }
        }
        else
        {
            shootArrowTransform.gameObject.SetActive(false);
        }
    }

    void OnClicked()
    {
        selectBox.SetActive(false);
        if (selectedChess == null || actionPoints <= 0)
        {
            if (ChessBoard.IsOnBoard(mouseCellPosition))
            {
                Chess chessClickedOn = ChessBoard.instance[mouseCellPosition.y, mouseCellPosition.x];
                if (chessClickedOn != null && chessClickedOn != selectedChess)
                {
                    SetSelectedChess(chessClickedOn);
                }
            }
            return;
        }
        if (ChessBoard.IsInView(mouseCellPosition))
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
                        ChessBoard.instance.HideRange();
                        ChessManager.instance.PushActingChess(this);
                        A_Move(mouseCellPosition.x - x, mouseCellPosition.y - y);
                    }
                    else if (selectedChess == baseChess)
                    {
                        //Debug.Log("移动");
                        ConsumeActionPoints();
                        ChessBoard.instance.HideRange();
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
                        ChessBoard.instance.HideRange();
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
            else if(actionType == ActionType.Shoot && cellPosition != mouseCellPosition)
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
        if(StageManager.isBossStage && selected == baseChess)
            { return; }
        if(selectedChess != null)
            ChessBoard.instance.HideRange();
        selectedChess = selected;
        if (selected != null)
        {
            if (selected.camp != camp || actionPoints > 0)
                selected.ShowRange();
            selected.actionTypeIndex += selectedChess.actionTypeList.Count - 1;
            ChangeActionType();
            if(selectedChess == this && !haveSelectedPlayer)
            {
                haveSelectedPlayer = true;
                string[] testTexts = {
                "这是战斗员，你的战斗单位，你可以看到它的移动范围。",
                "战斗员有<color=green><b><size=90>“移动”</b></size></color><color=yellow><b><size=90>“搭乘/脱离”</b></size></color><color=red><b><size=90>“攻击”</b></size></color><color=blue><b><size=90>“冰枪射击”</b></size></color>四种行动模式，在画面右侧的行动盘可以进行切换\n" +
                "<color=green><b><size=90>“移动”</size></b></color>可以使战斗员行走；<color=yellow><b><size=90>“搭乘”</size></b></color>可以使战斗员在<size=80><b>部分</b></size>敌人安全值降到最低（1点）时将其俘获，获得其移动范围和攻击范围",
                "<color=yellow><b><size=90>“脱离”</size></b></color>可以使战斗员离开其搭乘的敌人，并将该敌人永久冻结；<color=red><b><size=90>“攻击”</size></b></color>顾名思义，降低敌人的安全值；<color=blue><b><size=90>“冰枪射击”</size></b></color>即是用枪射出会反弹的子弹，" +
                "会冻结并击退其击中的棋子，不包括基地。",
                "行动盘旁边的数字是<size=90><b>行动点</b></size>，每次行动都会消耗行动点；一般每回合的行动点为两点，进入决战阶段后，每回合的行动点会变为一，战斗节奏也会变得更快。",
                "对了，还有一点差点忘记说:<b><size=90>“格挡”</size></b>，当战斗员遭受攻击时，其身上会显示一个图标，在此时按下鼠标左键，便可完成一次格挡；记得试试看，这是个很有用的技巧，" +
                "可以保护战斗员不受伤害"
                };
                StageManager.instance.OpenWindow(testTexts);
            }
            if (selectedChess == baseChess && !haveSelectedBase)
            {
                haveSelectedBase = true;
                string[] testTexts = {
                "这是你的基地，它只能向<b><size=90>正上方</size></b>移动。",
                "当某一行没有任何单位时，基地便可移动到那一行；基地移动时，视野也会随之移动，当敌方的基地进入视野，决战阶段便会开始。",
                };
                StageManager.instance.OpenWindow(testTexts);
            }
            if (selectedChess.camp != camp && !haveSelectedEnemy)
            {
                haveSelectedEnemy = true;
                string[] testTexts = {
                "这是一个敌人单位，你可以看到它的<color=green><b><size=90>移动范围</size></b></color>和<color=red><b><size=90>攻击范围</size></b></color>",
                "每个敌人的攻击方式不同，它们的具体信息会在信息面板中给出"
                };
                StageManager.instance.OpenWindow(testTexts);
            }
        }
        else
        {
            ChangeActionType();
            actionTypeName.text = defaultActionTypeName;
        }
    }

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
            for (int i = 0; i < rangeList.Count; i++)
            {
                if (!StageManager.isBossStage)
                {
                    if (rangeList[i].y >= ChessBoard.instance.bossAreaLine)
                    {
                        rangeList.RemoveAt(i);
                        i--;
                    }
                }
            }
            if (rangeList.Count > 0)
                return rangeList;
        }
        if (y > 0 && ChessBoard.instance[y - 1, x] == null)
            rangeList.Add(new Vector2Int(x, y - 1));
        if (x > 0 && ChessBoard.instance[y, x - 1] == null)
            rangeList.Add(new Vector2Int(x - 1, y));
        if (x < ChessBoard.instance.colNum - 1 && ChessBoard.instance[y, x + 1] == null)
            rangeList.Add(new Vector2Int(x + 1, y));
        if (y < ChessBoard.instance.rowNum - 1 && ChessBoard.instance[y + 1, x] == null)
        {
            if (y + 1 < ChessBoard.instance.bossAreaLine || StageManager.isBossStage)
                rangeList.Add(new Vector2Int(x, y + 1));
        }
        return rangeList;
    }
    public override List<Vector2Int> GetAttackRange()
    {
        List<Vector2Int> rangeList = new List<Vector2Int>();
        if (rideOn != null && rideOn.canBeRiden)
        {
            rangeList = rideOn.GetAttackRange();
            for (int i = 0; i < rangeList.Count; i++)
            {
                if (!StageManager.isBossStage)
                {
                    if (rangeList[i].y >= ChessBoard.instance.bossAreaLine)
                    {
                        rangeList.RemoveAt(i);
                        i--;
                    }
                }
            }
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
        {
            if (y + 1 < ChessBoard.instance.bossAreaLine || StageManager.isBossStage)
                rangeList.Add(new Vector2Int(x, y + 1));
        }
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
        {
            if (y + 1 < ChessBoard.instance.bossAreaLine || StageManager.isBossStage)
                rangeList.Add(new Vector2Int(x, y + 1));
        }
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
    public IEnumerator A_ActEnd()
    {
        audioSource.clip = punchSound;
        audioSource.Play();
        isActing = false;
        while(ChessManager.instance.haveActingChess())
        {
            yield return null;
        }
        if (actionPoints > 0)
        {
            if(actionType == ActionType.Shoot)
                shootArrowTransform.gameObject.SetActive(true);
            ShowRange();
        }
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
        if (rideOn == null && ChessBoard.instance[y, x] == baseChess)
        {
            SetRideOn(baseChess);
        }
        ChessBoard.instance[y,x] = this;
        StartCoroutine(MoveCoroutine(startPosition, endPosition));
    }
    public override IEnumerator MoveCoroutine(Vector3 startPosition, Vector3 endPosition)
    {
        for (float t = 0; t < 1f; t += (Time.deltaTime / moveDuration))
        {
            while (StageManager.isPaused) // 可暂停
                yield return null;
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }
        transform.position = endPosition;
        StartCoroutine(A_ActEnd());
    }

    public void A_Punch(int dx, int dy)
    {
        Chess target = ChessBoard.instance[y + dy, x + dx];
        if (target != null && target.camp != camp)
        {
            audioSource.clip = punchSound;
            audioSource.Play();
            target.TakeDamage(1, this, new Vector2Int(dx, dy));
        }
        StartCoroutine(A_ActEnd());
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
        StartCoroutine(RideCoroutine(startPosition, endPosition, target));
    }
    public IEnumerator RideCoroutine(Vector3 startPosition, Vector3 endPosition, Chess target)
    {
        for (float t = 0; t < 1f; t += (Time.deltaTime / moveDuration))
        {
            while (StageManager.isPaused) // 可暂停
                yield return null;
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }
        transform.position = endPosition;
        if (target != null && target.canBeRiden)
        {
            SetRideOn(target);
        }
        StartCoroutine(A_ActEnd());
    }

    public IEnumerator Shoot(int targetX, int targetY)
    {
        Debug.Log("Player Shoot");
        shootArrowTransform.gameObject.SetActive(false);
        bullet.gameObject.SetActive(true);
        bullet.shooter = this;
        bullet.Shoot(new Vector2(targetX, targetY).normalized);
        yield break;
    }

    public void ShootEnd()
    {
        StartCoroutine(A_ActEnd());
    }

    public override void TakeDamage(int damage, Chess attacker = null, Vector2Int attackDirection = new Vector2Int())
    {
        ChessManager.instance.PushActingChess(this);
        StartCoroutine(Damaged(damage, attacker, attackDirection));
    }

    public override void Die()
    {
        if (firstDie)
        {
            firstDie = false;
            string[] testTexts = {
               "<size=95><b>恭喜</b></size>，你的战斗员阵亡了！",
               "不过只要战斗员还有剩余生命，它就会在<size=80><b>下一回合开始时</b></size>基地所在位置复活。"
                };
            StageManager.instance.OpenWindow(testTexts);
        }
        if (ChessBoard.instance[this.y, this.x] == this)
        {
            ChessBoard.instance[this.y, this.x] = rideOn;
            if (rideOn != null)
            {
                DropRideOn();
            }
        }
        lifePoints--;
        lifeTokens.UpdateTokens(lifePoints);
        if(lifePoints == 0)
        {
            StageManager.instance.YouLose();
        }
        audioSource.clip = dieSound;
        audioSource.Play();
        sprite.gameObject.SetActive(false);
        isActing = false;
        isDead = true;
    }

    public override IEnumerator Damaged(int damage, Chess attacker = null, Vector2Int attackDirection = new Vector2Int())//受击效果
    {
        bool isBlocked = false;
        blockSign.color = Color.yellow;
        audioSource.clip = blockSound;
        audioSource.Play();
        for (float t = 0; t < qteTimeWindow; t += Time.deltaTime)
        {
            if (Input.GetMouseButtonDown(0))
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
            audioSource.clip = blockSuccessSound;
            audioSource.Play();
            yield return new WaitForSeconds(0.2f);
            AddForce(attackDirection);
            ForcedMove();
            blockSign.color = Color.clear;
            yield break;
        }
        else
        {
            blockSign.color = Color.red;
            hitPoints -= damage;
            Debug.Log("格挡失败");
            audioSource.clip = damagedSound;
            audioSource.Play();
            yield return new WaitForSeconds(0.2f);
            blockSign.color = Color.clear;
        }
        for (float i = 0; i <= 2 * Mathf.PI; i += 1)
        {
            sprite.Rotate(new Vector3(0, 0, i));
            yield return null;
        }
        sprite.rotation = Quaternion.identity;
        hpUI.UpdateHP(hitPoints);
        if(hitPoints <= 0)
        {
            Die();
        }
        isActing = false;
    }
    public override void ForcedMove()
    {
        Vector2Int forcedMoveTarget = GetForcedMoveTarget();
        if (forcedMoveTarget == new Vector2(x, y))
        {
            isActing = false;
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
