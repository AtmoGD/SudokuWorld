using UnityEngine;
using System;
using UnityEngine.EventSystems;
using TMPro;

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

[Serializable, RequireComponent(typeof(Animator))]
public class Cell : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameField gameField;
    [SerializeField] private TextMeshProUGUI text;

    private CellType cellType;
    public CellType CellType
    {
        get { return cellType; }
        set
        {
            cellType = value;
            text.color = cellType == CellType.FIXED ? Color.black : Color.blue;
        }
    }

    private int cellValue;
    public int CellValue
    {
        get { return cellValue; }
        set
        {
            cellValue = value;
            gameField.SetCellValue(cellPosition.x, CellPosition.y, value);
            UpdateText();
        }
    }

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

    public Animator Animator { get; private set; }

    private void Awake()
    {
        Animator = GetComponent<Animator>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        gameField.FloatingLens.PointerDown(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        gameField.FloatingLens.PointerUp();
    }

    public void Select()
    {
        if (isSelected)
        {
            Deselect();
            return;
        }

        gameField.SelectCell(this);

        isSelected = true;
        text.color = Color.green;
    }

    public void Deselect()
    {
        isSelected = false;
        text.color = cellType == CellType.FIXED ? Color.black : Color.blue;
    }

    public void UpdateText()
    {
        text.text = cellValue == 0 ? "" : cellValue.ToString();
    }

    public void HighlightError()
    {
        text.color = Color.red;
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
