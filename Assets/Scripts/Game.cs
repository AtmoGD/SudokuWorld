using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Manager { get; private set; }
    [SerializeField] private GameField activeGameField;
    private Vector2 lastTouchPosition;

    private void Awake()
    {
        if (Manager == null)
            Manager = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        activeGameField?.StartNewGame();
    }

    public void SetActiveGameField(GameField gameField)
    {
        activeGameField = gameField;
    }
}
