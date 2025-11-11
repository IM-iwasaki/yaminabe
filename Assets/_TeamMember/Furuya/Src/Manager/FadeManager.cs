using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 画面全体のフェードイン・フェードアウトを管理するクラス。
/// CanvasGroupを使ってアルファ値を補間し、シーン遷移時などの演出を制御する。
/// </summary>
public class FadeManager : SystemObject<FadeManager> {
    [Header("フェード制御用CanvasGroup")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("デフォルトのフェード時間（秒）")]
    [SerializeField] private float defaultDuration = 1f;

    private Coroutine currentRoutine;

    // ======================
    // --- 初期化 ---
    // ======================
    public override void Initialize() {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    // ======================
    // --- フェード制御 ---
    // ======================

    /// <summary>
    /// フェードアウトを開始する（暗転する）。
    /// </summary>
    /// <param name="duration">フェード時間（省略時はデフォルト）</param>
    /// <param name="onComplete">完了時コールバック</param>
    public void StartFadeOut(float duration = -1f, Action onComplete = null) {
        if (duration < 0f) duration = defaultDuration;
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeRoutine(0f, 1f, duration, onComplete));
    }

    /// <summary>
    /// フェードインを開始する（明転する）。
    /// </summary>
    /// <param name="duration">フェード時間（省略時はデフォルト）</param>
    /// <param name="onComplete">完了時コールバック</param>
    public void StartFadeIn(float duration = -1f, Action onComplete = null) {
        if (duration < 0f) duration = defaultDuration;
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeRoutine(1f, 0f, duration, onComplete));
    }

    // ======================
    // --- 内部処理 ---
    // ======================
    /// <summary>
    /// アルファ値を補間してフェードを実行する。
    /// </summary>
    private IEnumerator FadeRoutine(float from, float to, float duration, Action onComplete) {
        float t = 0f;
        while (t < duration) {
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        if (canvasGroup != null)
            canvasGroup.alpha = to;
        onComplete?.Invoke();
    }
}