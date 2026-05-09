using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_PopUpWindow : MonoBehaviour
{
    public TextMeshProUGUI textmeshContent;
    public Animator animator;
    public bool isPlaying = false;
    public bool isOpen = false;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void Close()
    {
        if (isPlaying || !isOpen)
            return;
        isPlaying = true;
        animator.Play("关闭窗口");
    }
    public void EndClose()
    {
        textmeshContent.text = "";
        isOpen = false;
        isPlaying = false;
    }
    public void Open(string text)
    {
        if (isPlaying || isOpen)
            return;
        isPlaying = true;
        textmeshContent.text = text;
        animator.Play("弹出窗口");
    }
    public void EndOpen()
    {
        isOpen = true;
        isPlaying = false;
    }
}
