using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Game : MonoBehaviour
{
    private static Game manager;
    public static Game Manager
    {
        get
        {
            if (manager == null)
                manager = FindObjectOfType<Game>();

            return manager;
        }
    }
    [SerializeField] private GameField activeGameField;
    [SerializeField] private Theme theme;
    public Theme Theme { get { return theme; } }

    private void Awake()
    {
        if (Manager == null)
            manager = this;
        else if (Manager != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        StartNewGame();
    }

    public void StartNewGame()
    {
        if (activeGameField != null)
            activeGameField.StartNewGame();
    }

    public void SetActiveGameField(GameField gameField)
    {
        activeGameField = gameField;
    }
}
