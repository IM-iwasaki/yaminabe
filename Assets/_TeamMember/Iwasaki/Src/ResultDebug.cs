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
        if (Input.GetKeyUp(KeyCode.Escape) && isServer) {
            toggle = !toggle;
            if (toggle) ShowIndividualBattle();
            else ShowTeamBattle();
        }
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
            new ResultScoreData { playerName = "Alic---",   score = 1200 },
            new ResultScoreData { playerName = "Bob-n",     score = 800 },
            new ResultScoreData { playerName = "Charlie", score = 1500 },
            new ResultScoreData { playerName = "Delta",   score = 600 },
            new ResultScoreData { playerName = "Boss",    score=30},
            new ResultScoreData { playerName = "God" ,    score=2},
        };
        /// <summary>
        /// 全プレイヤーのスコア情報を取得する
        /// （CharacterBaseを継承しているGeneralCharacterから取得）
        /// </summary>
           // List<ResultScoreData> scores = new List<ResultScoreData>();
           //
           // // --- サーバーでのみ実行（Mirrorの正しい設計） ---
           // if (!isServer) {
           //     Debug.LogWarning("スコア集計はサーバーでのみ実行されます。");
           //     return scores;
           // }
           //
           // // --- シーン上の全 GeneralCharacter を探索 ---
           // foreach (var c in FindObjectsOfType<GeneralCharacter>()) {
           //     // CharacterBaseを継承しているのでPlayerNameとScoreを直接参照できる
           //     scores.Add(new ResultScoreData {
           //         playerName = c.PlayerName, // ← CharacterBase側の名前変数
           //         score = c.score            // ← CharacterBase側のスコア変数
           //     });
           //
           //     Debug.Log($"[ResultDebug] スコア取得: {c.PlayerName} = {c.score}");
           // }
           //
           // return scores;
        

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
            var topPlayer = scores.OrderByDescending(s => s.score).FirstOrDefault();
            return topPlayer.playerName ?? "???";
        }
    }

    //===========================================================
    // ▼ テスト表示
    //===========================================================

    private void ShowIndividualBattle() {
        Debug.Log("[ResultDebug] 個人戦リザルト表示");

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
