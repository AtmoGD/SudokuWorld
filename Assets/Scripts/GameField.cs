using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    public float timeElapsed;
    public bool isSolved;
    public string difficulty;
}

public class GameField : MonoBehaviour
{
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
    // [SerializeField] private float cellspacing;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private FloatingLens floatingLens;
    public FloatingLens FloatingLens { get { return floatingLens; } }

    [Header("Debug")]
    [SerializeField] private bool isActivated = false;
    [SerializeField] private SudokuData sudoku;
    public SudokuData Sudoku { get { return sudoku; } }

    private Cell selectedCell;

    private void Awake()
    {
        sudoku = new SudokuData();
        isActivated = false;
    }

    private void Update()
    {
        if (!isActivated) return;

        UpdateTime();
    }

    private void UpdateTime()
    {
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
        sudoku.difficulty = difficulty.name;

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

    public void CloseGame()
    {
        isActivated = false;
    }

    public void TriggerLensMode()
    {
        floatingLens.LensFixed = !floatingLens.LensFixed;
    }

    private void GenerateNewSudoku()
    {
        int randomDifficulty = UnityEngine.Random.Range(difficulty.range.x, difficulty.range.y);
        sudoku.puzzle = SudokuGenerator.GeneratePuzzle(randomDifficulty);
        sudoku.solution = SudokuGenerator.GridSolved.Clone() as int[,];
        sudoku.userSolution = sudoku.puzzle.Clone() as int[,]; // Copy the puzzle to userSolution
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
                cell.GameField = this;
                cell.CellPosition = new Vector2Int(i, j);
                cell.CellValue = sudoku.puzzle[i, j];
                cell.CellType = cell.CellValue == 0 ? CellType.EMPTY : CellType.FIXED;

                field[i, j] = cell;
            }
        }
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
                    field[i, j].CellValue = 0;
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
                {
                    Cell cell = field[i, j];
                    cell.HighlightError();
                }
            }
        }
    }

    public bool IsSolved()
    {
        for (int i = 0; i < field.Length; i++)
        {
            for (int j = 0; j < field.Length; j++)
            {
                Cell cell = field[i, j];
                if (cell.CellValue != sudoku.solution[i, j])
                    return false;
            }
        }

        return true;
    }

    public void SelectCell(Cell cell)
    {
        selectedCell?.Deselect();
        selectedCell = cell;
    }

    public void SetCellValue(int posX, int posY, int value)
    {
        sudoku.userSolution[posX, posY] = value;
    }
}
