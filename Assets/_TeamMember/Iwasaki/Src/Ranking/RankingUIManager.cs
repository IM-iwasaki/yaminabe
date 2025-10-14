using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ランキングUIを更新・表示
/// </summary>
public class RankingUIManager : MonoBehaviour {
    [Header("ランキング表示用テキスト")]
    public Text rankingText;

    /// <summary>
    /// ランキングデータを受け取ってUIを更新
    /// </summary>
    public void UpdateRankingDisplay(string[] rankingData) {
        rankingText.text = "=== ランキング ===\n";
        for (int i = 0; i < rankingData.Length; i++) {
            rankingText.text += $"{i + 1}. {rankingData[i]}\n";
        }
    }
}
