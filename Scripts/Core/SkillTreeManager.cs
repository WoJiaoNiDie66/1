using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SkillTreeManager : MonoBehaviour 
{
    public static UnityAction<SkillNodeUI> OnSkillUpgrade;

    [SerializeField]
    private PlayerDecision playerDecision;
    
    int currentUpgradeIndex = 0;

    private void Awake()
    {
        OnSkillUpgrade += UpdateSkill;
    }


    private void UpdateSkill(SkillNodeUI nodeUI)
    {
        int i = (int)nodeUI.SkillNode.SkillData.SkillType;
        if(i != 0 && currentUpgradeIndex < playerDecision.equippedSkillsL.Length)
        {
            playerDecision.equippedSkillsL[currentUpgradeIndex] = i;
            playerDecision.equippedSkillsR[currentUpgradeIndex] = i;
            currentUpgradeIndex++;
        }


        //BaseSkill skill = CooldownSystem.Instance.GetSkill(i);
        //SkillUpgradeManager.Instance.ApplyUpgrade(skill,nodeUI.SkillNode.SkillData.UpgradeType,nodeUI.SkillNode.SkillData.Value);
        //SkillUpgradeManager.Instance.RecordUpgrade(i, nodeUI.SkillNode.SkillData.UpgradeType);
    }

}
