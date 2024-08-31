using UnityEngine;
using System;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UIElements;

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
    [SerializeField] private TextMeshProUGUI text;

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
            SetColor();
        }
    }

    // Idee: Eine History für jeden Step mit Timestamp um ein replay im menu anzeigen zu können

    private int cellValue;
    public int CellValue { get { return cellValue; } set { SetValue(value); } }

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

    private void Start()
    {
        SetColor();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetColor();
        gameField.FloatingLens.PointerDown(this);
        Debug.Log("PointerDown " + cellPosition);
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
        text.color = Game.Manager.Theme.CellSelectedTextColor;
    }

    public void Deselect()
    {
        // gameField.SelectCell(null);

        isSelected = false;
        SetColor();
    }

    public void UpdateText()
    {
        text.text = CellValue == 0 ? "" : CellValue.ToString();
    }

    public void SetValue(int value)
    {
        cellValue = value;
        gameField.SetCellValue(CellPosition.x, CellPosition.y, value);
        UpdateText();
    }

    public void SetColor()
    {
        switch (cellType)
        {
            case CellType.FIXED:
                text.color = Game.Manager.Theme.CellFixedTextColor;
                Debug.Log("Fixed " + Game.Manager.Theme.CellFixedTextColor);
                break;
            case CellType.USER:
                text.color = Game.Manager.Theme.CellUserTextColor;
                Debug.Log("User");
                break;
            case CellType.EMPTY:
                text.color = Game.Manager.Theme.CellFixedTextColor;
                Debug.Log("Empty");
                break;
        }
    }

    public void HighlightError()
    {
        if (cellType == CellType.FIXED || CellValue == 0)
            return;

        text.color = Game.Manager.Theme.CellErrorTextColor;
    }

    public void Highlight()
    {
        Animator.SetBool("Highlighted", true);
    }

    public void Unhighlight()
    {
        Animator.SetBool("Highlighted", false);
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
