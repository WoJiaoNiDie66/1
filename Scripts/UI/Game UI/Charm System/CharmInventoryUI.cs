// Assets/Scripts/UI/Game UI/Charm System/CharmInventoryUI.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CharmInventoryUI : SelectorManager
{
    [SerializeField] private CharmDescriptor charmDescriptor;
    [SerializeField] private Scrollbar charmScrollbar;
    [SerializeField] private Transform viewPort;
    [SerializeField] private float slotMaxDistance;
    [SerializeField] private float sliderMoveScale;

    protected override void Start()
    {
        InitializeUI();
        
        // Listen for when the SaveManager finishes loading data
        CharmSaveBridge.OnCharmStateApplied += TriggerSync;
        
        // Run it once on Start just in case it loaded before this script woke up
        TriggerSync();
        enabled = false;
    }

    protected virtual void OnDestroy()
    {
        CharmSaveBridge.OnCharmStateApplied -= TriggerSync;
    }

    private void TriggerSync()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(SyncCoroutine());
        }
        else
        {
            // If the UI is disabled when loading, we flag it to sync next time it enables
            pendingSync = true;
        }
    }

    private bool pendingSync = false;
    private void OnEnable()
    {
        if (pendingSync)
        {
            StartCoroutine(SyncCoroutine());
        }
    }

    // Wait until the end of the frame to ensure CharmCostManager is 100% awake!
    private IEnumerator SyncCoroutine()
    {
        yield return new WaitForEndOfFrame();
        pendingSync = false;

        if (uis == null || uis.Length == 0) yield break;

        for (int i = 0; i < uis.Length; i++)
        {
            var slotUI = uis[i] as CharmSlotUI;
            if (slotUI == null || slotUI.Charm == null) continue;

            // If the Save File says it's equipped, but the UI slot doesn't know it yet
            if (slotUI.Charm.Equipped && !slotUI.Equipped)
            {
                Debug.Log($"<color=cyan>[Save Sync]</color> Force Equipping: {slotUI.Charm.CharmName}");
                
                slotUI.OnEquipped(); 
                CharmManager.OnCharmEquipped?.Invoke(slotUI.Charm); // Tells top UI to populate slot

                if (CharmCostManager.Instance != null)
                {
                    CharmCostManager.Instance.IncreaseCost(slotUI.Charm.CharmCost);
                }
                else
                {
                    Debug.LogError($"<color=red>Could not apply cost for {slotUI.Charm.CharmName}. CharmCostManager.Instance is NULL!</color>");
                }
            }
            // If Save File says it's NOT equipped, but UI slot thinks it is (Cleanup from previous saves)
            else if (!slotUI.Charm.Equipped && slotUI.Equipped)
            {
                Debug.Log($"<color=yellow>[Save Sync]</color> Force Unequipping: {slotUI.Charm.CharmName}");
                
                slotUI.OnUnequipped();
                CharmManager.OnCharmUnequipped?.Invoke(slotUI.Charm);

                if (CharmCostManager.Instance != null)
                {
                    CharmCostManager.Instance.DecreaseCost(slotUI.Charm.CharmCost);
                }
            }
        }
    }

    private void InitializeUI()
    {
        CharmManager.OnCharmUnequipped += UpdateUnequippedCharm;
        CharmManager.OnUIModeChanged += UIHover;
        currentIndex = 0;
        
        if (uis.Length < 2)
        {
            Debug.LogError("Menu has only one option");
            return;
        }
        for (int i = 0; i < uis.Length; i++)
        {
            uis[i].UnHighlight();
        }
    }

    protected override void Update()
    {
        if (CharmManager.EquippedCharmMode || CharmManager.IsSwitchingMode) return;

        Debug.Log("In Charm Mode.");

        if (Input.GetKeyDown(KeyCode.F))
        {
            var slotUI = uis[currentIndex] as CharmSlotUI;
            if (slotUI == null || slotUI.Charm == null) return;
            if (!slotUI.Charm.Unlocked)
            {
                Debug.Log("Charm is not unlocked.");
                return;
            }

            // Equip Manually
            if (!slotUI.Equipped && CharmCostManager.Instance.CheckValidCost(slotUI.Charm.CharmCost))
            {
                slotUI.Charm.SetEquipped(true); // Save state update
                CharmManager.OnCharmEquipped.Invoke(slotUI.Charm);
                slotUI.OnEquipped();
                CharmCostManager.Instance.IncreaseCost(slotUI.Charm.CharmCost);
            }
            // Remove Manually
            else if (slotUI.Equipped)
            {
                slotUI.Charm.SetEquipped(false); // Save state update
                CharmManager.OnCharmUnequipped.Invoke(slotUI.Charm);
                CharmCostManager.Instance.DecreaseCost(slotUI.Charm.CharmCost);
            }
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentIndex > 0) currentIndex--;
            UIHover();
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentIndex < uis.Length - 1) currentIndex++;
            UIHover();
        }
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentIndex >= columns)
            {
                currentIndex -= columns;
            }
            else if (currentIndex < columns)
            {
                CharmManager.SwitchCharmMode();
                return;
            }
            UIHover();
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {

            if (currentIndex + columns < uis.Length)
            {
                Debug.Log("Go Down");
                currentIndex += columns;
            }
            UIHover();  

        }
    }

    public void OnItemUnselected()
    {
        charmDescriptor.ResetDescription();
    }

    public override void UIHover()
    {
        if (!CharmManager.EquippedCharmMode)
        {
            for (int i = 0; i < uis.Length; i++)
            {
                if (i != currentIndex) uis[i].UnHighlight();
                else uis[i].Highlight();
            }

            var slotUI = uis[currentIndex] as CharmSlotUI;
            if (slotUI == null) return;

            if (slotUI.Charm == null || !slotUI.Charm.Unlocked)
            {
                charmDescriptor.ResetDescription();
            }
            else if (slotUI.Charm != null && slotUI.Charm.Unlocked)
            {
                charmDescriptor.SetDescription(
                    slotUI.Charm.CharmName,
                    slotUI.Charm.CharmCost,
                    slotUI.Charm.CharmDescription,
                    slotUI.Charm.CharmSprite
                );
            }

            float distance = viewPort.position.y - slotUI.transform.position.y;
            int count = 0;
            if (distance * distance > slotMaxDistance * slotMaxDistance)
            {
                if (distance > 0 && charmScrollbar.value - sliderMoveScale > 0)
                {
                    while (distance * distance > slotMaxDistance * slotMaxDistance)
                    {
                        charmScrollbar.value -= sliderMoveScale;
                        distance = viewPort.position.y - slotUI.transform.position.y;
                        count++;
                        if (count > 10) break; 
                    }
                }
                else if (distance < 0 && charmScrollbar.value + sliderMoveScale <= 1)
                {
                    while (distance * distance > slotMaxDistance * slotMaxDistance)
                    {
                        charmScrollbar.value += sliderMoveScale;
                        distance = viewPort.position.y - slotUI.transform.position.y;
                        count++;
                        if (count > 10) break;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < uis.Length; i++) uis[i].UnHighlight();
        }
    }

    public override void UIHover(SelectionUI ui)
    {
        if (CharmManager.EquippedCharmMode)
        {
            CharmManager.SwitchCharmMode();
        }


        var slotUI = ui as CharmSlotUI;
        if (slotUI == null) return;
        
        for (int i = 0; i < uis.Length; i++)
        {
            CharmSlotUI slot = uis[i] as CharmSlotUI;
            if (slotUI != slot) slot.UnHighlight();
            else 
            { 
                slot.Highlight(); 
                currentIndex = i;
            }
        }

        if (slotUI.Charm == null || !slotUI.Charm.Unlocked)
        {
            charmDescriptor.ResetDescription();
        }
        else if (slotUI.Charm != null && slotUI.Charm.Unlocked)
        {
            charmDescriptor.SetDescription(
                slotUI.Charm.CharmName,
                slotUI.Charm.CharmCost,
                slotUI.Charm.CharmDescription,
                slotUI.Charm.CharmSprite
            );
        }
    }

    public override void UIClicked(SelectionUI ui)
    {
        var slotUI = uis[currentIndex] as CharmSlotUI;
        if (slotUI == null || slotUI.Charm == null || !slotUI.Charm.Unlocked) return;

        if (!slotUI.Equipped && CharmCostManager.Instance.CheckValidCost(slotUI.Charm.CharmCost))
        {
            slotUI.Charm.SetEquipped(true);
            CharmManager.OnCharmEquipped.Invoke(slotUI.Charm);
            slotUI.OnEquipped();
            CharmCostManager.Instance.IncreaseCost(slotUI.Charm.CharmCost);
        }
        else if (slotUI.Equipped)
        {
            slotUI.Charm.SetEquipped(false);
            CharmManager.OnCharmUnequipped.Invoke(slotUI.Charm);
            CharmCostManager.Instance.DecreaseCost(slotUI.Charm.CharmCost);
        }
    }

    private void UpdateEquippedCharm()
    {
        var slotUI = uis[currentIndex] as CharmSlotUI;
        slotUI.OnEquipped();
    }

    private void UpdateEquippedCharm(Charm charm)
    {
        for (int i = 0; i < uis.Length; i++)
        {
            var slotUI = uis[i] as CharmSlotUI;
            if (slotUI == null) return;
            if (slotUI.Charm == charm) slotUI.OnEquipped();
        }
    }

    private void UpdateUnequippedCharm()
    {
        var slotUI = uis[currentIndex] as CharmSlotUI;
        slotUI.OnUnequipped();
    }

    private void UpdateUnequippedCharm(Charm charm)
    {
        for (int i = 0; i < uis.Length; i++)
        {
            var slotUI = uis[i] as CharmSlotUI;
            if (slotUI == null) return;
            if (slotUI.Charm != null && slotUI.Charm == charm)
            {
                slotUI.OnUnequipped();
                break;
            }
        }
    }

    public List<Charm> GetEquippedCharms()
    {
        List<Charm> charms = new List<Charm>();
        for(int i = 0; i < uis.Length; i++)
        {
            var slotUI = uis[i] as CharmSlotUI;
            if (slotUI != null && slotUI.Charm != null && slotUI.Equipped)
            {
                charms.Add(slotUI.Charm);
            }
        }
        return charms;
    }
}