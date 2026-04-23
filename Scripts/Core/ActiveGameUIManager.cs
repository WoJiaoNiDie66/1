// Assets/Scripts/Core/UIManager.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ActiveGameUIManager : MonoBehaviour
{

    public static UnityAction<(float,float,float,float,float,float)> OnPlayerStatsChanged;
    public static UnityAction<int> OnPlayerCoinColleceted;
    public static UnityAction<int> OnPlayerHealPotionUsed;
    public static UnityAction OnPlayerStatsReset;
    public static bool isPaused = false;

    [SerializeField]
    private GameObject pauseMenu;

    [SerializeField]
    private CombatSystem_Player_A0 playerSystem;

    [SerializeField]
    private PlayerInput playerInput;

    [SerializeField]
    private PlayerMain_A0 player;

    /// <summary>
    /// elementsUI[0]: Health UI
    /// elementsUI[1]: Focus UI
    /// elementsUI[2]: Stamina UI
    /// </summary>
    [SerializeField]
    private ElementUI[] elementUIs;

    [SerializeField]
    private PotionUI potionUI;

    [SerializeField]
    private CoinUI coinUI;

    private void Awake()
    {
        OnPlayerStatsChanged += UpdateElementUIs;
        OnPlayerStatsReset += InitializeElementUIs;
        OnPlayerHealPotionUsed += UpdateHealPotionUI;
        OnPlayerCoinColleceted += UpdateCoinUI;
    }

    private void OnDestroy()
    {
        OnPlayerStatsChanged -= UpdateElementUIs;
        OnPlayerStatsReset -= InitializeElementUIs;
        OnPlayerHealPotionUsed -= UpdateHealPotionUI;
        OnPlayerCoinColleceted -= UpdateCoinUI;
    }

    private void Start()
    {
        //GameManager.onGamePaused += ShowPauseMenu;
        //GameManager.onGameResumed += HidePauseMenu;
        //continueButton.onClick.AddListener(()=>GameManager.onGameResumed?.Invoke());
        //if(CooldownSystem.Instance != null)
        //{
        //    int skillCount = CooldownSystem.Instance.GetSkillCount();
        //    if (skillUIList.Count > skillCount)
        //    {
        //        Debug.LogError("SkillUI and skill size mismatch");
        //    }
        //    else
        //    {
        //        for (int i = 0; i < skillCount; i++)
        //        {
        //            skillUIList[i].AssignSkill(CooldownSystem.Instance.GetSkill(i));
        //        }
        //    }
        //}
        //else
        //{
        //    Debug.LogError("CooldownSystem Instance is null");
        //}

        //if(GameManager.Instance.CurrentState != GameManager.GameState.Paused)HidePauseMenu();

        if(playerSystem == null)
        {
            Debug.LogError("Player System reference is missing in ActiveGameUIManager");
        }

        InitializeElementUIs();

        HidePauseMenu();
        playerInput.SwitchCurrentActionMap("Player");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                ShowPauseMenu();
                isPaused = true;
                playerInput.SwitchCurrentActionMap("Menu"); 
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0f;
            }
            else if (!KeyBindManager.IsActive)
            {
                HidePauseMenu();
                isPaused = false;
                Cursor.lockState = CursorLockMode.Locked;
                Time.timeScale = 1f;
                playerInput.SwitchCurrentActionMap("Player");
            }
        }
    }

    private void HidePauseMenu()
    {
        pauseMenu.SetActive(false);
    }

    private void ShowPauseMenu()
    {
        pauseMenu.SetActive(true);
    }

    private void InitializeElementUIs()
    {
        elementUIs[0].ResetUI();
        elementUIs[1].ResetUI();
        elementUIs[2].ResetUI();
        potionUI.ResetUI();
    }

    private void UpdateElementUIs(
        (
        float currentHealth, float currentFocus, float currentStamina,
        float maxHealth,float maxFocus, float maxStamina
        ) a
    )
    {
        elementUIs[0].UpdateUI(a.currentHealth, a.maxHealth);
        elementUIs[1].UpdateUI(a.currentFocus, a.maxFocus);
        elementUIs[2].UpdateUI(a.currentStamina, a.maxStamina);
    }

    private void UpdateHealPotionUI(int charge)
    {
        if(potionUI == null)
        {
            Debug.LogError("Potion UI reference is missing in ActiveGameUIManager");
            return;
        }

        potionUI.UpdateUI(charge);
    }

    private void UpdateCoinUI(int coinCount)
    {
        if(player == null)
        {
            Debug.LogError("Player reference is missing in ActiveGameUIManager");
            return;
        }

        if (coinUI == null)
        {
            Debug.LogError("Coin UI reference is missing in ActiveGameUIManager");
            return;
        }
        
        player.IncrementCoinCount(coinCount);
        coinUI.updateCount(player.Coins);

    }

}
