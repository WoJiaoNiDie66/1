using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillNode
{
    [SerializeField]private SkillUpgradeData _skillData;
    [SerializeField]private SkillNodeUI parentNode;
    [SerializeField]private List<SkillNodeUI> children;


    public SkillUpgradeData SkillData => _skillData;
    public SkillNodeUI ParentNode => parentNode;
    public List<SkillNodeUI> Children => children;

    [SerializeField]
    private bool locked = true;

    [SerializeField]
    private bool unlockable = false;

    private int upgradeOrder = -1;

    public bool HasBeenInitializedBySave { get; set; } = false;

    public bool IsLocked => locked;
    public bool Unlockable => unlockable;

    public void ApplyUpgrade()
    {
        if (!IsLocked)
        {
            Debug.Log("Skill has already been upgraded");
            return;
        }

        SetLocked(false);
        //_skillData.SetLocked(false);
        if (Unlockable && !IsLocked)
        {
            Debug.Log("Unlocking");
            foreach (SkillNodeUI nodeUI in children)
            {
                nodeUI.SkillNode.SetUnlockable(true);
                nodeUI.UpdateUpgrade();
            }
        }
    }

    public void SetUpgradeOrder(int order)
    {
        upgradeOrder = order;
    }

    public void SetLocked(bool locked)
    {
        this.locked = locked;
    }

    public void SetUnlockable(bool unlockable)
    {
        //this.unlockable = unlockable;
        this.unlockable = unlockable;
    }

}

public enum SkillType { 
    FIREBALL,
    SKILL2,
    SKILL3,
    SKILL4
}
