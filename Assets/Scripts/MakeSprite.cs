using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeSprite : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    [SerializeField] Texture2D texture;
    [SerializeField] Sprite sprite;
    // Start is called before the first frame update
    void Start()
    {
        Rect rect = new Rect(0.0f, 0.0f, texture.width, texture.height);
        Debug.Log(rect);
        sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 600.0f);
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
