using UnityEngine;
using Mirror;

/// <summary>
/// ESCキーでテスト的にリザルト画面を出すデバッグスクリプト
/// </summary>
public class ResultDebug : NetworkBehaviour {
    private ResultManager resultManager;

    void Start() {
        resultManager = FindObjectOfType<ResultManager>();
        if (resultManager == null)
            Debug.LogError("ResultManager がシーンに存在しません");
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.Escape) && isServer) {
            Debug.Log("ESC押下 → テストでリザルト表示");

            // 仮スコアデータ
            ScoreListUI.PlayerScoreData[] dummyScores = new ScoreListUI.PlayerScoreData[]
            {
                new ScoreListUI.PlayerScoreData { playerName = "Alice", score = 1200 },
                new ScoreListUI.PlayerScoreData { playerName = "Bob", score = 800 },
                new ScoreListUI.PlayerScoreData { playerName = "Charlie", score = 1500 },
                new ScoreListUI.PlayerScoreData { playerName = "Delta", score = 600 }
            };

            resultManager.ShowResultWithScores(dummyScores);
        }
    }
}
