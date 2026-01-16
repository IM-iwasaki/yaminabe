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

    public static bool IsResultShowing { get; private set; }

    // チームスコアのための配列
    [System.Serializable]
    public struct TeamScoreEntry {
        public int teamId;
        public float teamScore;
    }

    /// <summary>
    /// リザルトに必要なすべてのデータをまとめて送る構造体
    /// ・勝者名
    /// ・チーム戦かどうか
    /// ・個人スコア
    /// ・どのルールのリザルトか
    /// ・ルール別の追加情報（チームスコア、ホコの距離など）
    /// </summary>
    [System.Serializable]
    public struct ResultData {

        // --- 基本データ ---
        public bool isTeamBattle;         // チーム戦かどうか
        public string winnerName;         // 勝利チーム名
        public ResultScoreData[] scores;  //スコア一覧
        public GameRuleType rule;         // Area / Hoko / DeathMatch を判別
        /// <summary>
        /// チームのスコア（エリア・ホコ・デスマッチ兼用）
        /// Key = チームID, Value = 獲得スコア
        /// </summary>
        public TeamScoreEntry[] teamScores;
    }



    // --------------------------
    // RuleManager から呼ばれる
    // リザルトのデータを全てリザルトに送る
    // --------------------------
    [Server]
    public void ShowTeamResult(ResultData data) {
        // 個人スコアを PlayerListManager から取得して追加する
        var scoreList = PlayerListManager.Instance.GetResultDataList();
        data.scores = scoreList.ToArray();

        // 通常の結果表示を実行
        ShowResult(data);
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
        if (currentUIRoot != null)
            return;

        // ===== リザルト中フラグON =====
        IsResultShowing = true;

        // カーソル表示
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
        // -----------------------------
        // ① 勝敗表示
        // -----------------------------
        if (currentResultPanel != null) {
            currentResultPanel.ShowWinner(data.winnerName, data.isTeamBattle);

            // ルール別UIを切替
            currentResultPanel.ShowRuleUI(data.rule);
        }

        // -----------------------------
        // ② 個人スコア一覧表示
        // -----------------------------
        ScoreListUI ui = FindObjectOfType<ScoreListUI>();
        if (ui != null)
            ui.DisplayScores(new List<ResultScoreData>(data.scores));
        else
            Debug.LogWarning("[ResultManager] ScoreListUIが見つかりません。");

        // チームIDで明示的に取得
        float redScore = 0f;
        float blueScore = 0f;

        if (data.teamScores != null) {
            foreach (var entry in data.teamScores) {
                if (entry.teamId == 0) redScore = entry.teamScore;
                else if (entry.teamId == 1) blueScore = entry.teamScore;
            }
        }

        // ---- ルール別処理 ----
        switch (data.rule) {
            case GameRuleType.Area:
                currentResultPanel.SetAreaScores(redScore, blueScore);
                break;

            case GameRuleType.Hoko:
                currentResultPanel.SetHokoScores(redScore, blueScore);
                break;

            case GameRuleType.DeathMatch:
                currentResultPanel.SetDeathMatchScores((int)redScore, (int)blueScore);
                break;
        }
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

            // ===== リザルト中フラグOFF =====
            IsResultShowing = false;

            // カーソルを元に戻す
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
