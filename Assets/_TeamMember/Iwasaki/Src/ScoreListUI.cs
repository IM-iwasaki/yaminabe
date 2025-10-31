using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// スコア一覧をスクロールビューで表示するUI管理クラス。
/// 
/// ・ResultManager からリストを受け取り自動表示
/// ・初期状態では非表示
/// </summary>
public class ScoreListUI : MonoBehaviour {
    [Header("UI参照")]
    public Transform content;             // ScrollView の Content
    public GameObject scoreEntryPrefab;   // スコア1行分のプレハブ
   /* public GameObject rootPanel;  */        // ScrollView全体のルート

    private void Start() {
        // 起動時は非表示
        //if (rootPanel != null)
        //    rootPanel.SetActive(false);
    }

    /// <summary>
    /// 渡されたスコアデータを一覧表示する。
    /// </summary>
    public void DisplayScores(List<ResultScoreData> scores) {
        //if (rootPanel != null)
        //    rootPanel.SetActive(true);

        // 古い要素を削除
        foreach (Transform child in content)
            Destroy(child.gameObject);

        // スコア降順にソート
        scores.Sort((a, b) => b.score.CompareTo(a.score));

        // 新規エントリ生成
        foreach (var data in scores)
            AddScoreEntry(data.playerName, data.score);
    }

    /// <summary>
    /// 1行分のスコアエントリを生成して内容を設定。
    /// </summary>
    private void AddScoreEntry(string playerName, int score) {
        if (scoreEntryPrefab == null) {
            Debug.LogError("[ScoreListUI] scoreEntryPrefab が未設定です。");
            return;
        }

        GameObject entry = Instantiate(scoreEntryPrefab, content);

        // 名前とスコアを設定
        foreach (var t in entry.GetComponentsInChildren<Text>()) {
            if (t.name.Contains("Name")) t.text = playerName;
            else if (t.name.Contains("Score")) t.text = score.ToString();
        }
    }
}
