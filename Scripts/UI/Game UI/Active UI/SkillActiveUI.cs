using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillActiveUI : MonoBehaviour
{
    [SerializeField]
    private Image _skillHolder;
    [SerializeField]
    private Image _skillTimer;

    /// <summary>
    /// _skillSprites[0]: inactive sprite (not ready to use)
    /// _skillSprites[1]: active sprite (ready to use)
    /// </summary>
    [SerializeField]
    private Sprite[] _skillSprites;

    [SerializeField]
    private int skillIndex;
    [SerializeField]
    private float _skillMaxCooldown;

    private float _skillCurrentCooldown;

    private int currentCharges = 0;
    private const int MAXCHARGES = 4;

    private bool _isSkillReady;

    // Start is called before the first frame update
    private void Start()
    {
        _skillCurrentCooldown = 0;
    }

    // Update is called once per frame
    private void Update()
    {
        _isSkillReady = (currentCharges <= MAXCHARGES && currentCharges > 0);
        _skillCurrentCooldown = _skillCurrentCooldown + Time.deltaTime;
        _skillTimer.fillAmount = Mathf.Min(_skillCurrentCooldown / _skillMaxCooldown, 1);

        if (_skillCurrentCooldown >= _skillMaxCooldown / MAXCHARGES * (currentCharges + 1))
        {
            //Debug.Log("Adding Charges");
            //Debug.Log(_isSkillReady);   
            currentCharges = Mathf.Min(currentCharges + 1, MAXCHARGES);
        }

        if (!_isSkillReady) _skillHolder.sprite = _skillSprites[0];
        else _skillHolder.sprite = _skillSprites[1];


        if (!_isSkillReady) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && skillIndex == 1)
        {
            currentCharges--;
            _skillCurrentCooldown -= _skillMaxCooldown / MAXCHARGES;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && skillIndex == 2)
        {
            currentCharges--;
            _skillCurrentCooldown -= _skillMaxCooldown / MAXCHARGES;

        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && skillIndex == 3)
        {
            currentCharges--;
            _skillCurrentCooldown -= _skillMaxCooldown / MAXCHARGES;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && skillIndex == 4)
        {
            currentCharges--;
            _skillCurrentCooldown -= _skillMaxCooldown / MAXCHARGES;
        }


    }
}
