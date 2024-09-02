using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FixedInputCell : MonoBehaviour
{
    [SerializeField] private int value;
    [SerializeField] private Animator animator;
    [SerializeField] private Image sliderImage;
    [SerializeField] private Image errorSliderImage;
    [SerializeField] private float selectTimeout = 0.5f;
    private bool isSelected;
    private FixedInput fixedInput;
    private float lastSelectTime;

    private void Awake()
    {
        fixedInput = GetComponentInParent<FixedInput>();
        lastSelectTime = Time.time;
    }

    public void Select()
    {
        GameField gameField = fixedInput.GetGameField();
        if (gameField.SelectedCell && gameField.SelectedCell.CellType != CellType.FIXED)
            gameField.SetCellValue(gameField.SelectedCell, value);

        if (isSelected)
        {
            if (Time.time - lastSelectTime > selectTimeout)
                fixedInput.SelectValue(0);

            return;
        }

        fixedInput.SelectValue(value);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        animator.SetBool("Selected", selected);
        if (selected)
            lastSelectTime = Time.time;
    }

    public void UpdateSlider()
    {
        int currentValue = fixedInput.GetGameField().GetValueAmount(value);
        sliderImage.fillAmount = currentValue / 9f;
        errorSliderImage.fillAmount = (currentValue - 9) / 9f;

    }

    public int GetValue()
    {
        return value;
    }
}
