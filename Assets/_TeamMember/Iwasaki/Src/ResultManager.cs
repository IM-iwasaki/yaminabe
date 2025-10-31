using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Mirror対応：リザルト画面の生成・スコア送信・削除を統括管理する
/// 
/// ・サーバーから全員にUIを生成（ClientRpc）
/// ・スコアもサーバーから全員に送信
/// ・UI側（ScoreListUI）はネットワーク登録不要
/// 
/// 【使い方】
/// resultManager.ShowResultWithScores(scores);
/// </summary>
public class ResultManager : NetworkBehaviour {
    [Header("リザルトUIプレハブ（Canvas付き）")]
    [SerializeField] private GameObject resultUIPrefab;

    private GameObject currentUIRoot;
    private ResultPanel currentResultPanel;

    // Mirrorで送信可能なデータ型
    [System.Serializable]
    public struct PlayerScoreData {
        public string playerName;
        public int score;
    }

    // ===============================================================
    // メイン処理
    // ===============================================================

    /// <summary>
    /// サーバーで呼ぶ：リザルトUIを全員に生成してスコアを送信
    /// </summary>
    [Server]
    public void ShowResultWithScores(PlayerScoreData[] scores) {
        StartCoroutine(ShowResultCoroutine(scores));
    }

    /// <summary>
    /// UI生成とスコア送信を安全に行うコルーチン
    /// </summary>
    private IEnumerator ShowResultCoroutine(PlayerScoreData[] scores) {
        // UIを全クライアントに生成
        RpcSpawnResultPanel();

        // 生成完了を待つ
        yield return new WaitForEndOfFrame();

        // スコアを全クライアントに送信
        RpcDisplayScores(scores);
    }

    // ===============================================================
    // クライアントRPC
    // ===============================================================

    /// <summary>
    /// 各クライアントでリザルトUIを生成
    /// </summary>
    [ClientRpc]
    private void RpcSpawnResultPanel() {
        if (currentUIRoot != null) {
            Debug.Log("[ResultManager] 既にリザルトUIが存在します。");
            return;
        }

        GameObject ui = Instantiate(resultUIPrefab);
        currentUIRoot = ui;

        currentResultPanel = ui.GetComponentInChildren<ResultPanel>();
        if (currentResultPanel == null) {
            Debug.LogError("[ResultManager] ResultPanel がプレハブに存在しません！");
            return;
        }

        currentResultPanel.RpcShowResult();

        Debug.Log("[ResultManager] リザルトUI生成完了。");
    }

    /// <summary>
    /// 全クライアントにスコア一覧を送信し、ローカルUIに反映
    /// </summary>
    [ClientRpc]
    private void RpcDisplayScores(PlayerScoreData[] scores) {
        Debug.Log($"[ResultManager] RpcDisplayScores 呼び出し。count={scores.Length}");

        ScoreListUI scoreList = FindObjectOfType<ScoreListUI>();
        if (scoreList == null) {
            Debug.LogWarning("[ResultManager] ScoreListUI が見つかりません。UI生成順を確認してください。");
            return;
        }

        // 配列をListに変換して渡す
        var list = new List<ScoreListUI.PlayerScoreData>();
        foreach (var s in scores) {
            list.Add(new ScoreListUI.PlayerScoreData {
                playerName = s.playerName,
                score = s.score
            });
        }

        scoreList.DisplayScores(list);
    }

    // ===============================================================
    // UI削除処理
    // ===============================================================

    public void HideResult() {
        if (currentUIRoot != null) {
            Destroy(currentUIRoot);
            currentUIRoot = null;
            currentResultPanel = null;
            Debug.Log("[ResultManager] リザルト画面を削除しました。");
        }
    }
}
