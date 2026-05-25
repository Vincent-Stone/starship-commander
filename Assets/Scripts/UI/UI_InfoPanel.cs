using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InfoPanel : MonoBehaviour
{
    public Image image;
    public Image nameTextImage;
    public Image infoTextImage;
    public TextMeshProUGUI chessName;
    public TextMeshProUGUI chessInfo;
    public Sprite defaultSprite;
    public Color defaultColor;
    public Color highlightColor;

    Chess selectedChess = null;
    Chess lastSelectedChess = null;
    [SerializeField] Camera sceneCamera;
    Vector2Int mouseCellPosition;
    void UpdateInfoPanel()
    {
        if(selectedChess == null)
        {
            image.sprite = defaultSprite;
            chessName.text = "";
            chessInfo.text = "";
            nameTextImage.color = infoTextImage.color = defaultColor;
            return;
        }
        if(selectedChess.chessPicture != null)
        {
            image.sprite = selectedChess.chessPicture;
        }
        nameTextImage.color = infoTextImage.color = highlightColor;
        chessName.text = selectedChess.chessName;
        chessInfo.text = selectedChess.chessInfo;
    }

    void Start()
    {
        defaultSprite = image.sprite;

        if (sceneCamera == null)
            sceneCamera = Camera.main;
        UpdateInfoPanel();
    }

    void Update()
    {
        if (ChessBoard.instance == null)
            return;
        mouseCellPosition = ChessBoard.GetCell(sceneCamera.ScreenToWorldPoint(Input.mousePosition));
        if (Input.GetMouseButtonDown(0) && !StageManager.instance.windowIsPlaying && !StageManager.instance.windowIsOpen)
            selectedChess = ChessBoard.GetChess(mouseCellPosition);
        if (selectedChess == lastSelectedChess)
            return;
        lastSelectedChess = selectedChess;
        UpdateInfoPanel();
    }
}
