using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// エディタでリザルト画面を確認するためのテストスクリプト
/// ESCキーを押すとサーバー側でリザルトと仮スコアを全クライアントに送信する
/// </summary>
public class ResultDebug : NetworkBehaviour {
    private ResultManager resultManager;
    private ScoreListUI scoreListUI;

    private void Start() {
        // シーン内の ResultManager を取得
        resultManager = FindObjectOfType<ResultManager>();
        if (resultManager == null)
            Debug.LogError("ResultManager がシーンに存在しません");

        // シーン内の ScoreListUI を取得
        scoreListUI = FindObjectOfType<ScoreListUI>();
        if (scoreListUI == null)
            Debug.LogError("ScoreListUI がシーンに存在しません");
    }

    private void Update() {
        // ESC キーでテスト実行
        if (Input.GetKeyUp(KeyCode.Escape)) {
            if (isServer) // サーバー側のみ処理
            {
                Debug.Log("ESC押下 → ゲーム終了テスト（サーバー呼び出し）");

                // ResultManager で全員にリザルト画面表示
                resultManager.RpcShowResult();

                // 仮データ作成（PlayerScoreData 配列）
                ScoreListUI.PlayerScoreData[] dummyScores = new ScoreListUI.PlayerScoreData[]
                {
                    new ScoreListUI.PlayerScoreData { playerName = "Alice", score = 1200 },
                    new ScoreListUI.PlayerScoreData { playerName = "Bob", score = 800 },
                    new ScoreListUI.PlayerScoreData { playerName = "Charlie", score = 1500 },
                    new ScoreListUI.PlayerScoreData { playerName = "Delta", score = 600 }
                };

                // 全クライアントにスコア送信
                scoreListUI.RpcDisplayScores(dummyScores);
            }
        }
    }
}
