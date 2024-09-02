using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameField))]
public class GameFieldEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GameField gameField = (GameField)target;

        if (GUILayout.Button("Generate Field"))
        {
            gameField.SetIsActivated(false);
            gameField.StartNewGame();
            // SudokuGenerator.PrintGrid(gameField.Sudoku.puzzle);
        }

        // Draw the current sudoku solution with seperation
        if (gameField != null && gameField.Sudoku != null && gameField.Sudoku.solution != null)
        {
            GUILayout.Label("Current Sudoku Solution:");
            for (int i = 0; i < gameField.Sudoku.solution.GetLength(0); i++)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < gameField.Sudoku.solution.GetLength(1); j++)
                {
                    GUILayout.Label(gameField.Sudoku.solution[i, j].ToString());
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(20);

            GUILayout.Label("Current User Solution:");
            for (int i = 0; i < gameField.Sudoku.userSolution.GetLength(0); i++)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < gameField.Sudoku.userSolution.GetLength(1); j++)
                {
                    GUILayout.Label(gameField.Sudoku.userSolution[i, j].ToString());
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}