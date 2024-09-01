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
public enum CellQuarter
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
    [SerializeField] private float selectTimeout = 0.5f;

    private float lastSelectTime = 0;

    private GameField gameField;
    public GameField GameField
    {
        get { return gameField; }
        set { gameField = value; }
    }

    private CellType cellType;
    public CellType CellType
    {
        get { return cellType; }
        set
        {
            cellType = value;
            if (cellAnimator != null)
                cellAnimator.SetBool("Fixed", cellType == CellType.FIXED);
        }
    }

    // Idee: Eine History für jeden Step mit Timestamp um ein replay im menu anzeigen zu können

    private int cellValue;
    public int CellValue { get { return cellValue; } }

    private Vector2Int cellPosition;
    public Vector2Int CellPosition
    {
        get { return cellPosition; }
        set { cellPosition = value; }
    }

    private bool isSelected;
    public bool IsSelected
    {
        get { return isSelected; }
        set { isSelected = value; }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        gameField.FloatingLens.PointerDown(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        gameField.FloatingLens.PointerUp();
    }


    public void SafeSelect()
    {
        if (isSelected)
            SelfDeselct();
        else
            Select();
    }

    public void Select()
    {
        gameField.SelectCell(this);

        isSelected = true;
        lastSelectTime = Time.time;
        cellAnimator.SetBool("Selected", true);
    }

    public void Deselect()
    {
        isSelected = false;
        cellAnimator.SetBool("Selected", false);
    }

    public void SelfDeselct()
    {
        Deselect();
        gameField.SelfDeselectedCell(this);
    }

    public void UpdateText()
    {
        string newText = CellValue == 0 ? "" : CellValue.ToString();
        foreach (TextMeshProUGUI text in texts)
            text.text = newText;
    }

    public void SetValue(int value, bool highlight = false)
    {
        cellValue = value;
        gameField.SetCellValue(CellPosition.x, CellPosition.y, value);
        if (highlight)
            gameField.HighlightValue(value);
        UnhighlightError();
        UpdateText();
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

    public CellQuarter GetQuarter()
    {
        if (cellPosition.x < 3)
        {
            if (cellPosition.y < 3)
                return CellQuarter.TOP_LEFT;
            else if (cellPosition.y < 6)
                return CellQuarter.TOP_MIDDLE;
            else
                return CellQuarter.TOP_RIGHT;
        }
        else if (cellPosition.x < 6)
        {
            if (cellPosition.y < 3)
                return CellQuarter.MIDDLE_LEFT;
            else if (cellPosition.y < 6)
                return CellQuarter.MIDDLE_MIDDLE;
            else
                return CellQuarter.MIDDLE_RIGHT;
        }
        else
        {
            if (cellPosition.y < 3)
                return CellQuarter.BOTTOM_LEFT;
            else if (cellPosition.y < 6)
                return CellQuarter.BOTTOM_MIDDLE;
            else
                return CellQuarter.BOTTOM_RIGHT;
        }
    }
}
