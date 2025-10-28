using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// スコア一覧UIを管理するクラス（Mirror対応）
/// サーバーから ClientRpc で送られたスコアを全クライアントに表示する
/// 最初は非表示で、必要に応じて Show/Hide で表示切り替え可能
/// </summary>
public class ScoreListUI : NetworkBehaviour {
    [Header("UI参照")]
    public Transform content;             // ScrollView の Content（ここに ScoreEntry を生成）
    public GameObject scoreEntryPrefab;   // スコア表示用のプレハブ
    public GameObject rootPanel;          // スコア一覧全体（非表示切り替え用）

    /// <summary>
    /// プレイヤー名とスコアのデータ構造
    /// Mirrorで送信できるよう struct にしている
    /// </summary>
    [System.Serializable]
    public struct PlayerScoreData {
        public string playerName;
        public int score;
    }

    private void Awake() {
        // 最初に非表示にしておく
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    /// <summary>
    /// サーバーから全クライアントにスコア表示を送信する RPC
    /// </summary>
    /// <param name="scores">送信するスコア配列</param>
    [ClientRpc]
    public void RpcDisplayScores(PlayerScoreData[] scores) {
        // 受け取った配列を List に変換して表示
        DisplayScores(new List<PlayerScoreData>(scores));
    }

    /// <summary>
    /// 渡されたスコアリストを UI に表示する
    /// </summary>
    /// <param name="scores">表示するスコアリスト</param>
    public void DisplayScores(List<PlayerScoreData> scores) {
        if (rootPanel != null)
            rootPanel.SetActive(true); // UIを表示

        // 既存のスコア表示をクリア
        foreach (Transform child in content)
            Destroy(child.gameObject);

        // スコア降順でソート
        scores.Sort((a, b) => b.score.CompareTo(a.score));

        // 各プレイヤーのスコアを UI に追加
        foreach (var data in scores)
            AddScoreEntry(data.playerName, data.score);
    }

    /// <summary>
    /// 個別のスコア表示を生成
    /// </summary>
    /// <param name="playerName">プレイヤー名</param>
    /// <param name="score">スコア</param>
    private void AddScoreEntry(string playerName, int score) {
        GameObject entry = Instantiate(scoreEntryPrefab, content);

        // プレハブ内の Text を名前とスコアで更新
        var texts = entry.GetComponentsInChildren<Text>();
        foreach (var t in texts) {
            if (t.name == "PlayerNameText") t.text = playerName;
            else if (t.name == "ScoreText") t.text = score.ToString();
        }
    }

    /// <summary>
    /// スコア一覧を非表示にする
    /// </summary>
    public void HideScores() {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }
}
