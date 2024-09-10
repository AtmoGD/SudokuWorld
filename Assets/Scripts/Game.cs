using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[Serializable]
public class PlayerData
{
    public List<SudokuData> playedGames = new List<SudokuData>();

    public PlayerData()
    {
        playedGames = new List<SudokuData>();
    }
}

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

    [SerializeField] private string saveFileName = "game.sudoku";
    [SerializeField] private Menu menu;
    public Menu Menu { get { return menu; } }
    [SerializeField] private Theme theme;
    public Theme Theme { get { return theme; } }
    [SerializeField] private GameObject gameFieldParent;

    private PlayerData playerData;
    private GameField activeGameField;

    private void Awake()
    {
        if (Manager == null)
            manager = this;
        else if (Manager != this)
            Destroy(gameObject);

        LoadData();
    }

    private void LoadData()
    {
        playerData = DataManager.LoadData<PlayerData>(saveFileName) ?? new PlayerData();
    }

    public void SaveGame(SudokuData sudokuData)
    {
        SudokuData sudoku = playerData.playedGames.Find(sudoku => sudoku.uid == sudokuData.uid);
        if (sudoku != null)
            playerData.playedGames.Remove(sudoku);

        playerData.playedGames.Add(sudokuData);

        DataManager.SaveData(playerData, saveFileName);
    }

    public void DeletePlayerData()
    {
        DataManager.DeleteData(saveFileName);
    }

    public void StartNewBaseGame(DifficultySetting settings)
    {
        InstantiateNewGameField(settings.gameFieldPrefab);
        activeGameField.StartNewGame(settings);
    }

    public void ResumeLastOpenGameWithDifficulty(DifficultySetting settings)
    {
        SudokuData sudokuData = GetLastOpenGame(settings);
        if (sudokuData != null)
        {
            InstantiateNewGameField(settings.gameFieldPrefab);
            activeGameField.ResumeGame(sudokuData);
        }
        else
        {
            Debug.Log("CAN'T FIND LAST OPEN SUDOKU!");
        }
    }

    public SudokuData GetLastOpenGame(DifficultySetting settings)
    {
        return playerData.playedGames.Find(sudoku => sudoku.difficulty == settings.displayName && sudoku.lastOpened);
    }

    public void InstantiateNewGameField(GameObject prefab)
    {
        ClearGameFieldParent();
        GameObject gameFieldObject = Instantiate(prefab, gameFieldParent.transform);
        activeGameField = gameFieldObject.GetComponent<GameField>();
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

    public void SetLastOpenedFor(string uid)
    {
        playerData.playedGames.ForEach(data => { data.lastOpened = data.uid == uid; });
    }
}
