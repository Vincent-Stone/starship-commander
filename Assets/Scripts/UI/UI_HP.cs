using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UI_HP : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    public Sprite[] tokens;
    
    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void UpdateHP(int hp)
    {
        //Debug.LogError(hp);
        if(spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (hp >= 0 && hp < tokens.Length)
            spriteRenderer.sprite = tokens[hp];
    }
}
