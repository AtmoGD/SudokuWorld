using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FloatingInput : MonoBehaviour
{
    [SerializeField] private List<GameObject> inputElements = new List<GameObject>();
    [SerializeField] private Transform holder;
    [SerializeField] private Transform lens;
    [SerializeField] private TextMeshProUGUI lensText;
    [field: SerializeField] public bool LensFixed { get; set; }

    [SerializeField] private Animator animator;
    [SerializeField] private float circleRadius = 150f;
    [SerializeField] private float stepSize = 0.4f;
    [SerializeField] private float distanceThreshold = 50f;
    [SerializeField] private float clickDurationThreshold = 0.5f;

    private Cell currentCell = null;
    private Vector2 inputDirection;
    private float clickStartTime;
    private int currentElementIndex = -1;

    private void Start()
    {
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
    }

    public void PointerUp()
    {
        if (Time.time - clickStartTime < clickDurationThreshold)
            currentCell.Select();

        if (currentElementIndex != -1)
        {
            currentCell.CellType = CellType.USER;
            currentCell.CellValue = currentElementIndex;
        }

        currentElementIndex = -1;
        currentCell = null;

        animator.SetBool("Show", false);
    }

    private void checkPointerPosition()
    {
        if (currentCell && currentCell.CellType != CellType.FIXED)
        {
            float distance = Vector2.Distance(Input.mousePosition, currentCell.transform.position);

            if (distance > distanceThreshold)
            {
                animator.SetBool("Show", true);

                if (inputDirection == Vector2.zero)
                {
                    inputDirection = (Vector2)Input.mousePosition - (Vector2)currentCell.transform.position;
                    UpdateElementPosition();
                }

                GetNearestElement();
                UpdateLens();
            }
            else
            {
                animator.SetBool("Show", false);
                inputDirection = Vector2.zero;
                currentElementIndex = -1;
            }
        }
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
                currentElementIndex = i;
            }
        }

        return nearestElement;
    }

    private void UpdateLens()
    {
        if (currentElementIndex == -1) return;

        lensText.text = currentElementIndex == 0 ? "X" : currentElementIndex.ToString();

        if (!LensFixed)
            lens.position = inputElements[currentElementIndex].transform.position;

    }

    public void UpdateElementPosition()
    {
        Vector2 result;
        if (currentCell)
            result = inputDirection == Vector2.zero ? Vector2.zero : inputDirection.normalized;
        else
            result = new Vector2(4, 4) - new Vector2(8, 0);

        // Vector2 startPostion = new Vector2(4, 4);

        // Vector2 result = startPostion - (currentCell ? currentCell.CellPosition : new Vector2(8, 0));

        float rotationCorrection = Vector2.SignedAngle(Vector2.up, result);
        rotationCorrection += 45;
        holder.rotation = Quaternion.Euler(0, 0, rotationCorrection);

        for (int i = 0; i < inputElements.Count; i++)
        {
            float angle = i * -stepSize + 90 + rotationCorrection;
            inputElements[i].transform.localPosition = GetPositionFromAngle(angle, circleRadius);
        }

        if (LensFixed)
        {
            float angle = rotationCorrection + 45;
            lens.localPosition = GetPositionFromAngle(angle, circleRadius);
        }
    }

    private Vector2 GetPositionFromAngle(float angle, float radius)
    {
        float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
        float y = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
        return new Vector2(x, y);
    }
}