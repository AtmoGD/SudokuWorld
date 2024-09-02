using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using JetBrains.Annotations;
using System.Linq;

[Serializable]
public class Rows
{
    public List<Cell> cells;
}


[Serializable]
public class SudokuData
{
    public int[,] puzzle;
    public int[,] solution;
    public int[,] userSolution;
    public List<FieldCommand> commands;
    public float timeElapsed;
    public bool isSolved;
    public string difficulty;

    public SudokuData()
    {
        puzzle = new int[9, 9];
        solution = new int[9, 9];
        userSolution = new int[9, 9];
        commands = new List<FieldCommand>();
        timeElapsed = 0;
        isSolved = false;
        difficulty = "";
    }
}

public enum InputType
{
    FLOATING,
    FIXED
}


[Serializable]
public class HighlightData
{
    public List<Cell> cells;
    public InputType inputType;
}

[Serializable]
public class FieldCommand
{
    public int posX;
    public int posY;
    public float time;
    public int value;
}

public class GameField : MonoBehaviour
{
    public UnityAction OnSetCellValue;

    [Header("Settings")]
    [SerializeField] private DifficultySetting difficulty;
    [SerializeField] private string saveFileName;

    [Header("References")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform cellParent;
    [SerializeField] private float fieldWidth;
    [SerializeField] private int fieldSize;
    [SerializeField] private int subGridSize;
    [SerializeField] private Cell[,] field;
    [SerializeField] private float subGridSpacing;
    [SerializeField] private float borderSpacing;
    [SerializeField] private float cellSize;
    [SerializeField] private bool highlightRowsAndColumns;
    [SerializeField] private bool highlightSubGrids;
    [SerializeField] private bool highlightValues;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI headlineText;
    [SerializeField] private FloatingInput floatingInput;
    public FloatingInput FloatingInput { get { return floatingInput; } }
    [SerializeField] private FixedInput fixedInput;
    public FixedInput FixedInput { get { return fixedInput; } }

    [Header("Debug")]
    [SerializeField] private bool isActivated = false;
    [SerializeField] private SudokuData sudoku;
    public SudokuData Sudoku { get { return sudoku; } }
    [SerializeField] private List<HighlightData> highlightDatas = new List<HighlightData>();
    private Cell selectedCell;
    public Cell SelectedCell { get { return selectedCell; } }


    private void Awake()
    {
        sudoku = new SudokuData();
        isActivated = false;
        headlineText.text = difficulty.displayName;
    }

    private void Update()
    {
        if (!isActivated) return;

        CheckInputs();

        UpdateTime();
    }

    private void UpdateTime()
    {
        if (sudoku.isSolved) return;

        sudoku.timeElapsed += Time.deltaTime;

        timerText.text = TimeSpan.FromSeconds(sudoku.timeElapsed).ToString("mm':'ss");
    }

    public void StartNewGame()
    {
        if (isActivated) return;

        // UpdateHighlights();

        GenerateNewSudoku();

        GenerateField();

        ResetState();

        SaveGame();
    }

    public void ResetState()
    {
        sudoku.difficulty = difficulty.displayName;

        sudoku.timeElapsed = 0;

        SetIsActivated(true);
    }

    public void SetIsActivated(bool value)
    {
        isActivated = value;
    }

    public void ResumeGame()
    {
        // Load saved puzzle
    }

    public void SaveGame()
    {
        // Save current puzzle
    }

    public void AddCommand(FieldCommand command)
    {
        sudoku.commands.Add(command);
        SaveGame();
    }

    public void CloseGame()
    {
        isActivated = false;
    }

    public void TriggerLensMode()
    {
        floatingInput.LensFixed = !floatingInput.LensFixed;
    }

    public void TriggerHighlightRowsAndColumns()
    {
        highlightRowsAndColumns = !highlightRowsAndColumns;
    }

    public void TriggerHighlightSubGrids()
    {
        highlightSubGrids = !highlightSubGrids;
    }

    public void TriggerHighlightValues()
    {
        highlightValues = !highlightValues;
    }

    private void GenerateNewSudoku()
    {
        int randomDifficulty = UnityEngine.Random.Range(difficulty.range.x, difficulty.range.y);
        SudokuGenerator.GeneratePuzzle(randomDifficulty);
        sudoku.puzzle = SudokuGenerator.Puzzle.Clone() as int[,];
        sudoku.solution = SudokuGenerator.GridSolved.Clone() as int[,];
        sudoku.userSolution = SudokuGenerator.Puzzle.Clone() as int[,]; // Copy the puzzle to userSolution
    }

    private void GenerateField()
    {
        DeleteCells();

        CalculateCellSize();

        field = new Cell[fieldSize, fieldSize];

        for (int i = 0; i < fieldSize; i++)
        {
            for (int j = 0; j < fieldSize; j++)
            {
                GameObject cellObject = Instantiate(cellPrefab, cellParent);
                cellObject.transform.localPosition = CalculateCellPosition(i, j);

                Cell cell = cellObject.GetComponent<Cell>();
                cell.SetGameField(this);
                cell.SetPosition(new Vector2Int(i, j));
                cell.SetValue(sudoku.puzzle[i, j]);
                cell.SetType(cell.CellValue == 0 ? CellType.EMPTY : CellType.FIXED);

                field[i, j] = cell;
            }
        }

        OnSetCellValue?.Invoke(); // Just need to call this one time after the field is generated
    }

    public void CalculateCellSize()
    {
        cellSize = (fieldWidth - (2 * borderSpacing) - (((fieldSize / subGridSize) - 1) * subGridSpacing)) / fieldSize;
    }

    private Vector2 CalculateCellPosition(int i, int j)
    {
        return new Vector2(CalculatePosition(j), 8 - CalculatePosition(i)); // THIS IS FUCKED UP!
    }

    private float CalculatePosition(int i)
    {
        float position = borderSpacing + i * cellSize;
        position += Mathf.Floor(i / subGridSize) * subGridSpacing;
        position -= fieldWidth / 2;
        position += cellSize / 2;
        return position;
    }

    public void DeleteCells()
    {
        for (int i = cellParent.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            DestroyImmediate(cellParent.GetChild(i).gameObject);
#else
            Destroy(cellParent.GetChild(i).gameObject);
#endif
        }

        field = null;
    }

    public void ResetToDefaultValues()
    {
        if (field == null) return;

        for (int i = 0; i < fieldSize; i++)
        {
            for (int j = 0; j < fieldSize; j++)
            {
                if (field[i, j].CellType != CellType.FIXED)
                    field[i, j].SetValue(0);
            }
        }
    }

    public void ValidateField()
    {
        for (int i = 0; i < fieldSize; i++)
        {
            for (int j = 0; j < fieldSize; j++)
            {
                if (sudoku.userSolution[i, j] != sudoku.solution[i, j])
                    field[i, j].HighlightError();
            }
        }
    }

    public bool IsSolved()
    {
        for (int i = 0; i < field.Length; i++)
        {
            for (int j = 0; j < field.Length; j++)
            {
                try
                {
                    if (sudoku.userSolution[i, j] != sudoku.solution[i, j])
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("i: " + i + " j: " + j);
                    Debug.Log(e.Message);
                }
            }
        }

        return true;
    }

    public void CheckIfSolved()
    {
        if (IsSolved())
            sudoku.isSolved = true;
    }

    public void SelectCell(Cell cell)
    {
        selectedCell = cell;

        UpdateHighlights();
    }

    public void DeselectCell()
    {
        if (selectedCell == null) return;

        selectedCell.Deselect();

        selectedCell = null;

        UpdateHighlights();
    }

    public void SetCellValue(Cell cell, int value)
    {
        cell.SetType(CellType.USER);
        SetCellValue(cell.CellPosition.x, cell.CellPosition.y, value, true);
        UpdateHighlights();
    }

    public void SetCellValue(int posX, int posY, int value, bool userInput)
    {
        sudoku.userSolution[posX, posY] = value;

        Cell cell = field[posX, posY];
        cell.SetValue(value);

        if (userInput)
            AddCommand(new FieldCommand { posX = posX, posY = posY, value = value, time = sudoku.timeElapsed });

        OnSetCellValue?.Invoke();
    }


    public void UpdateHighlights()
    {

        foreach (Cell cell in field)
        {
            cell.Deselect();
            cell.UnhighlightBackground();
            cell.UnsoftHighlightBackground();

            if (cell.CellValue == 0)
                cell.UnhighlightError();
        }

        if (selectedCell != null)
        {
            selectedCell.Select();

            if (highlightRowsAndColumns)
            {
                List<Cell> cells = GetRowAndColumnCells(selectedCell.CellPosition.x, selectedCell.CellPosition.y);
                cells.ForEach(x => x.HighlightBackground());
            }

            if (highlightSubGrids)
            {
                List<Cell> cells = GetSubgridCells(selectedCell.CellPosition.x, selectedCell.CellPosition.y);
                cells.ForEach(x => x.HighlightBackground());
            }
        }

        if (highlightValues)
        {
            if (fixedInput.SelectedValue != 0)
            {
                List<Cell> cells = GetValueCells(fixedInput.SelectedValue);
                cells.ForEach(x => x.SoftHighlightBackground());
            }
        }
    }

    public List<Cell> GetRowAndColumnCells(int x, int y)
    {
        List<Cell> cells = new List<Cell>();
        for (int i = 0; i < fieldSize; i++)
        {
            cells.Add(field[x, i]);
            cells.Add(field[i, y]);
        }
        return cells;
    }

    public List<Cell> GetSubgridCells(int x, int y)
    {
        List<Cell> cells = new List<Cell>();
        int startX = x - x % subGridSize;
        int startY = y - y % subGridSize;

        for (int i = 0; i < subGridSize; i++)
        {
            for (int j = 0; j < subGridSize; j++)
                cells.Add(field[startX + i, startY + j]);
        }

        return cells;
    }

    public List<Cell> GetValueCells(int value)
    {
        List<Cell> cells = new List<Cell>();

        for (int i = 0; i < fieldSize; i++)
        {
            for (int j = 0; j < fieldSize; j++)
            {
                if (field[i, j].CellValue == value)
                    cells.Add(field[i, j]);
            }
        }

        return cells;
    }


    public int GetValueAmount(int value)
    {
        int amount = 0;
        for (int i = 0; i < fieldSize; i++)
        {
            for (int j = 0; j < fieldSize; j++)
            {
                if (sudoku.userSolution[i, j] == value)
                    amount++;
            }
        }
        return amount;
    }

    private void CheckInputs()
    {
        if (Input.anyKeyDown)
        {
            switch (Input.inputString)
            {
                case "1":
                    SetCellValue(selectedCell, 1);
                    break;
                case "2":
                    SetCellValue(selectedCell, 2);
                    break;
                case "3":
                    SetCellValue(selectedCell, 3);
                    break;
                case "4":
                    SetCellValue(selectedCell, 4);
                    break;
                case "5":
                    SetCellValue(selectedCell, 5);
                    break;
                case "6":
                    SetCellValue(selectedCell, 6);
                    break;
                case "7":
                    SetCellValue(selectedCell, 7);
                    break;
                case "8":
                    SetCellValue(selectedCell, 8);
                    break;
                case "9":
                    SetCellValue(selectedCell, 9);
                    break;
                case "0":
                    SetCellValue(selectedCell, 0);
                    break;
            }
        }
    }

}
