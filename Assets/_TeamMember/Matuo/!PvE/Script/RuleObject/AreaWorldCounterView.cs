using UnityEngine;
using TMPro;
using Mirror;
using System.Collections;

public class AreaWorldCounterView : MonoBehaviour {
    [SerializeField] private TMP_Text countText;

    private Camera mainCam;
    private CaptureAreaPVE area;

    [Header("ポップアニメーション")]
    [SerializeField] private float popScale = 1.25f;
    [SerializeField] private float popTime = 0.25f;
    [SerializeField]
    private AnimationCurve popCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine popCoroutine;
    private float lastScore = -1f;
    private int lastPlayerCount = -1;

    [SerializeField] private float disappearTime = 0.4f;
    [SerializeField]
    private AnimationCurve disappearCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Coroutine disappearCoroutine;

    private void Awake() {
        mainCam = Camera.main;
        area = GetComponentInParent<CaptureAreaPVE>();
    }

    private void LateUpdate() {
        if (area == null) return;

        // ビルボード処理
        if (mainCam != null) {
            Vector3 dir = transform.position - mainCam.transform.position;
            transform.rotation = Quaternion.LookRotation(dir);
        }

        UpdateText();
    }

    private void UpdateText() {
        if (countText == null) return;

        // 条件によってヘッダー変更
        string header;
        if (area.ClearCondition == AreaClearCondition.AllPlayers) {
            header = "<size=62%><b>Player</b></size>\n";
        } else {
            header = "<size=62%><b>Count</b></size>\n";
        }

        if (area.ClearCondition == AreaClearCondition.AllPlayers) {

            int current = area.CurrentPlayerCount;

            countText.text = header + current + " / " + area.MaxPlayerCount + " Player";

            // 色変更
            if (current >= area.MaxPlayerCount)
                countText.color = Color.green;
            else
                countText.color = Color.red;

            // 人数が変わったらポップ
            if (current != lastPlayerCount) {
                PlayPopEffect();
                lastPlayerCount = current;
            }
        } else {

            float current = Mathf.CeilToInt(area.CurrentScore);

            countText.text = header + current + " / " + Mathf.CeilToInt(area.TargetScore);

            countText.color = Color.white;

            // スコアが変わったらポップ
            if (current != lastScore) {
                PlayPopEffect();
                lastScore = current;
            }
        }
    }

    /// <summary>
    /// ポップアニメーション
    /// </summary>
    private void PlayPopEffect() {
        if (popCoroutine != null)
            StopCoroutine(popCoroutine);

        popCoroutine = StartCoroutine(PopAnimation());
    }

    private IEnumerator PopAnimation() {
        Transform tf = countText.transform;

        Vector3 baseScale = Vector3.one;
        Vector3 peakScale = Vector3.one * popScale;

        float t = 0f;
        while (t < popTime) {
            tf.localScale = Vector3.Lerp(baseScale, peakScale, popCurve.Evaluate(t / popTime));

            t += Time.deltaTime;
            yield return null;
        }

        tf.localScale = baseScale;
    }

    /// <summary>
    /// テキストリフレッシュ
    /// </summary>
    public void Refresh() {
        UpdateText();
    }

    /// <summary>
    /// UI隠す処理
    /// </summary>
    public void PlayDisappear() {
        if (disappearCoroutine != null)
            StopCoroutine(disappearCoroutine);

        disappearCoroutine = StartCoroutine(DisappearAnimation());
    }

    private IEnumerator DisappearAnimation() {
        Transform tf = countText.transform;

        Vector3 startScale = tf.localScale;
        float t = 0f;

        while (t < disappearTime) {
            float eval = disappearCurve.Evaluate(t / disappearTime);

            tf.localScale = startScale * eval;

            // フェードも入れる
            Color c = countText.color;
            c.a = eval;
            countText.color = c;

            t += Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}