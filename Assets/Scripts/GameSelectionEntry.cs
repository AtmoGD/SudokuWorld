using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameSelectionEntry : MonoBehaviour
{
    [SerializeField] private Menu menu;
    [SerializeField] private TextMeshProUGUI headlineText;
    [SerializeField] private GameObject loadButton;
    [SerializeField] private DifficultySetting difficultySetting;

    public void StartNewGame()
    {
        Game.Manager.StartNewBaseGame(difficultySetting);
        menu.SetMenuOpen(false);
    }

    public void LoadGame()
    {
        Game.Manager.ResumeLastOpenGameWithDifficulty(difficultySetting);
        menu.SetMenuOpen(false);
    }

    private void Start()
    {
        headlineText.text = difficultySetting.displayName;

        UpdateLoadButton();
    }

    public void UpdateLoadButton()
    {
        loadButton.SetActive(Game.Manager.GetLastOpenGame(difficultySetting) != null);
    }
}
