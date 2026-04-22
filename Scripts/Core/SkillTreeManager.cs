using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SkillTreeManager : MonoBehaviour 
{
    public static UnityAction<SkillNodeUI> OnSkillUpgrade;
    [SerializeField]
    private PlayerMain_A0 playerMain;

    [SerializeField]
    private List<SkillNodeUI> skillNodeUIs;

    private int currentOrder = 0;


    private void Awake()
    {
        OnSkillUpgrade += UpdateSkill;
    }

    private void Start()
    {
        for (int i = 0; i < skillNodeUIs.Count; i++)
        {
            if(skillNodeUIs[i] == null)
            {
                Debug.LogError("Skill Node UI cannot be null.");
            }
            if(skillNodeUIs[i].SkillNode == null)
            {
                Debug.LogError("Skill Node cannot be null.");
            }
             if(skillNodeUIs[i].SkillNode.SkillData == null)
            {
                Debug.LogError("Skill Data cannot be null.");
            }
        }

        InitializeSkills();

    }

    private void InitializeSkills()
    {
        for (int i = 0; i < skillNodeUIs.Count; i++)
        {
            if(skillNodeUIs[i].SkillNode.SkillData.IsLocked) continue;
            if (skillNodeUIs[i].SkillNode.SkillData.SkillType == -1)
            {
                Debug.LogWarning($"Skill Type {skillNodeUIs[i].SkillNode.SkillData.SkillType} will not be used.");
                continue;
            }
            playerMain._playerDecision.equippedSkillsL[i] = skillNodeUIs[i].SkillNode.SkillData.SkillType;
            playerMain._playerDecision.equippedSkillsR[i] = skillNodeUIs[i].SkillNode.SkillData.SkillType;
        }
    }


    private void UpdateSkill(SkillNodeUI nodeUI)
    {
        int i = nodeUI.SkillNode.SkillData.SkillType;
        if(i == -1)
        {
            return;
        }

        Debug.Log($"Upgrading skill: {i} at order {currentOrder}");
        nodeUI.SkillNode.SetUpgradeOrder(currentOrder);
        playerMain._playerDecision.equippedSkillsL[currentOrder] = nodeUI.SkillNode.SkillData.SkillType;
        playerMain._playerDecision.equippedSkillsR[currentOrder] = nodeUI.SkillNode.SkillData.SkillType;
        currentOrder++;
        //BaseSkill skill = CooldownSystem.Instance.GetSkill(i);
        //SkillUpgradeManager.Instance.ApplyUpgrade(skill,nodeUI.SkillNode.SkillData.UpgradeType,nodeUI.SkillNode.SkillData.Value);
        //SkillUpgradeManager.Instance.RecordUpgrade(i, nodeUI.SkillNode.SkillData.UpgradeType);
    }

}
