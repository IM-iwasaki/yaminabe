using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;


/// <summary>
/// スコア一覧UIを管理するクラス（Mirror対応）
/// 
/// 【使い方】
/// ▼概要
/// ・サーバー側がゲーム終了時などにスコアを集計し、
///   ResultManager から全クライアントに送信してリザルトに表示する。
/// ・スコアの表示（中身）はこのクラスが担当。
/// ・UIの生成／削除は ResultManager、
///   ボタン制御は ResultPanel が担当。
/// 
/// ▼設定手順
/// ① Canvas 内に ScrollView を用意する。
/// ② ScrollView の Content をこのスクリプトの「content」に割り当てる。
/// ③ 各スコア行用のプレハブ（例：PlayerNameText と ScoreText を持つ）を作り、
///    「scoreEntryPrefab」に割り当てる。
/// ④ ResultPanel プレハブの中にこの ScoreListUI を配置しておく。
/// 
/// ▼スコア送信例（サーバー側で呼ぶ）
/// var scores = new ScoreListUI.PlayerScoreData[] {
///     new ScoreListUI.PlayerScoreData { playerName = "Alice", score = 100 },
///     new ScoreListUI.PlayerScoreData { playerName = "Bob", score = 80 }
/// };
/// scoreListUI.RpcDisplayScores(scores);
/// 
/// ▼動作の流れ
/// ① サーバーが RpcDisplayScores() を呼ぶ
/// ② 全クライアントで DisplayScores() が実行され、
///    スコアリストが自動生成されて表示される。
/// </summary>
public class ScoreListUI : NetworkBehaviour {
    [Header("UI参照")]
    public Transform content;           // ScrollView の Content（ここに ScoreEntry を生成）
    public GameObject scoreEntryPrefab; // スコア表示用のプレハブ

    /// <summary>
    /// プレイヤー名とスコアのデータ構造（Mirrorで送信可能）
    /// </summary>
    [System.Serializable]
    public struct PlayerScoreData {
        public string playerName;
        public int score;
    }

    /// <summary>
    /// サーバーから全クライアントにスコア表示を送信
    /// </summary>
    [ClientRpc]
    public void RpcDisplayScores(PlayerScoreData[] scores) {
        DisplayScores(new List<PlayerScoreData>(scores));
    }

    /// <summary>
    /// 渡されたスコアリストを UI に表示
    /// </summary>
    public void DisplayScores(List<PlayerScoreData> scores) {
        // 既存エントリをクリア
        foreach (Transform child in content)
            Destroy(child.gameObject);

        // 降順ソート
        scores.Sort((a, b) => b.score.CompareTo(a.score));

        // スコアを追加
        foreach (var data in scores)
            AddScoreEntry(data.playerName, data.score);
    }

    /// <summary>
    /// 個別スコア行を生成
    /// </summary>
    private void AddScoreEntry(string playerName, int score) {
        GameObject entry = Instantiate(scoreEntryPrefab, content);

        var texts = entry.GetComponentsInChildren<Text>();
        foreach (var t in texts) {
            if (t.name == "PlayerNameText") t.text = playerName;
            else if (t.name == "ScoreText") t.text = score.ToString();
        }
    }
}
