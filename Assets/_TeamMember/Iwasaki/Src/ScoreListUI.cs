using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScoreListUI : MonoBehaviour {
    [Header("UI参照")]
    public Transform content;             // ScrollView の Content
    public GameObject scoreEntryPrefab;   // スコア表示用プレハブ

    void Start() {
        // 仮のスコアデータを作成
        var sampleData = new List<(string name, int score)>
        {
            ("プレイヤーA", 1500),
            ("プレイヤーB", 1230),
            ("プレイヤーC", 980),
            ("プレイヤーD", 2000),
            ("プレイヤーE", 450),
            ("プレイヤーF", 3000),
            ("プレイヤーG", 120),
        };

        // 各データをリストに追加
        foreach (var (name, score) in sampleData) {
            AddScoreEntry(name, score);
        }
    }

    /// <summary>
    /// スコア表示項目を追加する
    /// </summary>
    public void AddScoreEntry(string playerName, int score) {
        GameObject entry = Instantiate(scoreEntryPrefab, content);
        var texts = entry.GetComponentsInChildren<Text>();

        Debug.Log($"追加: {playerName} / {score}");

        foreach (var t in texts) {
            Debug.Log($"  Text名: {t.name}");
            if (t.name == "PlayerNameText") t.text = playerName;
            else if (t.name == "ScoreText") t.text = score.ToString();
        }
    }
}
