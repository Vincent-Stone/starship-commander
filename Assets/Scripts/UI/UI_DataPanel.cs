using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_DataPanel : MonoBehaviour
{
    public enum SliderType
    {
        Hp,
        BaseHp,
        BaseShield
    }
    [SerializeField] List<Slider> sliderList;
    public void UpdateSlider(SliderType sliderType,float value)
    {
        int index = (int)sliderType;
        if (index < sliderList.Count)
        {
            sliderList[index].value = value;
        }
    }
}
