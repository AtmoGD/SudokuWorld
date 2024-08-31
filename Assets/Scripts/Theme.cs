using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Theme", menuName = "Theme", order = 1)]
public class Theme : ScriptableObject
{
    [SerializeField] private Color cellUserTextColor;
    public Color CellUserTextColor { get { return cellUserTextColor; } }

    [SerializeField] private Color cellSelectedTextColor;
    public Color CellSelectedTextColor { get { return cellSelectedTextColor; } }

    [SerializeField] private Color cellHighlightedTextColor;
    public Color CellHighlightedTextColor { get { return cellHighlightedTextColor; } }

    [SerializeField] private Color cellFixedTextColor;
    public Color CellFixedTextColor { get { return cellFixedTextColor; } }

    [SerializeField] private Color cellErrorTextColor;
    public Color CellErrorTextColor { get { return cellErrorTextColor; } }
}
