using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingInputValueController : MonoBehaviour
{
    [SerializeField] private bool active;
    [SerializeField] private int value;
    [SerializeField] private Image sliderImage;

    private void Awake()
    {
        UpdateSlider();
    }

    public void UpdateSlider()
    {
        sliderImage.fillAmount = active ? value / 9f : 0f;
    }
}
