using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Linq; // ← スコア集計用

/// <summary>
/// テスト用：ESCキーでリザルトを表示。
/// スコア・勝者名は今後実データに差し替え可能な構成。
/// </summary>
public class ResultDebug : NetworkBehaviour {
    private ResultManager resultManager;
    private bool toggle = false;

    void Start() {
        resultManager = FindObjectOfType<ResultManager>();
    }

    void Update() {
      
    }

    //===========================================================
    // ▼ プレイヤー情報取得（後で差し替え可能）
    //===========================================================

    /// <summary>
    /// 全プレイヤーのスコア情報を取得する。
    /// 現在は仮データを返すが、将来的に実データに置き換え予定。
    /// </summary>
    private List<ResultScoreData> GetAllPlayerScores() {
        // --- 仮データ ---
        List<ResultScoreData> scores = new List<ResultScoreData>()
        {
            new ResultScoreData { PlayerName = "Alic---",   Score = 1200 },
            new ResultScoreData { PlayerName = "Bob-n",     Score = 800 },
            new ResultScoreData { PlayerName = "Charlie", Score = 1500 },
            new ResultScoreData { PlayerName = "Delta",   Score = 600 },
            new ResultScoreData { PlayerName = "Boss",    Score=30},
            new ResultScoreData { PlayerName = "God" ,    Score=2},
        };
       
        

        return scores;
    }

    /// <summary>
    /// スコアリストから勝者（または勝利チーム）を決定する。
    /// 現在は仮の処理（最高スコア者 or "Red"）。
    /// </summary>
    private string GetWinnerName(List<ResultScoreData> scores, bool isTeamBattle) {
        if (isTeamBattle) {
            // 仮：Redチーム勝利固定（あとでチームスコア比較に変更予定）
            return "Red";
        }
        else {
            // 仮：スコア最大のプレイヤーをWinnerとする
            var topPlayer = scores.OrderByDescending(s => s.Score).FirstOrDefault();
            return topPlayer.PlayerName ?? "???";
        }
    }

    //===========================================================
    // ▼ テスト表示
    //===========================================================

    private void ShowIndividualBattle() {
        var scores = GetAllPlayerScores();                  // 全スコア取得
        var winner = GetWinnerName(scores, false);          // 勝者判定

        var data = new ResultManager.ResultData {
            isTeamBattle = false,
            winnerName = winner,                            // ← 関数から取得
            scores = scores.ToArray()
        };

        resultManager.ShowResult(data);
    }

    private void ShowTeamBattle() {
        Debug.Log("[ResultDebug] チーム戦リザルト表示");

        var scores = GetAllPlayerScores();
        var winner = GetWinnerName(scores, true);           // ← チーム戦版

        var data = new ResultManager.ResultData {
            isTeamBattle = true,
            winnerName = winner,
            scores = scores.ToArray()
        };

        resultManager.ShowResult(data);
    }
}
