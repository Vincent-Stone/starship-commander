using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UI_AdjustPanels : MonoBehaviour
{
    [SerializeField] RectTransform leftPanel, rightPanel;
    [Header("–≈œ¢√Ê∞Â")]
    [SerializeField] string infoPanelName;
    [SerializeField] string infoImagePanelName;
    [SerializeField] string infoTextPanelName;
    [SerializeField] RectTransform infoPanel;
    [SerializeField] RawImage infoImage;
    [SerializeField] TextMeshProUGUI infoText;

    //[SerializeField] RectTransform chessBoardPanel;
    [SerializeField] float chessBoardSize = 4.5f;
    [SerializeField] Camera mainCamera;
    float girdSize = 0;
    float chessBoardWidth = 0;
    Vector2 mousePosition;
    public bool isInitialized = false;
    private void Start()
    {
        if(infoPanel == null) 
            infoPanel = leftPanel.transform.Find(infoPanelName).GetComponent<RectTransform>();
        if(infoPanel != null)
        {
            infoImage = infoPanel.transform.Find(infoImagePanelName).GetComponentInChildren<RawImage>();
            infoText = infoPanel.transform.Find(infoTextPanelName).GetComponentInChildren<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError("Error:infoPanel not found");
        }
    }
    void Adjust()
    {
        if (leftPanel == null || rightPanel == null)
        {
            Debug.LogError("LeftPanel or RightPanel is not assigned.");
        }
        if(mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        girdSize = Screen.height / 10;
        chessBoardWidth = chessBoardSize * girdSize;

        leftPanel.sizeDelta = rightPanel.sizeDelta = new Vector2((Screen.width / 2 - chessBoardWidth) * 2560f / Screen.width, 0);
        leftPanel.offsetMax = new Vector2(leftPanel.sizeDelta.x, 0);
        rightPanel.offsetMin = new Vector2(-rightPanel.sizeDelta.x, 0);
        //Debug.Log(leftPanel.sizeDelta + " " + rightPanel.sizeDelta);
    }
    Vector2 lastLeftPanelSize, lastRightPanelSize, lastLeftPanelOffset, lastRightPanelOffset;
    public IEnumerator StartAdjust()
    {
        isInitialized = false;
        while (!isInitialized)
        {
            lastLeftPanelSize = leftPanel.sizeDelta;
            lastRightPanelSize = rightPanel.sizeDelta;
            lastLeftPanelOffset = leftPanel.offsetMax;
            lastRightPanelOffset = rightPanel.offsetMin;
            Adjust();
            if(lastLeftPanelSize == leftPanel.sizeDelta && 
                lastRightPanelSize == rightPanel.sizeDelta && 
                lastLeftPanelOffset == leftPanel.offsetMax && 
                lastRightPanelOffset == rightPanel.offsetMin)
            {
                isInitialized= true;
            }
        }
            yield return null;
    }
    public void OnScreenSizeChanged()
    {
        Adjust();
    }
    public void GetMouseInput(out int area, out Vector2 mouseWorldPosition)
    {
        mousePosition = Input.mousePosition;
        mouseWorldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        if(mousePosition.x< 0 || mousePosition.x > Screen.width || mousePosition.y < 0 || mousePosition.y > Screen.height)
        {
            area = -1; // Mouse is outside the screen.
            return;
        }
        if (mousePosition.x < Screen.width / 2 - chessBoardWidth) // Mouse is on the left panel.
        {
            area = 0;
        }
        else if(mousePosition.x > Screen.width / 2 + chessBoardWidth) // Mouse is on the right panel.
        {
            area = 2;
        }
        else // Mouse is on the chess board panel.
        {
            area = 1;
        }
    }
    public void UpdateInfoPanel(Chess select)
    {
        //infoPanel.gameObject.SetActive(true);
        if(!infoPanel.gameObject.activeSelf)
        {
            return;
        }
        //switch(select.chessty)
        if(select != null)
            infoText.text= select.GetType().Name;
        else
            infoText.text = "Empty Space";
    }

    bool infoPanelIsChanging = false;
    public void ChangeInfoPanelActive()
    {
        if (!infoPanelIsChanging)
        {
            if (infoPanel.gameObject.activeSelf)
            {
                StartCoroutine(CloseInfoPanel());
            }
            else
            {
                StartCoroutine(OpenInfoPanel());
            }
        }
    }

    IEnumerator OpenInfoPanel()
    {
        while (infoPanelIsChanging)
        {
            yield return null;
        }
        if (infoPanel.gameObject.activeSelf)
            yield break;
        infoPanelIsChanging = true;
        infoPanel.gameObject.SetActive(true);
        //for (float i = 0; i < 1; i += Time.deltaTime)
        //{
        //    //Debug.Log("Opening...");
        //    yield return null;
        //}
        infoPanelIsChanging = false;
    }
    IEnumerator CloseInfoPanel()
    {
        while (infoPanelIsChanging)
        {
            yield return null;
        }
        if (!infoPanel.gameObject.activeSelf)
            yield break;
        infoPanelIsChanging = true;
        //for (float i = 0; i < 1; i += Time.deltaTime)
        //{
        //    //Debug.Log("Closing...");
        //    yield return null;
        //}
        infoPanel.gameObject.SetActive(false);
        infoPanelIsChanging = false;
    }
}

