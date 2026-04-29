using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using System;
using System.Collections.Generic;
using System.Collections;
[Serializable]
public class SkeletonSwordDecision : MonoBehaviour
{
    [Header("Respawn Settings")]
    public bool shouldRespawn = true; 
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    public SkeletonSwordMain_A0 _playerMain;
    [SerializeField] public EnemySkill[] battleSkills;
    /*
        0. idle L
        1. idle R
        2. attack 1
        3. attack 2
        4. attack 3
        5. attack 4
    */
    public int[] equippedSkillsR = { 5, -1, 12, -1, 11 }; // Max 5
    public int[] equippedSkillsL = { -1, 10, -1, 13, -1 }; // Max 5
    public int[] equippedItems = { -1, -1, -1, 14, 15 }; // Max 5
    public int currentEquippedSkillR = 0;
    public int currentEquippedSkillL = 1;
    public int currentEquippedItem = 3;
    public int curNextSkill = 0;

    public float minFloat = 0f;
    public float maxFloat = 100f;
    public float randomFloat = 100f;

    public bool applyQLearning = false; // can be used to enable or disable q learning in Decide function

    /*
        0: idle/walk/run blend
        1: dodge
        2: heal
        3: attack
        4: jump
        5: skillR
        6: skillL
        7: item
        8: sprint
        9: camlock
        10: interact
        11: switchR
        12: switchL
        13: switchItem

        14: get hit
        20: idle
        21: move back
        22: chase
    */

    public int _animID_getHitType;
    public int _animID_getHit;
    public int _animID_HP;
    public int _animID_FP; // temp ui
    public int _animID_SP; // temp ui
    public int _animID_PP; // temp ui
    public int _animIDGrounded;
    public int _animIDNextSkill;
    public int _animIDNextState;
    public int _animIDDeath;
    public int _animIDActionLock;

    public int _animIDSpeed;
    public int _animIDMotionSpeed;
    public int _animIDVV;

    public int _animIDGo;

    // attack behaviour
    [SerializeField] public int[] sideCounter;
    [SerializeField] public int[] farCounter;
    [SerializeField] public int[] drinkCounter;

    // navigation
    public Transform Target;
    public NavMeshAgent m_Agent;
    public float m_Distance;
    public float CloseChaseDistance;
    public float FarChaseDistance;
    public bool isHunting;

    // for debugging
    public bool debugMode = false;

    [Header("Boss 钩爪技能配置")]
    public LineRenderer bossLR;          // Boss 的绳索渲染器
    public Transform bossGunTip;         // Boss 手部发射点
    public GameObject bossHookPrefab;    // 钩爪头预制体
    
    [Space(10)]
    public float hookScale = 50f;        // 【需求】：缩放大小设为 50
    public Vector3 hookRotationOffset;   // 【需求】：开放角度调整
    public float flyOutDuration = 0.5f;  // 【需求】：匹配动画，调整飞出快慢
    public float stayDuration = 0.2f;    // 钩爪在终点停留的时间
    
    private GameObject activeBossHook;   // 运行时生成的钩爪实例

    public void AssignAnimationIDs()
    {
        _animID_getHitType = Animator.StringToHash("getHitType");
        _animID_getHit = Animator.StringToHash("getHit");
        _animID_HP = Animator.StringToHash("HP");
        _animID_FP = Animator.StringToHash("FP"); // temp ui
        _animID_SP = Animator.StringToHash("SP"); // temp ui
        _animID_PP = Animator.StringToHash("PP"); // temp ui
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDNextSkill = Animator.StringToHash("NextSkill");
        _animIDNextState = Animator.StringToHash("NextState");
        _animIDDeath = Animator.StringToHash("Death");
        _animIDActionLock = Animator.StringToHash("ActionLock");

        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDVV = Animator.StringToHash("VV"); // temp ui

        _animIDGo = Animator.StringToHash("Go");

        m_Agent = GetComponent<NavMeshAgent>();
    }

