using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Slider : MonoBehaviour
{
    public RectTransform imageTransform;
    float width;
    [Header("≤‚ ‘")]
    [Range(0,1)]
    public float setValue;
    void Start()
    {
        width = imageTransform.rect.width;
        if (imageTransform == null)
        {
            Debug.LogError(name + " Image Transform is null");
            return;
        }
        imageTransform.anchorMax = new(1, 1);
        imageTransform.anchorMin = new(0, 0);
        imageTransform.offsetMax = new(0, 0);
        imageTransform.offsetMin = new(0, 0);
        value = setValue;
    }
    public float value 
    {
        get
        {
            return value;
        }
        set
        {
            value = Mathf.Clamp01(value);
            imageTransform.offsetMax = new((value - 1) * width, 0);
        }
    }
    private void OnDrawGizmos()
    {
        value = setValue;
    }
}
