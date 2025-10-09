using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankingManager : MonoBehaviour {
    [Header("UI設定")]
    public GameObject rankingItemPrefab; // ランキングの1行UI（Textなど）
    public Transform rankingParent;      // 並べる親オブジェクト（VerticalLayoutGroup推奨）

    [Header("ランキング設定")]
    public int maxEntries = 10;          // 最大件数
    public List<RankingEntry> rankingList = new List<RankingEntry>();

    // ランキングに新しい結果を追加
    public void AddEntry(RankingEntry newEntry) {
        rankingList.Add(newEntry);

        // ソート基準：スコア高い順
        rankingList.Sort((a, b) => b.score.CompareTo(a.score));

        // 上位のみ残す
        if (rankingList.Count > maxEntries)
            rankingList.RemoveRange(maxEntries, rankingList.Count - maxEntries);

        SaveRanking();
        DisplayRanking();
    }

    // ランキング表示
    public void DisplayRanking() {
        foreach (Transform child in rankingParent)
            Destroy(child.gameObject);

        for (int i = 0; i < rankingList.Count; i++) {
            var entry = rankingList[i];
            GameObject item = Instantiate(rankingItemPrefab, rankingParent);
            Text text = item.GetComponent<Text>();
            if (text != null) {
                text.text =
                    $"{i + 1}. {entry.playerName} | Score: {entry.score} ";
            }
        }
    }

    // 保存
    public void SaveRanking() {
        string json = JsonUtility.ToJson(new RankingWrapper(rankingList));
        PlayerPrefs.SetString("RankingData", json);
        PlayerPrefs.Save();
    }

    // 読み込み
    public void LoadRanking() {
        if (PlayerPrefs.HasKey("RankingData")) {
            string json = PlayerPrefs.GetString("RankingData");
            RankingWrapper wrapper = JsonUtility.FromJson<RankingWrapper>(json);
            if (wrapper != null && wrapper.list != null)
                rankingList = wrapper.list;
        }
        DisplayRanking();
    }

    [System.Serializable]
    private class RankingWrapper {
        public List<RankingEntry> list;
        public RankingWrapper(List<RankingEntry> list) => this.list = list;
    }
}
