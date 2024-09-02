using UnityEngine;
using System;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UIElements;
using System.Collections.Generic;

[Serializable]
public enum CellType
{
    FIXED,
    EMPTY,
    USER
}

[Serializable]
public enum CellSubgrid
{
    TOP_LEFT,
    TOP_MIDDLE,
    TOP_RIGHT,
    MIDDLE_LEFT,
    MIDDLE_MIDDLE,
    MIDDLE_RIGHT,
    BOTTOM_LEFT,
    BOTTOM_MIDDLE,
    BOTTOM_RIGHT
}

public class Cell : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TextMeshProUGUI[] texts;
    [SerializeField] private Animator cellAnimator;
    [SerializeField] private Animator backgroundAnimator;
    [SerializeField] private List<Animator> valueAnimators;

    private GameField gameField;
    public GameField GameField { get { return gameField; } }

    private CellType cellType;
    public CellType CellType { get { return cellType; } }

    private int cellValue;
    public int CellValue { get { return cellValue; } }

    private Vector2Int cellPosition;
    public Vector2Int CellPosition { get { return cellPosition; } }

    private bool isSelected;
    public bool IsSelected
    {
        get { return isSelected; }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        gameField.FloatingInput.PointerDown(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        gameField.FloatingInput.PointerUp();
    }

    public void Select()
    {
        isSelected = true;
        cellAnimator.SetBool("Selected", true);
    }

    public void Deselect()
    {
        isSelected = false;
        cellAnimator.SetBool("Selected", false);
    }

    public void UpdateText()
    {
        string newText = CellValue == 0 ? "" : CellValue.ToString();
        foreach (TextMeshProUGUI text in texts)
            text.text = newText;
    }

    public void SetValue(int value)
    {
        cellValue = value;

        UnhighlightError(); // Just reset it if it was set
        UpdateText();
    }

    public void SetType(CellType type)
    {
        cellType = type;

        if (cellAnimator != null)
            cellAnimator.SetBool("Fixed", cellType == CellType.FIXED);
    }

    public void SetPosition(Vector2Int position)
    {
        cellPosition = position;
    }

    public void SetGameField(GameField field)
    {
        gameField = field;
    }

    public void HighlightBackground()
    {
        backgroundAnimator.SetBool("HighlightedBackground", true);
    }

    public void UnhighlightBackground()
    {
        backgroundAnimator.SetBool("HighlightedBackground", false);
    }

    public void HighlightValue()
    {
        valueAnimators.ForEach((value) =>
        {
            value.SetBool("HighlightedValue", true);
        });
    }

    public void UnhighlightValue()
    {
        valueAnimators.ForEach((value) =>
        {
            value.SetBool("HighlightedValue", false);
        });
    }

    public void HighlightError()
    {
        valueAnimators.ForEach((value) =>
        {
            value.SetBool("ErrorValue", true);
        });
    }

    public void UnhighlightError()
    {
        valueAnimators.ForEach((value) =>
        {
            value.SetBool("ErrorValue", false);
        });
    }

    public void VanishCell()
    {
        cellAnimator.SetBool("Vanish", true);
    }

    public void UnvanishCell()
    {
        cellAnimator.SetBool("Vanish", false);
    }

    public CellSubgrid GetCellSubgrid()
    {
        if (cellPosition.x < 3)
        {
            if (cellPosition.y < 3)
                return CellSubgrid.TOP_LEFT;
            else if (cellPosition.y < 6)
                return CellSubgrid.TOP_MIDDLE;
            else
                return CellSubgrid.TOP_RIGHT;
        }
        else if (cellPosition.x < 6)
        {
            if (cellPosition.y < 3)
                return CellSubgrid.MIDDLE_LEFT;
            else if (cellPosition.y < 6)
                return CellSubgrid.MIDDLE_MIDDLE;
            else
                return CellSubgrid.MIDDLE_RIGHT;
        }
        else
        {
            if (cellPosition.y < 3)
                return CellSubgrid.BOTTOM_LEFT;
            else if (cellPosition.y < 6)
                return CellSubgrid.BOTTOM_MIDDLE;
            else
                return CellSubgrid.BOTTOM_RIGHT;
        }
    }
}
