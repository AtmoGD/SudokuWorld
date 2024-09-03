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
    [SerializeField] private Menu menu;
    public Menu Menu { get { return menu; } }
    [SerializeField] private Theme theme;
    [SerializeField] private GameObject gameFieldParent;
    public Theme Theme { get { return theme; } }

    private void Awake()
    {
        if (Manager == null)
            manager = this;
        else if (Manager != this)
            Destroy(gameObject);
    }

    public void StartBaseGame(DifficultySetting settings)
    {
        ClearGameFieldParent();
        GameObject gameFieldObject = Instantiate(settings.gameFieldPrefab, gameFieldParent.transform);
        activeGameField = gameFieldObject.GetComponent<GameField>();
        activeGameField.StartNewGame(settings);
    }

    public void ClearGameFieldParent()
    {
        foreach (Transform child in gameFieldParent.transform)
#if UNITY_EDITOR
            DestroyImmediate(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
    }

    public void SetActiveGameField(GameField gameField)
    {
        activeGameField = gameField;
    }
}
