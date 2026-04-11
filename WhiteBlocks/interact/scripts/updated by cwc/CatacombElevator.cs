using UnityEngine;
using System.Collections.Generic;
using System;

public class CatacombElevator : MonoBehaviour
{
    [Header("按鈕設定")]
    public Transform redButton; // 拖入 Button_Red
    [Tooltip("按鈕被踩下時，往下凹陷的距離")]
    public float buttonPressDepth = 0.05f; 
    public float buttonSpeed = 5f;

    [Header("電梯設定")]
    [Tooltip("電梯要上升的總高度")]
    public float[] liftRiseHeight = { 10f, 0f }; // from top to bottom
    public int[] liftNextMoveIndex = { 1, 0 }; // 定义每个移动模式下，liftMoveSequence 的下一步索引
    public int currentLiftIndex = 1; // 当前电梯所在位置的索引（0表示在底部，1表示在顶部）
    public float liftSpeed = 2f;
    
    [Tooltip("離開壓力板時，電梯會自動降下來嗎？")]
    public bool autoReturnToBottom = false;

    // 紀錄座標
    private Vector3 buttonStartLocalPos;
    private Vector3 buttonPressedLocalPos;
    private Vector3 liftStartPos;
    private Vector3 liftTargetPos;

    // 狀態開關
    private CatacombGButton _gButton;
    private bool isPressed = false;
    private bool canResetNextIndex = true;

    void Start()
    {
        // 1. 初始化按鈕的位置 (使用 localPosition，這樣按鈕才會相對電梯移動)
        if (redButton != null)
        {
            buttonStartLocalPos = redButton.localPosition;
            buttonPressedLocalPos = buttonStartLocalPos - new Vector3(0, buttonPressDepth, 0);
        }
        if (_gButton == null)
        {
            _gButton = GetComponentInChildren<CatacombGButton>(); // 嘗試在子物件中找到 CatacombGButton
        }

        // 2. 初始化電梯的位置
        liftStartPos = transform.position;

        // 3. 根據 liftMoveSequence 的第一個步驟來設定初始的 liftTargetPos
        currentLiftIndex = liftNextMoveIndex[currentLiftIndex]; // 根据当前索引获取下一个索引
        liftTargetPos = liftStartPos + new Vector3(0, liftRiseHeight[currentLiftIndex], 0);
    }

    void Update()
    {
        // --- 處理紅色按鈕的凹凸動畫 ---
        if (redButton != null)
        {
            if (_gButton != null) // 从 CatacombGButton 获取当前的按压状态
            {
                isPressed = _gButton.isPressed ? true : isPressed;
                if (Vector3.Distance(transform.position, liftTargetPos) < 0.01f)
                {
                    isPressed = _gButton.isPressed;
                }
            }

            Vector3 targetBtnPos = isPressed ? buttonPressedLocalPos : buttonStartLocalPos;
            redButton.localPosition = Vector3.MoveTowards(redButton.localPosition, targetBtnPos, buttonSpeed * Time.deltaTime);
        }

        // --- 根據按鈕狀態決定電梯的目標位置 ---
        if (_gButton == null && Vector3.Distance(transform.position, liftTargetPos) < 0.01f) // 如果没有找到 CatacombGButton，就直接根据当前位置判断是否需要切换目标位置
        {
            isPressed = false; // 没有按钮控制时，默认不按下
        }
        if (!isPressed && !canResetNextIndex)
        {
            currentLiftIndex = liftNextMoveIndex[currentLiftIndex];
            liftTargetPos = liftStartPos + new Vector3(0, liftRiseHeight[currentLiftIndex], 0);
        }
        canResetNextIndex = !isPressed; // 当按钮被按下时，禁止重置索引；当按钮未被按下时，允许重置索引

        // --- 處理電梯的升降動畫 ---
        if (isPressed)
        transform.position = Vector3.MoveTowards(transform.position, liftTargetPos, liftSpeed * Time.deltaTime);
    }

    public void CommandFromOtherScript(int targetIndex)
    {
        currentLiftIndex = targetIndex;
        liftTargetPos = liftStartPos + new Vector3(0, liftRiseHeight[currentLiftIndex], 0);
        isPressed = true; // 直接设置为按下状态，触发电梯上升
    }

    // --- 玩家踩上電梯 ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 將玩家設為電梯的子物件，防止玩家在電梯上升時掉下去或瘋狂抖動
            other.transform.SetParent(transform);
            
            Debug.Log("【電梯】玩家踩上電梯！");
        }
    }

    // --- 玩家離開電梯 ---
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 解除玩家與電梯的父子關係
            other.transform.SetParent(null);
        }
    }
}