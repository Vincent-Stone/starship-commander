using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Base baseChess;
    [SerializeField] private ChessManager chessManager;
    public static StageManager instance;
    public static bool isPaused = false;
    public static bool isBossStage = false;
    public static int stageIndex = 1;
    public bool SetPause = false;
    public UI_PopUpWindow popUpWindow;
    void Start()
    {
        if(instance == null)
            instance = this;
        LoadStage();
    }
    public void OpenWindow(string text)
    {
        isPaused = true;
        StartCoroutine(PopUpWindow(text));
    }
    public void OpenWindow(string[] texts)
    {
        isPaused = false;
        StartCoroutine(PopUpMultipleWindows(texts));
    }
    IEnumerator PopUpWindow(string text)
    {
        if(popUpWindow == null)
        {
            Debug.LogError("Pop-Up Window is null");
            yield break;
        }
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
    }
    IEnumerator PopUpMultipleWindows(string[] texts)
    {
        if (popUpWindow == null)
        {
            Debug.LogError("Pop-Up Window is null");
            yield break;
        }
        foreach(string text in texts)
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
    }
    public void LoadStage()
    {
        isBossStage = false;
        Debug.Log("Loading stage: " + stageIndex);
        chessManager.chessDataPath = $"Assets/LO/{stageIndex}.txt";
        player.maxActionPoints = 2;
        chessManager.Init();
    }
    public void BossStart()
    {
        isBossStage = true;
        player.maxActionPoints = 1;
        player.StartCoroutine(player.BossPrepareCoroutine());
    }
}