using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Generated with Perplexity
public static class SudokuGenerator
{
    private const int GridSize = 9;
    private const int SubGridSize = 3;
    private static int[,] gridSolved;
    public static int[,] GridSolved { get { return gridSolved; } }
    private static int[,] puzzle;
    public static int[,] Puzzle { get { return puzzle; } }
    private static System.Random random = new System.Random();

    public static int[,] GeneratePuzzle(int difficulty)
    {
        Debug.Log("Generating puzzle with difficulty: " + difficulty);
        gridSolved = new int[GridSize, GridSize];
        puzzle = new int[GridSize, GridSize];
        FillGrid();
        PrintGrid(gridSolved);
        GenerateSolution(difficulty);
        PrintGrid(puzzle);
        return puzzle;
    }

    public static int[,] GenerateSolution(int difficulty)
    {
        puzzle = gridSolved.Clone() as int[,];
        RemoveNumbers(difficulty);
        return puzzle;
    }

    private static void FillGrid()
    {
        FillCell(0, 0);
    }

    private static bool FillCell(int row, int col)
    {
        if (col >= GridSize)
        {
            row++;
            col = 0;
        }

        if (row >= GridSize)
            return true;

        var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        Shuffle(numbers);

        foreach (int num in numbers)
        {
            if (IsValid(row, col, num))
            {
                gridSolved[row, col] = num;

                if (FillCell(row, col + 1))
                    return true;

                gridSolved[row, col] = 0;
            }
        }

        return false;
    }

    private static bool IsValid(int row, int col, int num)
    {
        // Check row
        for (int i = 0; i < GridSize; i++)
            if (gridSolved[row, i] == num)
                return false;

        // Check column
        for (int i = 0; i < GridSize; i++)
            if (gridSolved[i, col] == num)
                return false;

        // Check sub-grid
        int startRow = row - row % SubGridSize;
        int startCol = col - col % SubGridSize;
        for (int i = 0; i < SubGridSize; i++)
            for (int j = 0; j < SubGridSize; j++)
                if (gridSolved[i + startRow, j + startCol] == num)
                    return false;

        return true;
    }

    private static void RemoveNumbers(int difficulty)
    {
        int numbersToRemove = GridSize * GridSize - difficulty;
        while (numbersToRemove > 0)
        {
            int row = random.Next(GridSize);
            int col = random.Next(GridSize);
            if (puzzle[row, col] != 0)
            {
                puzzle[row, col] = 0;
                numbersToRemove--;
            }
        }
    }

    private static void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void PrintGrid(int[,] sudokuGrid)
    {
        Debug.Log("Sudoku Grid:");
        for (int i = 0; i < GridSize; i++)
        {
            if (i % SubGridSize == 0 && i != 0)
                Debug.Log("---------------------");

            string logTxt = "";

            for (int j = 0; j < GridSize; j++)
            {
                if (j % SubGridSize == 0 && j != 0)
                    logTxt += "| ";

                if (sudokuGrid[i, j] == 0)
                    logTxt += ". ";
                else
                    logTxt += sudokuGrid[i, j] + " ";
            }
            Debug.Log(logTxt);
        }
    }
}