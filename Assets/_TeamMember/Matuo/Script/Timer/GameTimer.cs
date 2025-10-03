using UnityEngine;

// 制限時間を管理するクラス
// GameManagerと連携して動作

// ＜猿でもわかる使い方(時間の取得とUI表示)＞
// float remaining = GameManager.Instance.GetComponent<GameTimer>().GetRemainingTime(); //現在時間取得
// uiText.text = $"残り時間: {remaining:F1} 秒";  // UIに表示
public class GameTimer : MonoBehaviour {
    [Header("制限時間(秒)")]
    [SerializeField] private float limitTime = 180f;

    private float elapsedTime = 0f;
    private bool isRunning = false;

    /// <summary>
    /// タイマーを開始する
    /// </summary>
    public void StartTimer() {
        elapsedTime = 0f;
        isRunning = true;
    }

    /// <summary>
    /// タイマーを停止する
    /// </summary>
    public void StopTimer() {
        isRunning = false;
    }

    /// <summary>
    /// 経過時間を返す
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

    private void Update() {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= limitTime) {
            // 時間切れになったらGameManagerに通知
            GameManager.Instance.EndGame();
        }
    }
}