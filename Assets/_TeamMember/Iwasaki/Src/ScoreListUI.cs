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
  

  

    /// <summary>
    /// 渡されたスコアデータを一覧表示する。
    /// </summary>
    public void DisplayScores(List<ResultScoreData> scores) {

        // 古い要素を削除
        foreach (Transform child in content)
            Destroy(child.gameObject);

        // スコア降順にソート
        scores.Sort((a, b) => b.Score.CompareTo(a.Score));

        // 新規エントリ生成
        foreach (var data in scores)
            AddScoreEntry(data.PlayerName, data.Score,data.Kills,data.Deaths,data.KD,data.TeamId);
    }

    /// <summary>
    /// 1行分のスコアエントリを生成して内容を設定。
    /// </summary>
    private void AddScoreEntry(string playerName, int score ,int kills, int deaths, float kd,int teamId) {
        if (scoreEntryPrefab == null) {
            Debug.LogError("[ScoreListUI] scoreEntryPrefab が未設定です。");
            return;
        }

        GameObject entry = Instantiate(scoreEntryPrefab, content);


        //　背景カラーを取得
        Image bg = entry.GetComponent<Image>();

        if (bg != null) {
            if (teamId == 0)
                bg.color = new Color(1f, 0.3f, 0.3f, 0.5f); // 赤チーム（半透明）
            else
                bg.color = new Color(0.1f, 0.1f, 1f, 0.3f); // 青チーム（半透明）
        }


        // 名前とスコアを設定
        foreach (var t in entry.GetComponentsInChildren<Text>()) {
            // 代入するオブジェクトにName、Scoreが入っていれば取れる
            if (t.name.Contains("Name")) t.text = playerName;
            else if (t.name.Contains("Score")) t.text = score.ToString();
            else if (t.name.Contains("Kill")) t.text = kills.ToString();
            else if (t.name.Contains("Death")) t.text = deaths.ToString();
            else if (t.name.Contains("KD")) t.text = kd.ToString("0.0");   // ★ 小数1桁


        }
    }
}
