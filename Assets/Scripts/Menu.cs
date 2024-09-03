using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void SetMenuOpen(bool open)
    {
        Debug.Log("SetMenuOpen");
        animator.SetBool("MenuOpen", open);
    }

    public void SetStartScreenOpen(bool open)
    {
        animator.SetBool("StartScreenOpen", open);
    }

    public void SetGameSelectionOpen(bool open)
    {
        animator.SetBool("GameSelectionOpen", open);
    }
}
