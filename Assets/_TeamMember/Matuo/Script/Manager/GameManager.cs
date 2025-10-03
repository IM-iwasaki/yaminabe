using UnityEngine;

/// <summary>
/// ゲーム全体の開始と終了を管理するマネージャー
/// SystemObjectを継承してるお
/// </summary>
public class GameManager : SystemObject<GameManager> {
    private bool isGameRunning = false;

    /// <summary>
    /// 初期化処理
    /// SystemManagerから呼ばれる
    /// </summary>
    public override void Initialize() {
        base.Initialize();
    }

    /// <summary>
    /// ゲームを開始する
    /// </summary>
    public void StartGame() {
        if (isGameRunning) return;
        isGameRunning = true;
        // マップとかプレイヤー生成するならここでヨロ

    }

    /// <summary>
    /// ゲームを終了する
    /// </summary>
    public void EndGame() {
        if (!isGameRunning) return;
        isGameRunning = false;
        // リザルト画面とかはここで

    }

    /// <summary>
    /// ゲームが進行中かどうかを返す
    /// </summary>
    public bool IsGameRunning() {
        return isGameRunning;
    }
}