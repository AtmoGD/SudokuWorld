using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedInput : MonoBehaviour
{
    [SerializeField] private GameField gameField;
    [SerializeField] private List<FixedInputCell> inputElements = new();

    private void Start()
    {
        gameField.OnSetCellValue += UpdateSliders;
    }

    public void SelectValue(int value)
    {
        foreach (var element in inputElements)
            element.SetSelected(element.GetValue() == value);

        gameField.HighlightValue(value);
    }

    public void OverrideValue(int value)
    {
        foreach (var element in inputElements)
            element.SetSelected(element.GetValue() == value);
    }

    public void UpdateSliders()
    {
        foreach (var element in inputElements)
            element.UpdateSlider();
    }


    public GameField GetGameField()
    {
        return gameField;
    }
}
