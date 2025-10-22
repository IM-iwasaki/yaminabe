using UnityEngine;
using Mirror;
using System;

/// <summary>
/// 制限時間管理
/// GameManagerと連携して使用
/// </summary>
public class GameTimer : NetworkBehaviour {
    [Header("制限時間(秒)")]
    [SerializeField] private float limitTime = 180f; // (仮で3分想定)

    [SyncVar] private float elapsedTime = 0f; // サーバーとクライアントで同期
    private bool isRunning = false;

    /// <summary>
    /// タイマー終了時に発火するイベント
    /// </summary>
    public event Action OnTimerFinished;

    /// <summary>
    /// タイマーを開始する (サーバー専用)
    /// </summary>
    [Server]
    public void StartTimer() {
        elapsedTime = 0f;
        isRunning = true;
    }

    /// <summary>
    /// タイマーを停止する (サーバー専用)
    /// </summary>
    [Server]
    public void StopTimer() {
        isRunning = false;
    }

    /// <summary>
    /// タイマーを一時停止または再開する (サーバー専用)
    /// </summary>
    /// <param name="running">trueで再開、falseで停止</param>
    [Server]
    public void SetRunning(bool running) {
        isRunning = running;
    }

    /// <summary>
    /// 経過時間を取得
    /// </summary>
    /// <returns>経過時間(秒)</returns>
    public float GetElapsedTime() {
        return elapsedTime;
    }

    /// <summary>
    /// 残り時間を取得
    /// </summary>
    /// <returns>残り時間(秒)</returns>
    public float GetRemainingTime() {
        return Mathf.Max(limitTime - elapsedTime, 0f);
    }

    /// <summary>
    /// サーバー側で毎フレームタイマーを更新
    /// 終了時にはイベント発火
    /// </summary>
    [ServerCallback]
    private void Update() {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= limitTime) {
            elapsedTime = limitTime;
            isRunning = false;

            // タイマー終了イベント
            OnTimerFinished?.Invoke();
        }
    }
}