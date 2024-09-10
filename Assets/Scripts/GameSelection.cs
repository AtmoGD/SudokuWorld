using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSelection : MonoBehaviour
{
    [SerializeField] private List<Animator> animators;
    private int currentSelection = 0;

    private void Start()
    {
        UpdateCurrentSelection(0);
    }

    public void UpdateCurrentSelection(int dir)
    {
        currentSelection = Mathf.Clamp(currentSelection + dir, 0, animators.Count - 1);

        for (int i = 0; i < animators.Count; i++)
            animators[i].SetBool("Active", i == currentSelection);

        Debug.Log("Current selection: " + currentSelection);
    }
}
