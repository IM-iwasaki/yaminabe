using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PVENotificationManager : MonoBehaviour
{
    public static PVENotificationManager Instance;

    [Header("テキスト")]
    [SerializeField] private TextMeshProUGUI text;

    [Header("通知を表示する時間")]
    [SerializeField] private float isVisibleTime = 3.0f;

    //  透明度を管理する
    private CanvasGroup cg;
    //  フェードインまでの時間
    private float fadeInTime = 0.5f;
    //  フェードアウトまでの時間
    private float fadeOutTime = 1.0f;

    //  実行中のコルーチンを保持しておく
    private Coroutine messageCoroutine = null;
    private Coroutine fadeCoroutine = null;

    private void Awake() {
        Instance = this;
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0;
    }

    /// <summary>
    /// 通知のテキストを受け取って表示する
    /// </summary>
    /// <param name="message"></param>
    public void SendNotificationMessage(string message) {
        text.SetText(message);
        //  コルーチンが既に動いているなら停止させる
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        //  コルーチンを開始させる
        messageCoroutine = StartCoroutine(IsVisibleMessage());
    }

    /// <summary>
    /// 実際に通知を表示させる
    /// </summary>
    /// <returns></returns>
    private IEnumerator IsVisibleMessage() {
        //  フェードインする
        StartFadeInMessage();
        //  表示時間分待つ
        yield return new WaitForSeconds(isVisibleTime);
        //  フェードアウトする
        StartFadeOutMessage();
    }

    /// <summary>
    /// フェードインを開始させる
    /// </summary>
    private void StartFadeInMessage() {
        //  コルーチンが既に動いているなら停止させる
        if(fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        //  コルーチンを開始させる
        fadeCoroutine = StartCoroutine(FadeRoutine(0f, 1f, fadeInTime));
    }
    /// <summary>
    /// フェードアウトを開始させる
    /// </summary>
    private void StartFadeOutMessage() {
        //  コルーチンが既に動いているなら停止させる
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        //  コルーチンを開始させる
        fadeCoroutine = StartCoroutine(FadeRoutine(1f, 0f, fadeOutTime));
    }

    /// <summary>
    /// 共通コルーチン
    /// </summary>
    /// <param name="startAlpha"></param>
    /// <param name="endAlpha"></param>
    /// <param name="fadeTime"></param>
    /// <returns></returns>
    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float fadeTime) {
        float t = 0f;           //  経過時間
        cg.alpha = startAlpha;  //  初期透明度

        //  指定時間まで回す
        while(t < fadeTime) {
            //  時間を加算
            t += Time.deltaTime;
            //  透明度を補間していく
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, t / fadeTime);

            yield return null;
        }

        cg.alpha = endAlpha;

        fadeCoroutine = null;
        messageCoroutine = null;
    }

}