    public void Decide()
    {
        // Implement your decision logic here
        if (debugMode) Debug.Log("Deciding next action based on current state and inputs.");

        // Hunt behaviour
        m_Distance = Vector3.Distance(transform.position, Target.position);
        UpdateChaseState();
        
        // Attack behaviour
        nextSkill();
            // side handling
            SideHandling();
            // far handling
            FarHandling();
            // drink handling
            // DrinkHandling();

        if (m_Distance < battleSkills[curNextSkill].minDistance)
        {
            // move back
            _playerMain.nextStateID = _playerMain.nextStateID == 14 ? 14 : 21;
        }
        if (m_Distance > battleSkills[curNextSkill].maxDistance)
        {
            // chase
            _playerMain.nextStateID = _playerMain.nextStateID == 14 ? 14 : 22;
        }
        if (!isHunting)
        {
            // idle
            _playerMain.nextStateID = _playerMain.nextStateID == 14 ? 14 : 20;
        }

        _playerMain._animator.SetInteger(_animID_getHitType, _playerMain._combatSystem.curKB_getHitType);
        _playerMain._animator.SetInteger(_animID_getHit, _playerMain._combatSystem.curKB_getHit);
        _playerMain._animator.SetFloat(_animID_HP, _playerMain._combatSystem.currentHealth);
        _playerMain._animator.SetFloat(_animID_FP, _playerMain._combatSystem.currentFocus); // temp ui
        _playerMain._animator.SetFloat(_animID_SP, _playerMain._combatSystem.currentStamina); // temp ui
        _playerMain._animator.SetFloat(_animID_PP, _playerMain._combatSystem.currentPoise); // temp ui
        _playerMain._animator.SetBool(_animIDGrounded, _playerMain.Grounded);
        _playerMain._animator.SetInteger(_animIDNextSkill, curNextSkill);
        _playerMain._animator.SetInteger(_animIDNextState, _playerMain.nextStateID);

        _playerMain._animator.SetBool(_animIDGo, true); // trigger animation transition, can be used in Animator to control the character's animation state
    }

    private void UpdateChaseState()
    {
        if (isHunting)
        {
            // Implement chase behavior
            isHunting = m_Distance < FarChaseDistance ? true : false;
        }
        else
        {
            isHunting = m_Distance < CloseChaseDistance ? true : false;
        }
    }

    // ===========================

    private void nextSkill()
    {
        // if the current skill is an attack skill, keep the current skill, can be used in Animator to trigger certain skill animation
        if (curNextSkill >= 2)
        {
            return;
        }

        // L R switching
        if (_playerMain._anim_enemySkillIndex < 2 && curNextSkill < 2) // if it is idle or dodge, set next state ID to the index of the skill
        {
            float idleRLFloat = UnityEngine.Random.Range(0f, 1f);
            if (idleRLFloat < Time.deltaTime * 0.333f) // geometric distribution E[X] = 1/p
            {
                curNextSkill = (curNextSkill + 1) % 2; // switch between idle L and idle R
            }
        }

        // check Stamina, if it is below the stamina cost of the current skill, switch to idle
        float idle2AttackFloat = UnityEngine.Random.Range(0f, 1f);
        if (idle2AttackFloat > Time.deltaTime * _playerMain._combatSystem.currentStamina / _playerMain._combatSystem.maxStamina) // the lower the stamina, the more likely to switch to attack
        {
            return; // stay in idle
        }

        // reset (max/min)Float to sum of posibility of all skills, can be used for random skill selection in enemy AI
        minFloat = battleSkills[0].posibility + battleSkills[1].posibility;
        maxFloat = 0f;
        for (int i = 0; i < battleSkills.Length; i++)
        {
            maxFloat += battleSkills[i].posibility;
        }

        // Draw a random float between minFloat and maxFloat, can be used for random skill selection in enemy AI
        randomFloat = UnityEngine.Random.Range(minFloat, maxFloat);
        float cumulativePosibility = 0f;
        for (int i = 0; i < battleSkills.Length; i++)
        {
            if (randomFloat >= cumulativePosibility && randomFloat < cumulativePosibility + battleSkills[i].posibility)
            {
                curNextSkill = i;
                if (debugMode) Debug.Log("Selected Skill: " + battleSkills[i].Name + " with posibility: " + battleSkills[i].posibility);
                break;
            }
            cumulativePosibility += battleSkills[i].posibility;
        }
    }

    private void SideHandling()
    {
        if (Target == null) return;

        // Check if player is outside enemy FOV (120° total -> 60° either side)
        Vector3 toTarget = (Target.position - transform.position).normalized;
        float angleDeg = Vector3.SignedAngle(transform.forward, toTarget, Vector3.up);
        if (Mathf.Abs(angleDeg) <= 60f)
        {
            // player is inside forward FOV; skip side/behind handling
            return;
        }

        if (curNextSkill >= 2 &&
            sideCounter.Length > 0 &&
            // m_Distance < battleSkills[curNextSkill].maxDistance &&
            m_Distance < battleSkills[sideCounter[0]].maxDistance )
        {
            float tempFloat = UnityEngine.Random.Range(0f, 1f);
            int tempInt = UnityEngine.Random.Range(0, sideCounter.Length);
            tempInt = sideCounter[tempInt];
            if (tempFloat <
                Time.deltaTime *
                battleSkills[tempInt].posibility /
                battleSkills[tempInt].defaultPosibility ) // geometric distribution E[X] = 1/p // times 1 / next expected happen time
            {
                curNextSkill = tempInt;
                if (debugMode) Debug.Log($"SideHandling: player angle {angleDeg:F1}°, switching to skill {curNextSkill}");
            }
        }
    }

