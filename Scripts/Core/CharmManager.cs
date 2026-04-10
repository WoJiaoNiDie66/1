using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharmManager : MonoBehaviour
{
    public static UnityAction<Charm> OnCharmEquipped;
    public static UnityAction<Charm> OnCharmUnequipped;
    public static UnityAction OnUIModeChanged;
    public static bool EquippedCharmMode = true;

    [SerializeField] private CharmInventoryUI charmInventoryUI;
    [SerializeField] private EquipCharmInventoryUI equipCharmInventoryUI;
    [SerializeField] private CharmCostManager costManager;

    // NEW: Reference to player combat system
    [SerializeField] private CombatSystem_Player_A0 playerCombatSystem;

    private void Awake()
    {
        OnCharmEquipped += HandleCharmEquipped;
        OnCharmUnequipped += HandleCharmUnequipped;
        UpdateCharm();
    }

    private void OnDestroy()
    {
        OnCharmEquipped -= HandleCharmEquipped;
        OnCharmUnequipped -= HandleCharmUnequipped;
    }

    private void HandleCharmEquipped(Charm charm)
    {
        if (playerCombatSystem == null) return;
        switch (charm.CharmID)
        {
            case 0: playerCombatSystem.charmRunSpeed = true; break;
            case 1: playerCombatSystem.charmHealthRegen = true; break;
            case 2: playerCombatSystem.charmStaminaBoost = true; break;
            case 3: playerCombatSystem.charmHealOnHit = true; break;
            case 4: playerCombatSystem.charmDamageReduction = true; break;
        }
    }

    private void HandleCharmUnequipped(Charm charm)
    {
        if (playerCombatSystem == null) return;
        switch (charm.CharmID)
        {
            case 0: playerCombatSystem.charmRunSpeed = false; break;
            case 1: playerCombatSystem.charmHealthRegen = false; break;
            case 2: playerCombatSystem.charmStaminaBoost = false; break;
            case 3: playerCombatSystem.charmHealOnHit = false; break;
            case 4: playerCombatSystem.charmDamageReduction = false; break;
        }
    }

    private void UpdateCharm()
    {
        int totalCost = 0;
        List<Charm> charms = charmInventoryUI.GetEquippedCharms();
        foreach (Charm charm in charms)
        {
            equipCharmInventoryUI.InitializeEquipCharmSlots(charm);
            totalCost += charm.CharmCost;
            // Re-apply effects on load
            HandleCharmEquipped(charm);
        }
        costManager.InitializeUI(totalCost);
    }
}