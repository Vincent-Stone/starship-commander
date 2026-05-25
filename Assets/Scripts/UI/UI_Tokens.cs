using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Tokens : MonoBehaviour
{
    int count;
    public float spaceing = 0;
    List<RectTransform> tokens;
    RectTransform tokenPrefabRectTransform;
    int value;
    public void Init(int value)
    {
        if(value <= count)
        {
            UpdateTokens(value);
            return;
        }
        tokens = new List<RectTransform>();
        tokenPrefabRectTransform = transform.Find("token").gameObject.GetComponent<RectTransform>();
        if(tokenPrefabRectTransform == null)
        {
            Debug.LogError(name + " can't find token prefab RectTransform");
            return;
        }
        tokens.Add(tokenPrefabRectTransform.GetComponent<RectTransform>());
        count = value;
        float tokenRectWidth = tokenPrefabRectTransform.rect.width; 
        for (int i = 0; i < count - 1; i++)
        {
            RectTransform token = Instantiate(tokenPrefabRectTransform).GetComponent<RectTransform>();
            token.transform.parent = this.transform;
            token.anchorMax = tokenPrefabRectTransform.anchorMax;
            token.anchorMin = tokenPrefabRectTransform.anchorMin;
            token.offsetMax = tokenPrefabRectTransform.offsetMax;
            token.offsetMin = tokenPrefabRectTransform.offsetMin;
            token.anchoredPosition3D = tokens[i].anchoredPosition3D + new Vector3(tokenRectWidth + spaceing, 0, 0);
            token.localScale = tokenPrefabRectTransform.localScale;
            token.localRotation = tokenPrefabRectTransform.localRotation;
            tokens.Add(token);
        }
    }

    public void UpdateTokens(int setValue)
    {
        value = Mathf.Clamp(setValue, 0, count);
        int i;
        for(i=0;i<value;i++)
        {
            tokens[i].gameObject.SetActive(true);
        }
        for(;i<count;i++)
        {
            tokens[i].gameObject.SetActive(false);
        }
    }
}
