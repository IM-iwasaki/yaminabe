using UnityEngine;
using Mirror;

/// <summary>
/// ゲーム全体の開始・終了を管理するマネージャー
/// NetworkSystemObjectを継承
/// <summary>

// ＜猿でもわかる使い方(ゲームの開始と終了)＞ 
// GameManager.Instance.StartGame(); 
// GameManager.Instance.EndGame();
public class GameManager : NetworkSystemObject<GameManager> {
    [SyncVar]
    private bool isGameRunning = false; // サーバーとクライアントで同期されるゲーム進行状態

    private GameTimer gameTimer;

    /// <summary>
    /// 初期化処理
    /// SystemManagerから呼ばれる
    /// </summary>
    public override void Initialize() {
        base.Initialize();

        // GameTimerコンポーネントを取得、無ければ追加
        gameTimer = GetComponent<GameTimer>();
        if (gameTimer == null) {
            gameTimer = gameObject.AddComponent<GameTimer>();
        }
    }

    /// <summary>
    /// ゲームを開始する (サーバー専用)
    /// </summary>
    [Server]
    public void StartGame() {
        if (isGameRunning) return;

        isGameRunning = true;

        // タイマー開始
        gameTimer.StartTimer();

        // マップ生成などはここで

    }

    /// <summary>
    /// ゲームを終了する (サーバー専用)
    /// </summary>
    [Server]
    public void EndGame() {
        if (!isGameRunning) return;

        isGameRunning = false;

        // タイマー停止
        gameTimer.StopTimer();

        // リザルト表示などはここで

    }

    /// <summary>
    /// ゲームが進行中かどうかを返す (サーバーとクライアントで同期)
    /// </summary>
    public bool IsGameRunning() {
        return isGameRunning;
    }

    /// <summary>
    /// 残り時間を取得 (クライアントも参照可能)
    /// </summary>
    public float GetRemainingTime() {
        return gameTimer != null ? gameTimer.GetRemainingTime() : 0f;
    }
}