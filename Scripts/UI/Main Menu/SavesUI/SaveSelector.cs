using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the three game-slot buttons on the main menu.
/// Attach to the same GameObject that holds the SelectorManager uis[] array.
/// </summary>
public class SaveSelector : SelectorManager
{
    [Tooltip("Name of the gameplay scene to load.")]
    [SerializeField]
    private string gameSceneName = "combat demo";

    protected override void Start()
    {
        // Populate each SaveUI with its slot data before the base highlights the first.
        for (int i = 0; i < uis.Length; i++)
        {
            if (uis[i] is SaveUI saveUI)
                saveUI.InitializeUI(i);
        }

        base.Start();
    }

    public override void UIClicked(SelectionUI ui)
    {
        var saveUI = ui as SaveUI;
        if (saveUI == null) return;

        // Tell SaveManager which slot to use before we enter the game scene.
        SaveManager.ActiveSlot = saveUI.SlotIndex;

        Debug.Log($"[SaveSelector] Loading slot {saveUI.SlotIndex} → scene \"{gameSceneName}\"");
        SceneManager.LoadScene(gameSceneName);
    }
}