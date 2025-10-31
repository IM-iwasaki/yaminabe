using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// スコア一覧UI（Mirror非依存）
/// ResultManager から ClientRpc 経由でデータを受け取り、
/// ローカル上でリストを生成して表示する。
/// </summary>
public class ScoreListUI : MonoBehaviour {
    [Header("UI参照")]
    public Transform content;           // ScrollViewのContent
    public GameObject scoreEntryPrefab; // スコア1行分のプレハブ
    public GameObject rootPanel;        // ScrollView全体の親（非表示制御用）

    [System.Serializable]
    public struct PlayerScoreData {
        public string playerName;
        public int score;
    }

    private void Start() {
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    /// <summary>
    /// サーバーから渡されたスコアリストをUIに反映
    /// </summary>
    public void DisplayScores(List<PlayerScoreData> scores) {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        foreach (Transform child in content)
            Destroy(child.gameObject);

        // スコア降順ソート
        scores.Sort((a, b) => b.score.CompareTo(a.score));

        foreach (var data in scores)
            AddScoreEntry(data.playerName, data.score);
    }

    private void AddScoreEntry(string playerName, int score) {
        if (scoreEntryPrefab == null) {
            Debug.LogError("[ScoreListUI] scoreEntryPrefab が未設定！");
            return;
        }

        GameObject entry = Instantiate(scoreEntryPrefab, content);

        // TextまたはTMP_Text両対応
        foreach (var t in entry.GetComponentsInChildren<Text>()) {
            if (t.name.Contains("Name")) t.text = playerName;
            else if (t.name.Contains("Score")) t.text = score.ToString();
        }

        Debug.Log($"[ScoreListUI] AddScoreEntry: {playerName} ({score})");
    }
}