    private void FarHandling()
    {
        if (curNextSkill >= 2 &&
            farCounter.Length > 0 &&
            m_Distance > battleSkills[curNextSkill].maxDistance &&
            m_Distance > battleSkills[farCounter[0]].minDistance)
        {
            float tempFloat = UnityEngine.Random.Range(0f, 1f);
            int tempInt = UnityEngine.Random.Range(0, farCounter.Length);
            tempInt = farCounter[tempInt];
            if (tempFloat < 
                Time.deltaTime * 
                battleSkills[tempInt].posibility / 
                battleSkills[tempInt].defaultPosibility ) // geometric distribution E[X] = 1/p // times 1 / next expected happen time
            {
                curNextSkill = tempInt;
            }
        }
    }

    // ===========================

    public void EnemyHittedPlayer()
    {
        // can be called in animation event to add reward for player, like heal or stamina regen, based on the damageData and current state
        if (_playerMain._anim_enemySkillIndex >= 2 && 
            battleSkills[_playerMain._anim_enemySkillIndex].posibility - 10f >= battleSkills[_playerMain._anim_enemySkillIndex].minPosibility) // if it is an attack skill and the posibility is above min, decrease posibility for next time
        {
            battleSkills[_playerMain._anim_enemySkillIndex].posibility -= 10f;
        }
    }

    public void PlayerHittedEnemy()
    {
        // can be called in animation event to add reward for player, like heal or stamina regen, based on the damageData and current state
        if (_playerMain._anim_enemySkillIndex >= 2 && 
            battleSkills[_playerMain._anim_enemySkillIndex].posibility + 30f <= battleSkills[_playerMain._anim_enemySkillIndex].maxPosibility) // if it is an attack skill and the posibility is above min, decrease posibility for next time
        {
            battleSkills[_playerMain._anim_enemySkillIndex].posibility += 30f;
        }
    }

    private void Awake()
    {
        // 在 Awake 时记录初始位置
        _startPosition = transform.position;
        _startRotation = transform.rotation;
        
        AssignAnimationIDs();
    }
    
    // 核心复活/回血函数
    public void ResetEnemy()
    {
        // =========================================================
        // 【新增逻辑】：判定是否允许复活
        // 如果是 Boss (shouldRespawn = false) 且已经死了，则彻底跳过重置
        // =========================================================
        if (!shouldRespawn && IsDead())
        {
            Debug.Log($"<color=gray>[ResetEnemy]</color> {gameObject.name} 是已击败的Boss，跳过复活。");
            return; 
        }

        // 1. 立即激活物体
        gameObject.SetActive(true);
        
        // 2. 开启协程，处理物理与状态重置
        StartCoroutine(ResetRoutine());
    }

    // 协程：等待引擎底层苏醒后再下刀
    private System.Collections.IEnumerator ResetRoutine()
    {
        // 【核心魔法】：等待当前帧所有的渲染和物理运算结束。
        // 这完美避开了 OnTriggerEnter 的物理冲突和 Animator 的装死判定！
        yield return new WaitForEndOfFrame();

        // 1. 关掉物理，解除锁定
        if (m_Agent != null) m_Agent.enabled = false;
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 2. 传送并强制同步物理引擎（非常重要！）
        transform.position = _startPosition;
        transform.rotation = _startRotation;
        Physics.SyncTransforms(); // 强行同步物理坐标

        // 3. 恢复物理
        if (cc != null) cc.enabled = true;
        if (m_Agent != null) m_Agent.enabled = true;

        // 4. 洗清所有的内鬼数据
        if (_playerMain != null)
        {
            _playerMain.nextStateID = 0;
            _playerMain._anim_enemySkillIndex = 0;
            _playerMain._anim_flag = false;
            curNextSkill = 0;

            if (_playerMain._combatSystem != null)
            {
                // 我们直接在这里暴力重置 CombatSystem，防止漏掉任何数值
                _playerMain._combatSystem.currentHealth = 1000f; 
                _playerMain._combatSystem.currentFocus = 125f;
                _playerMain._combatSystem.currentStamina = 100f;
                _playerMain._combatSystem.currentPoise = 19f;
                _playerMain._combatSystem.curKB_getHit = 0;
                _playerMain._combatSystem.curKB_getHitType = 1;
                
                // 强行清空伤害和编辑队列，拔除致死残留！
                _playerMain._combatSystem.currentPendingDamageDataList.Clear();
                _playerMain._combatSystem.currentPendingVEList.Clear();
            }

            if (_playerMain._animator != null)
            {
                _playerMain._animator.enabled = true;
                _playerMain._animator.Rebind();
                _playerMain._animator.Update(0f);
                
                // 此时 Animator 已完全清醒，参数注入绝对有效
                _playerMain._animator.SetFloat(_animID_HP, 1000f);
                _playerMain._animator.SetBool(_animIDDeath, false);
                _playerMain._animator.SetInteger(_animIDNextState, 0);
                _playerMain._animator.SetInteger(_animID_getHit, 0);

                // 霸王硬上弓：播放 Idle
                _playerMain._animator.Play("Idle Walk Run Blend", 0, 0f); 
            }
        }

        Debug.Log($"<color=green>[ResetEnemy]</color> {gameObject.name} 协程重置完毕，物理与动画已强制归位！");
    }

