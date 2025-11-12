using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Mirror対応：リザルト画面の生成・勝敗・スコア送信を統括管理するクラス。
/// 
/// ・サーバーが ResultData を全クライアントに送信
/// ・クライアント側でリザルトUIを生成して表示
/// ・ResultPanel（勝敗）＋ScoreListUI（スコアリスト）を同時に扱う
/// </summary>
public class ResultManager : NetworkSystemObject<ResultManager> {
    [Header("リザルトUIプレハブ（Canvas付き）")]
    [SerializeField] private GameObject resultUIPrefab; // リザルト画面全体プレハブ

    private GameObject currentUIRoot;    // 現在のUIルート（生成後のCanvas）
    private ResultPanel currentResultPanel; // 勝敗パネル参照

    // --- 勝敗＋スコア情報を一時保持する ---
    private string cachedWinnerName;
    private bool cachedIsTeamBattle;
    private List<ResultScoreData> cachedScores = new();


    /// <summary>
    /// 勝敗＋スコアをまとめて送信する構造体
    /// </summary>
    [System.Serializable]
    public struct ResultData {
        public bool isTeamBattle;            // チーム戦かどうか
        public string winnerName;            // 勝者 or 勝利チーム名
        public ResultScoreData[] scores;     // スコア一覧
    }

    // --------------------------
    // RuleManager から呼ばれる
    //勝敗名前スコアをまとめてリザルトを表示する
    // --------------------------
    [Server]
    public void OnTeamResultReceived(string winnerName, bool isTeamBattle) {
        cachedWinnerName = winnerName;
        cachedIsTeamBattle = isTeamBattle;

        // PlayerListManager から個人スコア取得
        if (PlayerListManager.Instance != null)
            cachedScores = PlayerListManager.Instance.GetResultDataList();

        // まとめてリザルト表示
        var resultData = new ResultData {
            isTeamBattle = cachedIsTeamBattle,
            winnerName = cachedWinnerName,
            scores = cachedScores.ToArray()
        };

        ShowResult(resultData);
    }





    //================================================================
    // サーバー側：UI生成とスコア送信
    //================================================================

    /// <summary>
    /// サーバーがゲーム終了時に呼び出す。
    /// 全クライアントへ勝敗・スコアデータを送信してリザルトUIを表示。
    /// </summary>
    [Server]
    public void ShowResult(ResultData data) {
        StartCoroutine(ShowResultCoroutine(data));
    }

    /// <summary>
    /// 1フレーム待機後にUI生成→スコア表示を行う安全処理。
    /// Mirrorの同期タイミングを考慮。
    /// </summary>
    private IEnumerator ShowResultCoroutine(ResultData data) {
        RpcSpawnResultPanel();             // 各クライアントでUI生成
        yield return new WaitForEndOfFrame();
        RpcDisplayResult(data);            // 勝敗＆スコア表示
    }

    //================================================================
    // クライアント側：UI生成・表示
    //================================================================

    /// <summary>
    /// 各クライアントでリザルトUIプレハブを生成。
    /// </summary>
    [ClientRpc]
    private void RpcSpawnResultPanel() {
        // 既にUIが存在する場合はスキップ
        if (currentUIRoot != null) {
            return;
        }

        // プレハブを生成
        GameObject ui = Instantiate(resultUIPrefab);
        currentUIRoot = ui;

        // ResultPanelコンポーネント取得
        currentResultPanel = ui.GetComponentInChildren<ResultPanel>();
        if (currentResultPanel == null) {
            Debug.LogError("[ResultManager] ResultPanelがプレハブに見つかりません！");
            return;
        }

        // 勝敗パネルを表示
        currentResultPanel.RpcShowResult();
    
    }

    /// <summary>
    /// 各クライアントで勝敗とスコアをUIに表示。
    /// </summary>
    [ClientRpc]
    private void RpcDisplayResult(ResultData data) {
       

        // 勝敗表示
        if (currentResultPanel != null)
            currentResultPanel.ShowWinner(data.winnerName, data.isTeamBattle);

        // スコア一覧表示
        ScoreListUI ui = FindObjectOfType<ScoreListUI>();
        if (ui != null)
            ui.DisplayScores(new List<ResultScoreData>(data.scores));
        else
            Debug.LogWarning("[ResultManager] ScoreListUIが見つかりません。");
    }

    //================================================================
    // リザルト削除
    //================================================================

    /// <summary>
    /// リザルトUIを削除（再戦・ロビー戻り時など）
    /// </summary>
    public void HideResult() {
        if (currentUIRoot != null) {
            Destroy(currentUIRoot);
            currentUIRoot = null;
            currentResultPanel = null;
           
        }
    }
}
