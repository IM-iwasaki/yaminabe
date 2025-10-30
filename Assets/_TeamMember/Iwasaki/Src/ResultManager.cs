using UnityEngine;
using Mirror;
using System.Collections;

/// <summary>
/// リザルト画面の生成・スコア表示・削除を統括管理するクラス（Mirror対応）
/// 
/// ・サーバーが必要なタイミングでリザルトUIを生成（全クライアントに同期）
/// ・ScoreListUI にスコアを送信して一覧表示
/// ・HideResult() でプレハブ全体を安全に削除
/// ・Canvasを含むUIプレハブ全体を保持しておく構成
/// </summary>
public class ResultManager : NetworkBehaviour {
    [Header("リザルトUIプレハブ（Canvas含む）")]
    [SerializeField] private GameObject resultUIPrefab;

    // 生成されたUIのルート（Canvas付き）
    private GameObject currentUIRoot;
    // プレハブ内のResultPanel
    private ResultPanel currentResultPanel;
    // プレハブ内のScoreListUI
    private ScoreListUI currentScoreList;

    /// <summary>
    /// サーバーが呼ぶ：リザルト画面を全クライアントに表示してスコアを送信
    /// </summary>
    [Server]
    public void ShowResultWithScores(ScoreListUI.PlayerScoreData[] scores) {
        StartCoroutine(ShowResultCoroutine(scores));
    }

    /// <summary>
    /// UI生成とスコア送信を安全に行う
    /// </summary>
    private IEnumerator ShowResultCoroutine(ScoreListUI.PlayerScoreData[] scores) {
        // 全クライアントでUI生成
        RpcSpawnResultPanel();

        // 1フレーム待機（生成完了を待つ）
        yield return null;

        // スコア送信（生成されたUIから参照）
        if (currentScoreList == null)
            currentScoreList = FindObjectOfType<ScoreListUI>();

        if (currentScoreList != null) {
            currentScoreList.RpcDisplayScores(scores);
            Debug.Log("[ResultManager] スコア送信完了");
        }
        else {
            Debug.LogWarning("[ResultManager] ScoreListUI が見つかりません。プレハブに含まれているか確認してください。");
        }
    }

    /// <summary>
    /// 各クライアントでリザルトUIを生成
    /// </summary>
    [ClientRpc]
    private void RpcSpawnResultPanel() {
        // すでにUIが存在する場合は何もしない
        if (currentUIRoot != null) {
            Debug.Log("[ResultManager] すでにリザルト画面が存在します。");
            return;
        }

        // プレハブ生成
        GameObject ui = Instantiate(resultUIPrefab);
        currentUIRoot = ui; // リザルトプレハブを保持

        // 内部のパネルとスコアリストを参照
        currentResultPanel = ui.GetComponentInChildren<ResultPanel>();
        currentScoreList = ui.GetComponentInChildren<ScoreListUI>();

        if (currentResultPanel == null)
            Debug.LogError("[ResultManager] ResultPanel コンポーネントが見つかりません！");
        else
            currentResultPanel.RpcShowResult();

        Debug.Log("[ResultManager] リザルトUIを生成しました。");
    }

    /// <summary>
    /// 現在のリザルトUIを削除（ResultPanel＋ScoreListUI含めて全体破棄）
    /// </summary>
    public void HideResult() {
        if (currentUIRoot != null) {
            Destroy(currentUIRoot);
            Debug.Log("[ResultManager] リザルト画面を削除しました。");

            // 参照クリア
            currentUIRoot = null;
            currentResultPanel = null;
            currentScoreList = null;
        }
        else {
            Debug.LogWarning("[ResultManager] 削除しようとしたリザルトUIが存在しません。");
        }
    }
}