    public bool IsDead()
    {
        // 1. 【最高优先级判定】：查阅“生死簿”
        // 如果它是不重生的 Boss，并且它的名字已经在存档的击杀名单里，那它就是绝对死亡！
        if (!shouldRespawn && SaveManager.Instance != null)
        {
            if (SaveManager.Instance.CurrentSaveData.defeatedBossNames.Contains(gameObject.name))
            {
                return true; 
            }
        }

        // 2. 常规战斗血量判定
        if (_playerMain != null && _playerMain._combatSystem != null)
        {
            return _playerMain._combatSystem.currentHealth <= 0;
        }

        // 3. 保底状态判定
        return !gameObject.activeInHierarchy;
    }

    private void Start()
    {
        _playerMain = GetComponent<SkeletonSwordMain_A0>(); // 获取主脚本引用
        
        // 【核心修复】：生成时确保它没有父物体 (null)
        if (bossHookPrefab != null)
        {
            activeBossHook = Instantiate(bossHookPrefab, Vector3.zero, Quaternion.identity, null);
            activeBossHook.transform.localScale = Vector3.one * hookScale; // 应用 50 倍缩放
            activeBossHook.SetActive(false);
        }
        
        if (bossLR != null)
        {
            bossLR.positionCount = 2;
            bossLR.enabled = false;
        }
        AssignAnimationIDs();
    }

    // --- 动画事件调用函数 ---
    public void AnimEvent_BossShootHook()
    {
        if (Target == null) return;
        
        // 瞄准玩家位置（可以加个 Y 轴偏移瞄准胸口）
        Vector3 targetPos = Target.position;
        StopAllCoroutines(); // 防止连发冲突
        StartCoroutine(BossHookRoutine(targetPos));
    }

    private IEnumerator BossHookRoutine(Vector3 targetPos)
    {
        if (bossLR == null || activeBossHook == null) yield break;

        activeBossHook.SetActive(true);
        bossLR.enabled = true;
        Vector3 startPos = bossGunTip.position; // 绳子起点固定在手部
        float timer = 0f;

        while (timer < flyOutDuration)
        {
            timer += Time.deltaTime;
            float t = timer / flyOutDuration;
            
            // 使用 Lerp 确保位置严格处于起点和终点之间
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

            // 更新绳索：两点必须完全对齐
            bossLR.SetPosition(0, bossGunTip.position);
            bossLR.SetPosition(1, currentPos);

            // 更新钩爪位置
            activeBossHook.transform.position = currentPos;

            // 【核心修复】：重新对齐旋转逻辑
            // 先让 Z 轴指向目标，再叠加你自定义的角度偏移
            Vector3 direction = (targetPos - startPos).normalized;
            if (direction != Vector3.zero)
            {
                activeBossHook.transform.rotation = Quaternion.LookRotation(direction);
                // 应用面板里的 hookRotationOffset
                activeBossHook.transform.Rotate(hookRotationOffset, Space.Self);
            }

            yield return null;
        }

        // 3. 钉在目标位置停留
        activeBossHook.transform.position = targetPos;
        bossLR.SetPosition(1, targetPos);
        yield return new WaitForSeconds(stayDuration);

        // 4. 快速收回逻辑
        timer = 0f;
        float retractDuration = 0.2f;
        Vector3 lastPos = activeBossHook.transform.position;

        while (timer < retractDuration)
        {
            timer += Time.deltaTime;
            float t = timer / retractDuration;
            Vector3 currentPos = Vector3.Lerp(lastPos, bossGunTip.position, t);

            bossLR.SetPosition(0, bossGunTip.position);
            bossLR.SetPosition(1, currentPos);
            activeBossHook.transform.position = currentPos;
            yield return null;
        }

        // 5. 隐藏
        activeBossHook.SetActive(false);
        bossLR.enabled = false;
    }

    private void CheckHookHit(Vector3 pos)
    {
        // 这里可以写伤害逻辑
        // 如果玩家离 pos 很近，则判定被钩中
    }
}