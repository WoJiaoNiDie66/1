// Assets/Scripts/UI/Game UI/Skill Tree/SkillNodeUI.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillNodeUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _skillBorder;
    [SerializeField] private Sprite[] _borderSprites;
    [SerializeField] private SkillNode _skillNode;
    [SerializeField] private List<Image> _upgradeLinkImages;
    [SerializeField] private Color[] _linkColor;
    [SerializeField] private Image skillIcon;

    private SkillNodeSelector _parentSelector;
    public SkillNode SkillNode => _skillNode;

    // --- NEW: Safely initialize SO data once ---
    public void InitializeFromSO()
    {
        if (_skillNode == null || _skillNode.SkillData == null) return;
        
        // Prevent overwriting if already initialized!
        if (_skillNode.HasBeenInitializedBySave) return;

        _skillNode.SetLocked(_skillNode.SkillData.IsLocked);
        _skillNode.SetUnlockable(_skillNode.SkillData.Unlockable);
        
        _skillNode.HasBeenInitializedBySave = true;
    }

    private void Start()
    {
        if(_skillNode == null || _skillNode.SkillData == null) return;

        // 1. Pull defaults ONLY if the SaveManager hasn't already done it
        InitializeFromSO();

        skillIcon.sprite = _skillNode.SkillData.UpgradeSprite;

        // 2. Set visuals based on the CURRENT state (which might have been unlocked by the SaveManager)
        UpdateVisualState();

        _parentSelector = transform.parent?.GetComponent<SkillNodeSelector>();
    }

    // Helper to sync colors and borders on Start
    private void UpdateVisualState()
    {
        if(_skillNode.Unlockable && !_skillNode.IsLocked)
        {
            for (int i = 0; i < _upgradeLinkImages.Count; i++) _upgradeLinkImages[i].color = _linkColor[0];
            _skillBorder.sprite = _borderSprites[1];
        }
        else if(_skillNode.IsLocked && _skillNode.Unlockable)
        {
            for (int i = 0; i < _upgradeLinkImages.Count; i++) _upgradeLinkImages[i].color = _linkColor[1];
            _skillBorder.sprite = _borderSprites[2];
        }
        else
        {
            for (int i = 0; i < _upgradeLinkImages.Count; i++) _upgradeLinkImages[i].color = _linkColor[2];
            _skillBorder.sprite = _borderSprites[3];
        }
    }

    public void Upgrade()
    {
        if (_skillNode.Unlockable) { 
            _skillNode.ApplyUpgrade();
            SkillTreeManager.OnSkillUpgrade?.Invoke(this);
            for (int i = 0; i < _upgradeLinkImages.Count; i++)
            {
                _upgradeLinkImages[i].color = _linkColor[2];
            }
        }
    }

    public void UpdateUpgrade()
    {
        if(_skillNode.IsLocked && _skillNode.Unlockable) {
            _skillBorder.sprite = _borderSprites[2];
            for (int i = 0; i < _upgradeLinkImages.Count; i++)
            {
                _upgradeLinkImages[i].color = _linkColor[1];
            }
        }
        else _skillBorder.sprite = _borderSprites[3];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Upgrade();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ToolTipSystem.Instance != null) ToolTipSystem.Instance.OnToolTipSelected(this);
        _parentSelector?.UIHover(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ToolTipSystem.Instance != null) ToolTipSystem.Instance.CloseToolTip();
    }

    public void Highlight()
    {
        _skillBorder.sprite = _borderSprites[0];
    }

    public void UnHighlight()
    {
        if (_skillNode.Unlockable && !_skillNode.IsLocked)
        {
            _skillBorder.sprite = _borderSprites[1];
        }
        else if (_skillNode.Unlockable && _skillNode.IsLocked)
        {
            _skillBorder.sprite = _borderSprites[2];
        }
        else
        {
            _skillBorder.sprite = _borderSprites[3];
        }
    }

    public string GetSkillID()
    {
        if (_skillNode != null && _skillNode.SkillData != null)
            return ScriptableObjectRuntimeSaveUtil.GetId(_skillNode.SkillData);
        return string.Empty;
    }

    public bool IsNodeUnlocked()
    {
        return _skillNode != null && !_skillNode.IsLocked;
    }
}