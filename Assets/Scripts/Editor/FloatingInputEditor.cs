using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FloatingInput))]
public class FloatingInputEditor : Editor
{
    private FloatingInput floatingInput;

    private void OnEnable()
    {
        floatingInput = (FloatingInput)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Update Element Position"))
        {
            floatingInput.UpdateElementPosition();
        }
    }
}