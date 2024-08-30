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
    [SerializeField] private int fieldSize;
    [SerializeField] private int subGridSize;
    [SerializeField] private List<Rows> field;
    [SerializeField] private float subGridSpacing;
    [SerializeField] private float cellSize;
    [SerializeField] private float cellspacing;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private FloatingLens floatingLens;
    public FloatingLens FloatingLens { get { return floatingLens; } }

    [Header("Debug")]
    [SerializeField] private bool isActivated = false;
    [SerializeField] private SudokuData sudoku;

    private Cell selectedCell;

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

        ResetToDefaultValues();

        ResetState();

        SaveGame();
    }

    public void ResetState()
    {
        sudoku.difficulty = difficulty.name;

        sudoku.timeElapsed = 0;

        isActivated = false;
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
        sudoku.solution = SudokuGenerator.GeneratePuzzle();
        sudoku.puzzle = SudokuGenerator.GenerateSolution(randomDifficulty);
        sudoku.userSolution = sudoku.puzzle.Clone() as int[,];
    }

    private void GenerateField()
    {
        DeleteCells();

        field = new List<Rows>();

        for (int i = 0; i < fieldSize; i++)
        {
            Rows row = new Rows();
            row.cells = new List<Cell>();

            for (int j = 0; j < fieldSize; j++)
            {
                GameObject cellObject = Instantiate(cellPrefab, cellParent);
                cellObject.transform.localPosition = CalculateCellPosition(i, j);

                Cell cell = cellObject.GetComponent<Cell>();
                cell.GameField = this;
                cell.CellPosition = new Vector2Int(i, j);
                cell.CellValue = 0;
                cell.CellType = CellType.EMPTY;

                row.cells.Add(cell);
            }

            field.Add(row);
        }
    }

    private Vector2 CalculateCellPosition(int i, int j)
    {
        return new Vector2(CalculatePosition(i), CalculatePosition(j));
    }

    private float CalculatePosition(int i)
    {
        float position = i * cellSize + i * cellspacing;
        position += Mathf.Floor(i / subGridSize) * subGridSpacing;
        position -= (fieldSize * cellSize + fieldSize * cellspacing) / 2;

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

        for (int i = 0; i < field.Count; i++)
        {
            for (int j = 0; j < field[i].cells.Count; j++)
            {
                Cell cell = field[i].cells[j];
                cell.CellValue = sudoku.puzzle[i, j];
                cell.CellType = cell.CellValue == 0 ? CellType.EMPTY : CellType.FIXED;
                cell.CellPosition = new Vector2Int(i, j);
            }
        }
    }

    public void ValidateField()
    {
        for (int i = 0; i < field.Count; i++)
        {
            for (int j = 0; j < field[i].cells.Count; j++)
            {
                Cell cell = field[i].cells[j];
                if (cell.CellValue != sudoku.solution[i, j])
                    cell.HighlightError();
            }
        }
    }

    public bool IsSolved()
    {
        for (int i = 0; i < field.Count; i++)
        {
            for (int j = 0; j < field[i].cells.Count; j++)
            {
                Cell cell = field[i].cells[j];
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
