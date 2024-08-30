using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingInput : MonoBehaviour
{
    [SerializeField] private List<GameObject> inputElements = new List<GameObject>();
    [SerializeField] private float circleRadius = 150f;
    [SerializeField] private float stepSize = 0.4f;
    [SerializeField] private float distanceThreshold = 50f;
    [SerializeField] private float clickDurationThreshold = 0.5f;

    private Cell currentCell = null;
    private float clickStartTime;
    private int currentElementIndex = -1;

    private void Start()
    {
        HideInputElements();
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
        HideInputElements();

        if (Time.time - clickStartTime < clickDurationThreshold && currentCell.CellType != CellType.FIXED)
            currentCell.Select();

        if (currentElementIndex != -1)
            currentCell.CellValue = currentElementIndex;

        currentElementIndex = -1;
        currentCell = null;
    }

    private void ShowInputElements()
    {
        for (int i = 0; i < inputElements.Count; i++)
        {
            inputElements[i].SetActive(true);
        }
    }

    private void HideInputElements()
    {
        for (int i = 0; i < inputElements.Count; i++)
        {
            inputElements[i].SetActive(false);
        }
    }

    private void checkPointerPosition()
    {
        if (currentCell && currentCell.CellType != CellType.FIXED)
        {
            float distance = Vector2.Distance(Input.mousePosition, currentCell.transform.position);

            if (distance > distanceThreshold)
            {
                ShowInputElements();
                ScaleElement(GetNearestElement());
            }
            else
            {
                HideInputElements();
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


    private void UpdateElementPosition()
    {
        float rotationCorrection = 0;

        CellQuarter quarter = currentCell.GetQuarter();
        switch (quarter)
        {
            case CellQuarter.TOP_LEFT:
                rotationCorrection = 90;
                break;
            case CellQuarter.TOP_MIDDLE:
                rotationCorrection = 0;
                break;
            case CellQuarter.TOP_RIGHT:
                rotationCorrection = 90;
                break;
            case CellQuarter.MIDDLE_LEFT:
                rotationCorrection = 90;
                break;
            case CellQuarter.MIDDLE_MIDDLE:
                rotationCorrection = 90;
                break;
            case CellQuarter.MIDDLE_RIGHT:
                rotationCorrection = 90;
                break;
            case CellQuarter.BOTTOM_LEFT:
                rotationCorrection = 90;
                break;
            case CellQuarter.BOTTOM_MIDDLE:
                rotationCorrection = 180;
                break;
            case CellQuarter.BOTTOM_RIGHT:
                rotationCorrection = 90;
                break;
        }

        for (int i = 0; i < inputElements.Count; i++)
        {
            float angle = i * stepSize + rotationCorrection;
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * circleRadius;
            float y = Mathf.Sin(angle * Mathf.Deg2Rad) * circleRadius;

            x = currentCell.CellPosition.y > 5 ? -x : x;

            inputElements[i].transform.position = new Vector2(currentCell.transform.position.x + x, currentCell.transform.position.y + y);
        }
    }
}