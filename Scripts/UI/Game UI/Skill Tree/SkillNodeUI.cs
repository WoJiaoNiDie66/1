using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// SkillNodeUI is a node that reference to SkillNode class (Don't mix between the two).
/// With this SkillNode can reference to its parent and also children.
/// 
/// In default: 
/// parent SkillNodeUI does NOT have a parent Node (i.e. _skillNode.parentNode = null)
/// 
/// Leaves of the tree does NOT have any children. (i.e. _skillNode.children.Count = 0)
/// </summary>
public class SkillNodeUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image _skillBorder;

    /// <summary>
    /// _borderSprites[0]: Selected border
    /// _borderSprites[1]: Unselected learnt border
    /// _borderSprites[2]: Unselected unlockable border
    /// _borderSprites[3]: Unselected locked border
    /// </summary>
    [SerializeField]
    private Sprite[] _borderSprites;

    [SerializeField] 
    private SkillNode _skillNode;

    [SerializeField]
    private List<Image> _upgradeLinkImages;

    /// <summary>
    /// _linkColor[0]: grey,
    /// _linkColor[1]: white,
    /// _linkColor[2]: yellow.
    /// </summary>
    [SerializeField]
    private Color[] _linkColor;

    [SerializeField]
    private Image skillIcon;

    private SkillNodeSelector _parentSelector;

    public SkillNode SkillNode => _skillNode;
    // Start is called before the first frame update
    private void Start()
    {
        if(_skillNode == null)
        {
            Debug.LogError("Skill Node cannot be null.");
            return;
        }

        if(_skillNode.SkillData == null)
        {
            Debug.LogError("Skill Data cannot be null.");
            return;
        }

        _skillNode.SetLocked(_skillNode.SkillData.IsLocked);
        _skillNode.SetUnlockable(_skillNode.SkillData.Unlockable);

        skillIcon.sprite = _skillNode.SkillData.UpgradeSprite;

        //if(!_skillNode.SkillData.Unlockable)
        if(_skillNode.Unlockable)
        {
            for (int i = 0; i < _upgradeLinkImages.Count; i++)
            {
                _upgradeLinkImages[i].color = _linkColor[0];
            }
        }
        //else if(_skillNode.SkillData.IsLocked)
        else if(_skillNode.IsLocked)
        {
            for (int i = 0; i < _upgradeLinkImages.Count; i++)
            {
                _upgradeLinkImages[i].color = _linkColor[1];
            }
        }
        else
        {
            for (int i = 0; i < _upgradeLinkImages.Count; i++)
            {
                _upgradeLinkImages[i].color = _linkColor[2];
            }
        }

        _parentSelector = transform.parent.GetComponent<SkillNodeSelector>();
    }

    public void Upgrade()
    {
        //if (_skillNode.SkillData.Unlockable) { 
        if (_skillNode.Unlockable) { 
            _skillNode.ApplyUpgrade();
            SkillTreeManager.OnSkillUpgrade.Invoke(this);
            for (int i = 0; i < _upgradeLinkImages.Count; i++)
            {
                _upgradeLinkImages[i].color = _linkColor[2];
            }
        }
    }
    public void UpdateUpgrade()
    {
        //if (_skillNode.SkillData.IsLocked && _skillNode.SkillData.Unlockable) {
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
        ToolTipSystem.Instance.OnToolTipSelected(this);
        _parentSelector?.UIHover(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ToolTipSystem.Instance.CloseToolTip();
    }

    public void Highlight()
    {
        _skillBorder.sprite = _borderSprites[0];
    }

    public void UnHighlight()
    {
        //if (!_skillNode.SkillData.Unlockable)
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
        {
            // Will fallback to the filename of the ScriptableObject
            return ScriptableObjectRuntimeSaveUtil.GetId(_skillNode.SkillData);
        }
        return string.Empty;
    }

    // Returns true if the local node wrapper has been unlocked during runtime
    public bool IsNodeUnlocked()
    {
        return _skillNode != null && !_skillNode.IsLocked;
    }
}
