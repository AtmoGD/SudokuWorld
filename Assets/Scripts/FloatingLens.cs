using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FloatingLens : MonoBehaviour
{
    [SerializeField] private List<GameObject> inputElements = new List<GameObject>();
    [SerializeField] private Transform holder;
    [SerializeField] private Transform lens;
    [SerializeField] private TextMeshProUGUI lensText;
    [SerializeField] private Animator animator;
    [SerializeField] private float circleRadius = 150f;
    [SerializeField] private float stepSize = 0.4f;
    [SerializeField] private float distanceThreshold = 50f;
    [SerializeField] private float clickDurationThreshold = 0.5f;

    private Cell currentCell = null;
    private float clickStartTime;
    private int currentElementIndex = -1;

    private void Start()
    {
        // HideInputElements();
        animator.SetBool("Show", false);
    }

    private void Update()
    {
        if (!currentCell) return;

        checkPointerPosition();
    }

    public void PointerDown(Cell cell)
    {
        currentCell = cell;
        transform.position = currentCell.transform.position;
        clickStartTime = Time.time;
        UpdateElementPosition();

    }

    public void PointerUp()
    {
        // HideInputElements();

        if (Time.time - clickStartTime < clickDurationThreshold && currentCell.CellType != CellType.FIXED)
            currentCell.Select();

        if (currentElementIndex != -1)
            currentCell.CellValue = currentElementIndex;

        currentElementIndex = -1;
        currentCell = null;

        animator.SetBool("Show", false);
    }

    // private void ShowInputElements()
    // {
    //     for (int i = 0; i < inputElements.Count; i++)
    //     {
    //         inputElements[i].SetActive(true);
    //     }
    // }

    // private void HideInputElements()
    // {
    //     for (int i = 0; i < inputElements.Count; i++)
    //     {
    //         inputElements[i].SetActive(false);
    //     }
    // }

    private void checkPointerPosition()
    {
        if (currentCell && currentCell.CellType != CellType.FIXED)
        {
            float distance = Vector2.Distance(Input.mousePosition, currentCell.transform.position);

            if (distance > distanceThreshold)
            {
                animator.SetBool("Show", true);
                // ShowInputElements();
                ScaleElement(GetNearestElement());
                UpdateLens();
            }
            else
            {
                // HideInputElements();
                animator.SetBool("Show", false);
                currentElementIndex = -1;
            }
        }
    }

    private void ScaleElement(GameObject nearestElement)
    {
        inputElements.ForEach(element =>
        {
            if (element == nearestElement)
            {
                element.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                currentElementIndex = inputElements.IndexOf(element);
            }
            else
            {
                element.transform.localScale = new Vector3(1f, 1f, 1f);
            }
        });
    }

    private GameObject GetNearestElement()
    {
        GameObject nearestElement = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < inputElements.Count; i++)
        {
            float elementDistance = Vector2.Distance(Input.mousePosition, inputElements[i].transform.position);

            if (elementDistance < nearestDistance)
            {
                nearestElement = inputElements[i];
                nearestDistance = elementDistance;
            }
        }

        return nearestElement;
    }

    private void UpdateLens()
    {
        if (currentElementIndex == -1)
        {
            lens.localScale = new Vector3(0, 0, 0);
        }
        else
        {
            lensText.text = currentElementIndex == 0 ? "X" : currentElementIndex.ToString();
            lens.position = inputElements[currentElementIndex].transform.position;

        }
    }

    private void UpdateElementPosition()
    {
        // the holder should be rotated so its pointing to the center of the field (cell[4,4])
        Vector2 startPostion = new Vector2(4, 4);
        Vector2 result = startPostion - currentCell.CellPosition;
        float rotationCorrection = Vector2.SignedAngle(Vector2.up, result);
        //Add 45 to correct the rotation of the holder
        rotationCorrection -= 45;
        holder.rotation = Quaternion.Euler(0, 0, rotationCorrection);

        // PLace the elements based on the result vector
        for (int i = 0; i < inputElements.Count; i++)
        {
            float angle = i * -stepSize + 90 + rotationCorrection;
            float x = circleRadius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = circleRadius * Mathf.Sin(angle * Mathf.Deg2Rad);
            inputElements[i].transform.localPosition = new Vector2(x, y);
        }
    }
}