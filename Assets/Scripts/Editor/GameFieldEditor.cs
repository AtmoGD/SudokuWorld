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
            gameField.StartNewGame();
        }

        if (GUILayout.Button("Reset Field"))
        {
            gameField.ResetField();
        }
    }
}