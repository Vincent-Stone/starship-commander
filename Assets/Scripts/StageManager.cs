using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Base baseChess;
    [SerializeField] private ChessManager chessManager;
    public static StageManager instance;
    public static bool isPaused = false;
    public static bool isBossStage = false;
    public static int stageIndex = 1;
    public int maxStageIndex = 3;
    public bool SetPause = false;
    public static bool isInited = false;
    public static bool youWin = false;
    public static bool youLose = false;
    [Header("UI")]
    public UI_InfoPanel infoPanel;
    public UI_PopUpWindow popUpWindow;
    public bool windowIsOpen => popUpWindow.isOpen;
    public bool windowIsPlaying => popUpWindow.isPlaying;
    public Image curtain;
    public Color curtainOriginalColor;
    public TextMeshProUGUI thanksToPlay;
    [Header("测试")]
    [Range(1,3)]
    public int SetStageIndex = 1;
    void Start()
    {
        if(instance == null)
        {
            instance = this;
            stageIndex = SetStageIndex;
        }
        //if (infoPanel != null)
        //    infoPanel.enabled = false;
        curtainOriginalColor = curtain.color;
        LoadStage();
    }

    public void OpenWindow(string text)
    {
        StartCoroutine(PopUpWindow(text));
    }
    public void OpenWindow(string[] texts)
    {
        StartCoroutine(PopUpMultipleWindows(texts));
    }
    IEnumerator PopUpWindow(string text)
    {
        if(popUpWindow == null)
        {
            Debug.LogError("Pop-Up Window is null");
            yield break;
        }
        isPaused = true;
        popUpWindow.Open(text);
        while (popUpWindow.isPlaying || !popUpWindow.isOpen)
            yield return null;
        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                popUpWindow.Close();
                break;
            }
            yield return null;
        }
        while (popUpWindow.isPlaying)
            yield return null;
        isPaused = false;
        if (youWin || youLose)
        {
            youWin = youLose = false;
            LoadStage();
        }
    }
    IEnumerator PopUpMultipleWindows(string[] texts)
    {
        if (popUpWindow == null)
        {
            Debug.LogError("Pop-Up Window is null");
            yield break;
        }
        //while (popUpWindow.isPlaying || !popUpWindow.isOpen)
        //    yield return null;
        isPaused = true;
        foreach (string text in texts)
        {
            popUpWindow.Open(text);
            while (popUpWindow.isPlaying || !popUpWindow.isOpen)
                yield return null;
            while (true)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    popUpWindow.Close();
                    break;
                }
                yield return null;
            }
            while (popUpWindow.isPlaying || popUpWindow.isOpen)
                yield return null;
        }
        isPaused = false;
        if (youWin || youLose)
        {
            youWin = youLose = false;
            LoadStage();
        }
    }
    public void LoadStage()
    {
        isBossStage = false;
        isInited = false;
        Debug.Log("Loading stage: " + stageIndex);
        chessManager.chessDataPath = $"Assets/LO/{stageIndex}.txt";
        chessManager.restarted = true;
        player.maxActionPoints = 2;
        StartCoroutine(LoadingCoroutine());
    }

    public void YouWin(string text = "你成功保卫了基地")
    {
        youWin = true;
        youLose = false;
        if (ChessBoard.GetChess(player.cellPosition) == player)
            ChessBoard.instance[player.y, player.x] = player.rideOn;
        player.DropRideOn();
        stageIndex++;
        OpenWindow(text);
    }
    public void YouLose(string text = "失败")
    {
        youLose = true;
        youWin = false;
        OpenWindow(text);
    }
    IEnumerator LoadingCoroutine()
    {
        Color color = curtainOriginalColor;
        color.a = 0;
        if(!curtain.gameObject.activeSelf)
        {
            curtain.color = color;
            curtain.gameObject.SetActive(true);
            for (float t = 0; t < 1; t += Time.deltaTime / 0.1f)
            {
                color.a = curtainOriginalColor.a * t;
                curtain.color = color;
                yield return null;
            }
            curtain.color = curtainOriginalColor;
            Debug.Log("Curtain Up");
        }
        infoPanel.enabled = false;
        if (stageIndex > maxStageIndex)
        {
            thanksToPlay.color = Color.clear;
            thanksToPlay.gameObject.SetActive(true);
            for(float i = 0; i < 1; i += Time.deltaTime / 0.5f)
            {
                thanksToPlay.color = new Color(1, 1, 1, i);
                yield return null;
            }
            thanksToPlay.color = Color.white;
            yield break;
        }
        chessManager.Init();
        while (!isInited)
            yield return null;
        infoPanel.enabled = true;
        for(float t = 1; t > 0; t -= Time.deltaTime)
        {
            color.a = curtainOriginalColor.a * t;
            curtain.color = color;
            yield return null;
        }
        color.a = 0;
        curtain.color = color;
        curtain.gameObject.SetActive(false);
    }
    public void BossStart()
    {
        if (isBossStage)
            return;
        string[] testTexts = {
               "决战阶段，你可以挑选战斗员的位置。"
                };
        StageManager.instance.OpenWindow(testTexts);
        isBossStage = true;
        player.maxActionPoints = 1;
        player.StartCoroutine(player.BossPrepareCoroutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}