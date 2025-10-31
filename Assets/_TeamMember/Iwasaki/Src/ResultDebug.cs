using UnityEngine;
using Mirror;

/// <summary>
/// テスト用：ESCキーでダミースコアを表示
/// </summary>
public class ResultDebug : NetworkBehaviour {
    private ResultManager resultManager;

    void Start() {
        resultManager = FindObjectOfType<ResultManager>();
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.Escape) && isServer) {
            Debug.Log("[ResultDebug] ESC押下 → リザルトテスト開始");

            ResultManager.PlayerScoreData[] dummy = new ResultManager.PlayerScoreData[]
            {
                new ResultManager.PlayerScoreData { playerName = "Alice", score = 1200 },
                new ResultManager.PlayerScoreData { playerName = "Bob", score = 800 },
                new ResultManager.PlayerScoreData { playerName = "Charlie", score = 1500 },
                new ResultManager.PlayerScoreData { playerName = "Delta", score = 600 }
            };

            resultManager.ShowResultWithScores(dummy);
        }
    }
}
