using UnityEngine;


// ゲーム全体の開始・終了と制限時間を管理するマネージャー
// SystemObjectを継承

// ＜猿でもわかる使い方＞
// GameManager.Instance.StartGame(); これでスタートを呼べる
// float remaining = GameManager.Instance.GetRemainingTime();  /// 時間取得
// uiText.text = $"残り時間: {remaining:F1} 秒";    /// UIに表示したい時はこんな感じ
public class GameManager : SystemObject<GameManager> {
    private bool isGameRunning = false;

    [Header("制限時間(秒)")]
    [SerializeField] private float limitTime = 180f; // 一応3分想定

    private float elapsedTime = 0f;

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
        elapsedTime = 0f;
        // プレイヤーとかマップとか生成するなら多分ここ

    }

    /// <summary>
    /// ゲームを終了する
    /// </summary>
    public void EndGame() {
        if (!isGameRunning) return;
        isGameRunning = false;      
        // リザルトなどはここ

    }

    /// <summary>
    /// 現在の経過時間を返す
    /// </summary>
    public float GetElapsedTime() {
        return elapsedTime;
    }

    /// <summary>
    /// 残り時間を返す
    /// </summary>
    public float GetRemainingTime() {
        return Mathf.Max(limitTime - elapsedTime, 0f);
    }

    /// <summary>
    /// ゲームが進行中かどうかを返す
    /// </summary>
    public bool IsGameRunning() {
        return isGameRunning;
    }

    private void Update() {
        if (!isGameRunning) return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= limitTime) {
            EndGame();
        }
    }
}