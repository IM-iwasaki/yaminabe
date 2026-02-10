using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PveBossHpBarController : MonoBehaviour
{

    [Header("参照")]
    [SerializeField] private EnemyParameter enemyParameter;

    [Header("HP表示")]
    [SerializeField] private Slider hpSlider;

    [Header("名前表示")]
    [SerializeField] private TextMeshProUGUI nameText;

    private EnemyStatusBase enemyStatus;

    private void Start() {
        if (enemyParameter != null && hpSlider != null) {
            hpSlider.maxValue = enemyParameter.HP;
            hpSlider.value = enemyParameter.HP;
        }

        enemyStatus = GetComponent<EnemyStatusBase>();
        if (nameText == null) return;
        nameText.SetText(enemyStatus.statusData.name);

        HideBossUI();
    }

    /// <summary>
    /// HP更新（EnemyStatusBaseから呼ぶ）
    /// </summary>
    public void UpdateHP(int currentHP) {
        if (hpSlider != null) {
            hpSlider.value = currentHP;
        }
    }

    /// <summary>
    /// UIを表示させる
    /// </summary>
    public void ShowBossUI() {
        if(nameText == null || hpSlider == null) return;

        hpSlider.gameObject.SetActive(true);
        nameText.gameObject.SetActive(true);
    }

    /// <summary>
    /// UIを非表示させる
    /// </summary>
    public void HideBossUI() {
        if (nameText == null || hpSlider == null) return;

        hpSlider.gameObject.SetActive(false);
        nameText.gameObject.SetActive(false);
    }

}
