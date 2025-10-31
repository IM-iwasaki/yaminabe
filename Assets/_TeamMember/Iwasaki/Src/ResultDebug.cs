using UnityEngine;
using Mirror;

/// <summary>
/// テスト用：ESCキーで個人戦 / チーム戦リザルトを切り替え表示。
/// </summary>
public class ResultDebug : NetworkBehaviour {
    private ResultManager resultManager;
    private bool toggle = false;

    private void Start() {
        resultManager = FindObjectOfType<ResultManager>();
    }

    private void Update() {
        if (Input.GetKeyUp(KeyCode.Escape) && isServer) {
            toggle = !toggle;
            if (toggle) ShowIndividualBattle();
            else ShowTeamBattle();
        }
    }

    /// <summary>
    /// 個人戦の勝敗サンプル表示。
    /// </summary>
    private void ShowIndividualBattle() {
        Debug.Log("[ResultDebug] 個人戦リザルト表示");

        var data = new ResultManager.ResultData {
            isTeamBattle = false,
            winnerName = "Alice",
            scores = new ResultScoreData[]
            {
                new ResultScoreData { playerName = "Alice", score = 1200 },
                new ResultScoreData { playerName = "Bob", score = 800 },
                new ResultScoreData { playerName = "Charlie", score = 1500 },
                new ResultScoreData { playerName = "Delta", score = 600 }
            }
        };

        resultManager.ShowResult(data);
    }

    /// <summary>
    /// チーム戦の勝敗サンプル表示。
    /// </summary>
    private void ShowTeamBattle() {
        Debug.Log("[ResultDebug] チーム戦リザルト表示");

        var data = new ResultManager.ResultData {
            isTeamBattle = true,
            winnerName = "Red",
            scores = new ResultScoreData[]
            {
                new ResultScoreData { playerName = "Alice", score = 1200 },
                new ResultScoreData { playerName = "Bob", score = 800 },
                new ResultScoreData { playerName = "Charlie", score = 1500 },
                new ResultScoreData { playerName = "Delta", score = 600 }
            }
        };

        resultManager.ShowResult(data);
    }
}
