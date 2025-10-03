using UnityEngine;

// ゲーム全体の開始・終了を管理するマネージャー
// SystemObjectを継承

// ＜猿でもわかる使い方(ゲームの開始と終了)＞
// GameManager.Instance.StartGame();
// GameManager.Instance.EndGame();
public class GameManager : NetworkSystemObject<GameManager> {
    private bool isGameRunning = false;

    private GameTimer gameTimer;

    /// <summary>
    /// 初期化処理
    /// SystemManagerから呼ばれる
    /// </summary>
    public override void Initialize() {
        base.Initialize();
        gameTimer = GetComponent<GameTimer>();
        if (gameTimer == null) {
            gameTimer = gameObject.AddComponent<GameTimer>();
        }
    }

    /// <summary>
    /// ゲームを開始する
    /// </summary>
    public void StartGame() {
        if (isGameRunning) return;
        isGameRunning = true;
        // タイマー開始
        gameTimer.StartTimer();
        // ここでプレイヤーやマップ生成など

    }

    /// <summary>
    /// ゲームを終了する
    /// </summary>
    public void EndGame() {
        if (!isGameRunning) return;
        isGameRunning = false;
        // タイマー停止
        gameTimer.StopTimer();
        // リザルト表示など

    }

    /// <summary>
    /// ゲームが進行中かどうかを返す
    /// </summary>
    public bool IsGameRunning() {
        return isGameRunning;
    }
}