using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Base baseChess;
    [SerializeField] private ChessManager chessManager;
    // Start is called before the first frame update
    void Start()
    {
        chessManager.Init();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadStage(int stageIndex)
    {
        // Load the stage data based on the stageIndex
        // This could involve loading a scene, instantiating game objects, etc.
        Debug.Log("Loading stage: " + stageIndex);
        player.maxActionPoints = 2;
        chessManager.Init();
    }
    public void BossStart()
    {
        player.maxActionPoints = 1;
        player.StartCoroutine(player.BossPrepareCoroutine());
    }
}