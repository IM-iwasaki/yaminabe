using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EnemyHealthView : MonoBehaviour {

    [Header("参照")]
    [SerializeField] private EnemyParameter enemyParameter;

    [Header("HP表示")]
    [SerializeField] private Slider hpSlider;

    [Header("ダメージテキスト")]
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private Transform textSpawnPoint;

    private Camera mainCam;
    private float dmgTextTimer;
    private float dmgTextDuration = 0.7f;
    private Color nextDamageColor = Color.yellow;

    [Header("ダメージポップ設定")]
    [SerializeField] private float popScale = 1.6f;
    [SerializeField] private float popTime = 0.2f;
    [SerializeField]
    private AnimationCurve popCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine popCoroutine;

    private void Awake() {
        mainCam = Camera.main;

        if (damageText != null) {
            damageText.gameObject.SetActive(false);

            // 最初から親を設定しておく
            if (textSpawnPoint != null) {
                damageText.transform.SetParent(textSpawnPoint);
            }
        }
    }

    private void Start() {
        if (enemyParameter != null && hpSlider != null) {
            hpSlider.maxValue = enemyParameter.HP;
            hpSlider.value = enemyParameter.HP;
        }
    }

    private void LateUpdate() {
        // HPバーをカメラ方向へ
        if (hpSlider != null && mainCam != null) {
            hpSlider.transform.LookAt(mainCam.transform);
        }

        // ダメージテキスト更新
        if (damageText != null && damageText.gameObject.activeSelf) {
            if (mainCam != null) {
                Vector3 dir = damageText.transform.position - mainCam.transform.position;
                damageText.transform.rotation = Quaternion.LookRotation(dir);
            }

            // ワールドY方向に上昇させる
            damageText.transform.position += Vector3.up * Time.deltaTime;

            dmgTextTimer += Time.deltaTime;
            if (dmgTextTimer >= dmgTextDuration) {
                damageText.gameObject.SetActive(false);
            }
        }
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
    /// ダメージ表示
    /// </summary>
    public void ShowDamage(int damage) {
        if (damageText == null) return;

        // 必ず textSpawnPoint の子にする
        if (textSpawnPoint != null) {
            damageText.transform.SetParent(textSpawnPoint);
        }

        // 毎回ローカル位置をリセット
        damageText.transform.localPosition = Vector3.up * 0.9f;

        int displayDamage = damage * 10;
        damageText.text = displayDamage.ToString();

        damageText.color = nextDamageColor;
        damageText.gameObject.SetActive(true);

        dmgTextTimer = 0f;
        nextDamageColor = Color.yellow;

        PlayPopEffect();
    }

    public void SetNextDamageColor(Color color) {
        nextDamageColor = color;
    }

    private void PlayPopEffect() {
        if (popCoroutine != null)
            StopCoroutine(popCoroutine);

        popCoroutine = StartCoroutine(PopAnimation());
    }

    /// <summary>
    /// ポップアニメーション
    /// </summary>
    private IEnumerator PopAnimation() {
        Transform tf = damageText.transform;
        Vector3 baseScale = Vector3.one;
        Vector3 peakScale = Vector3.one * popScale;

        float t = 0f;

        while (t < popTime) {
            tf.localScale =
                Vector3.Lerp(baseScale, peakScale,
                popCurve.Evaluate(t / popTime));

            t += Time.deltaTime;
            yield return null;
        }

        tf.localScale = baseScale;
    }
}