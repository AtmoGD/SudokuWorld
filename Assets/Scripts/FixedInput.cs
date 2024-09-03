using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixedInput : MonoBehaviour
{
    [SerializeField] private GameField gameField;
    [SerializeField] private List<FixedInputCell> inputElements = new();
    private int selectedValue = 0;
    public int SelectedValue { get { return selectedValue; } }

    private void Start()
    {
        gameField.OnSetCellValue += OnSetCell;
    }

    public void SelectValue(int value)
    {
        selectedValue = value;

        foreach (var element in inputElements)
            element.SetSelected(element.GetValue() == value);

        gameField.FixedInputSelected();
    }

    public void OnSetCell()
    {
        foreach (var element in inputElements) // Update sliders before early return
            element.UpdateSlider();

        if (gameField.SelectedCell == null) return;

        if (gameField.SelectedCell.CellValue != 0)
        {
            selectedValue = gameField.SelectedCell.CellValue;

            foreach (var element in inputElements)
                element.SetSelected(element.GetValue() == selectedValue);
        }
    }


    public GameField GetGameField()
    {
        return gameField;
    }
}
