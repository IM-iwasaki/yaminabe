using UnityEngine;
using Mirror;

/// <summary>
/// 制限時間を管理するクラス
/// GameManagerと連携して動作する
/// <summary>

// ＜猿でもわかる使い方(時間の取得とUI表示)＞
// float remaining = GameManager.Instance.GetComponent<GameTimer>().GetRemainingTime(); //現在時間取得
// uiText.text = $"残り時間: {remaining:F1} 秒";  // UIに表示
public class GameTimer : NetworkBehaviour {
    [Header("制限時間(秒)")]
    [SerializeField] private float limitTime = 180f; // 一旦3分想定

    [SyncVar] private float elapsedTime = 0f; // サーバーとクライアントで同期される経過時間
    private bool isRunning = false;

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
    /// 経過時間を取得
    /// </summary>
    public float GetElapsedTime() {
        return elapsedTime;
    }

    /// <summary>
    /// 残り時間を取得
    /// </summary>
    public float GetRemainingTime() {
        return Mathf.Max(limitTime - elapsedTime, 0f);
    }

    /// <summary>
    /// サーバー側で毎フレームタイマーを更新
    /// 時間切れ時にはGameManagerに通知
    /// </summary>
    [ServerCallback]
    private void Update() {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= limitTime) {
            isRunning = false;
            GameManager.Instance?.EndGame();
        }
    }
}