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
    [SerializeField] private List<Rows> field;
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

        sudoku.difficulty = difficulty.name;

        sudoku.timeElapsed = 0;

        isActivated = true;

        SaveGame();
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

        ResetField();
    }

    public void ResetField()
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
