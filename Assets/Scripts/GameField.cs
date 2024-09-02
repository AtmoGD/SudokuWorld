using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

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
        if (selectedCell == cell) return;

        if (selectedCell != null)
        {
            selectedCell.Deselect();
            FixedInput.OverrideValue(0);
            ClearHighlightBackground();
        }

        selectedCell = cell;

        if (selectedCell != null)
        {
            selectedCell.Select();

            if (selectedCell.CellType != CellType.FIXED)
            {
                HighlightRowAndColumn(selectedCell.CellPosition.x, selectedCell.CellPosition.y);
                HighlightSubGrid(selectedCell.CellPosition.x, selectedCell.CellPosition.y);
            }
            FixedInput.OverrideValue(selectedCell.CellValue);
            HighlightValue(selectedCell.CellValue);
        }
    }

    public void DeselectCell()
    {
        if (selectedCell == null) return;

        selectedCell.Deselect();
        selectedCell = null;
        FixedInput.OverrideValue(0);
        ClearHighlightBackground();
        ClearHighlightValue();
    }

    public void SetCellValue(Cell cell, int value)
    {
        cell.SetType(CellType.USER);
        SetCellValue(cell.CellPosition.x, cell.CellPosition.y, value, true);
    }

    public void SetCellValue(int posX, int posY, int value, bool userInput)
    {
        sudoku.userSolution[posX, posY] = value;

        Cell cell = field[posX, posY];
        cell.SetValue(value);

        if (userInput)
        {
            AddCommand(new FieldCommand { posX = posX, posY = posY, value = value, time = sudoku.timeElapsed });
            HighlightValue(value);
            FixedInput.SelectValue(value);
        }

        OnSetCellValue?.Invoke();
    }

    public void HighlightRowAndColumn(int x, int y)
    {
        for (int i = 0; i < fieldSize; i++)
        {
            field[x, i].HighlightBackground();
            field[i, y].HighlightBackground();
        }
    }

    public void HighlightSubGrid(int x, int y)
    {
        int startX = x - x % subGridSize;
        int startY = y - y % subGridSize;

        for (int i = 0; i < subGridSize; i++)
        {
            for (int j = 0; j < subGridSize; j++)
                field[startX + i, startY + j].HighlightBackground();
        }
    }

    public void HighlightValue(int value)
    {
        ClearHighlightValue();

        for (int i = 0; i < fieldSize; i++)
        {
            for (int j = 0; j < fieldSize; j++)
            {
                if (field[i, j].CellValue == value)
                    field[i, j].HighlightValue();
            }
        }
    }

    public void ClearHighlightValue()
    {
        foreach (Cell c in field)
            c.UnhighlightValue();
    }

    void ClearHighlightBackground()
    {
        foreach (Cell c in field)
            c.UnhighlightBackground();
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

}
