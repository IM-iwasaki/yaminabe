using UnityEngine;
using UnityEngine.UI;
using Mirror;

/// <summary>
/// テスト用：スコア加算・リセットボタン
/// </summary>
public class ScoreTestButtons : MonoBehaviour {
    [Header("ボタン参照")]
    public Button addScoreButton;
    public Button resetScoreButton;

    private PlayerScoreData localPlayer;

    void Start() {
        // 自分のプレイヤーを取得（ちょっと遅れるのでコルーチンで探す）
        StartCoroutine(FindLocalPlayer());

        // ボタンイベント設定
        if (addScoreButton != null)
            addScoreButton.onClick.AddListener(OnAddScoreClicked);

        if (resetScoreButton != null)
            resetScoreButton.onClick.AddListener(OnResetScoreClicked);
    }

    System.Collections.IEnumerator FindLocalPlayer() {
        while (localPlayer == null) {
            foreach (var player in FindObjectsOfType<PlayerScoreData>()) {
                if (player.isLocalPlayer) {
                    localPlayer = player;
                    Debug.Log(" 自分のPlayerScoreDataを取得しました");
                    break;
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    /// <summary>
    /// スコア加算ボタン押下
    /// </summary>
    void OnAddScoreClicked() {
        if (localPlayer == null) {
            Debug.LogWarning(" プレイヤーがまだ見つかっていません");
            return;
        }

        // ランダムスコアを加算（コマンド経由でサーバー実行）
        int addValue = Random.Range(10, 100);
        localPlayer.CmdAddScore(addValue);
        Debug.Log($" スコアを {addValue} 加算リクエスト");
    }

    /// <summary>
    /// スコアリセットボタン押下
    /// </summary>
    void OnResetScoreClicked() {
        if (localPlayer == null) {
            Debug.LogWarning(" プレイヤーがまだ見つかっていません");
            return;
        }

        localPlayer.CmdResetScore();
        Debug.Log(" スコアをリセットリクエスト");
    }
}
