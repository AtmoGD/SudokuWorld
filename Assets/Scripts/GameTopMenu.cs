using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTopMenu : MonoBehaviour
{
    [SerializeField] private Animator settingsPopup;
    [SerializeField] private bool isSettingsOpen = false;

    public void ToggleSettings()
    {
        isSettingsOpen = !isSettingsOpen;
        settingsPopup.SetBool("isOpen", isSettingsOpen);
    }
}
