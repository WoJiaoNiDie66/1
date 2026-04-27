// Assets/Scripts/UI/CheckpointPanel.cs
using UnityEngine;
using System.Collections.Generic;

public class CheckpointPanel : MonoBehaviour
{
    public static CheckpointPanel Instance { get; private set; }

    [Header("Panel Style")]
    [SerializeField] private int panelWidth = 320;
    [SerializeField] private int panelHeight = 400;
    [SerializeField] private int buttonHeight = 50;
    [SerializeField] private Color panelColor = new Color(0.05f, 0.05f, 0.1f, 0.92f);
    [SerializeField] private Color buttonColor = new Color(0.2f, 0.4f, 0.8f, 1f);
    [SerializeField] private Color buttonHoverColor = new Color(0.3f, 0.55f, 1f, 1f);
    [SerializeField] private Color closeButtonColor = new Color(0.7f, 0.15f, 0.15f, 1f);

    private bool isOpen = false;
    public bool IsOpen => isOpen; // Added property so Checkpoint.cs can check state

    private Checkpoint currentCheckpoint;
    private List<Checkpoint> options = new List<Checkpoint>();

    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;
    private GUIStyle closeStyle;
    private GUIStyle emptyStyle;
    private bool stylesInitialized = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CheckpointManager.OnCheckpointPanelOpened += Open;
    }

    private void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Open(Checkpoint from)
    {
        options = CheckpointManager.Instance.GetOtherCheckpoints(from);
        currentCheckpoint = from;
        isOpen = true;

        ActiveGameUIManager.isPaused = true;
        Time.timeScale = 0f;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var pInput = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();
        if (pInput != null) pInput.SwitchCurrentActionMap("Menu");
    }

    public void Close()
    {
        isOpen = false;
        currentCheckpoint = null;
        options.Clear();

        ActiveGameUIManager.isPaused = false;
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        var pInput = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();
        if (pInput != null) pInput.SwitchCurrentActionMap("Player");
    }

    private void InitStyles()
    {
        if (stylesInitialized) return;
        
        panelStyle = new GUIStyle(GUI.skin.box);
        panelStyle.normal.background = MakeTex(2, 2, panelColor);
        panelStyle.border = new RectOffset(4, 4, 4, 4);

        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        titleStyle.normal.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
        buttonStyle.normal.background = MakeTex(2, 2, buttonColor);
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.background = MakeTex(2, 2, buttonHoverColor);
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.border = new RectOffset(4, 4, 4, 4);

        closeStyle = new GUIStyle(buttonStyle);
        closeStyle.normal.background = MakeTex(2, 2, closeButtonColor);
        closeStyle.hover.background = MakeTex(2, 2, new Color(0.9f, 0.25f, 0.25f, 1f));

        emptyStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, alignment = TextAnchor.MiddleCenter };
        emptyStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        stylesInitialized = true;
    }

    private void OnGUI()
    {
        if (!isOpen) return;

        InitStyles();

        int screenW = Screen.width;
        int screenH = Screen.height;
        int contentHeight = 60 + (options.Count > 0 ? options.Count * (buttonHeight + 8) : 60) + 20 + buttonHeight + 20; 
        int actualHeight = Mathf.Min(contentHeight, panelHeight);

        Rect panelRect = new Rect((screenW - panelWidth) / 2f, (screenH - actualHeight) / 2f, panelWidth, actualHeight);
        GUI.Box(panelRect, GUIContent.none, panelStyle);

        GUILayout.BeginArea(panelRect);
        GUILayout.Space(12);
        GUILayout.Label("✦ Teleport to Checkpoint ✦", titleStyle);
        GUILayout.Space(8);

        Rect divider = GUILayoutUtility.GetRect(panelWidth, 2);
        GUI.DrawTexture(divider, MakeTex(2, 2, new Color(0.4f, 0.6f, 1f, 0.6f)));
        GUILayout.Space(8);

        Checkpoint selectedTarget = null; // Fix: Store the selection to avoid modifying the list while iterating

        if (options.Count == 0)
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label("No other checkpoints activated yet.", emptyStyle);
            GUILayout.FlexibleSpace();
        }
        else
        {
            foreach (Checkpoint cp in options)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(16);
                if (GUILayout.Button(cp.CheckpointName, buttonStyle, GUILayout.Height(buttonHeight)))
                {
                    selectedTarget = cp; // Store it instead of teleporting right away
                }
                GUILayout.Space(16);
                GUILayout.EndHorizontal();
                GUILayout.Space(8);
            }
        }

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.Space(16);
        if (GUILayout.Button("✕  Close", closeStyle, GUILayout.Height(buttonHeight)))
        {
            Close();
        }
        GUILayout.Space(16);
        GUILayout.EndHorizontal();

        GUILayout.Space(12);
        GUILayout.EndArea();

        // Fix: Execute the teleport outside of the foreach loop to prevent the enumeration error
        if (selectedTarget != null)
        {
            CheckpointManager.Instance.TeleportPlayerTo(selectedTarget);
            Close();
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}