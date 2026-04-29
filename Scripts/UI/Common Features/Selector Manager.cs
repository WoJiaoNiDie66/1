using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class SelectorManager : MonoBehaviour
{
    [SerializeField]
    protected SelectionUI[] uis;

    [SerializeField]
    protected int columns=1;

    protected int currentIndex = 0;

    protected SelectionUI currentUI;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (uis.Length < 2)
        {
            Debug.LogError("Menu has only one option");
            return;
        }
        for (int i = 1; i < uis.Length; i++)
        {
            uis[i].UnHighlight();
        }
        currentUI = uis[currentIndex];
        currentUI.Highlight();
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentUI.UnHighlight();
            if (currentIndex == 0)
            {
                currentIndex = uis.Length - 1;
            }
            else
            {
                currentIndex--;
            }
            UIHover();
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentUI.UnHighlight();
            if (currentIndex == uis.Length - 1)
            {
                currentIndex = 0;
            }
            else
            {
                currentIndex++;
            }
            UIHover();

        }
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentUI.UnHighlight();
            if (currentIndex < columns)
            {
                currentIndex = (uis.Length - columns + currentIndex );
            }
            else
            {
                currentIndex = currentIndex - columns;
            }
            UIHover();

        }
        else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentUI.UnHighlight();
            if (currentIndex+columns >= uis.Length)
            {
                currentIndex = currentIndex+columns - uis.Length;
            }
            else
            {
                currentIndex = currentIndex + columns;
            }

            UIHover();

        }
    }

    public virtual void UIHover()
    {
        currentUI = uis[currentIndex];
        currentUI.Highlight();
    }

    public virtual void UIHover(SelectionUI UI)
    {
        if (currentUI != null)
        {
            currentUI.UnHighlight();
        }
        currentUI = UI;
        currentUI.Highlight();
        currentIndex = Array.IndexOf(uis, UI);
    }

    public abstract void UIClicked(SelectionUI ui);
}
