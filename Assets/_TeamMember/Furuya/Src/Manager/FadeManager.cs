using UnityEngine;
using System.Collections;
using System;

public class FadeManager : SystemObject<FadeManager> {
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float defaultDuration = 1f;
    private Coroutine currentRoutine;

    public override void Initialize() {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    public void StartFadeOut(float duration = -1f, Action onComplete = null) {
        if (duration < 0f) duration = defaultDuration;
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeRoutine(0f, 1f, duration, onComplete));
    }

    public void StartFadeIn(float duration = -1f, Action onComplete = null) {
        if (duration < 0f) duration = defaultDuration;
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeRoutine(1f, 0f, duration, onComplete));
    }

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