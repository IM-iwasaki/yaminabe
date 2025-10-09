using UnityEngine;

public class TestRanking : MonoBehaviour {
    void Start() {


        PlayerPrefs.DeleteKey("RankingData");
        PlayerPrefs.Save();
        Debug.Log("RankingData を削除しました。");


        // RankingManagerを探す
        RankingManager manager = FindObjectOfType<RankingManager>();




        // 保存済みデータ読み込み
        manager.LoadRanking();

        // 新しいデータを追加してみる
        RankingEntry newEntry = new RankingEntry("PlayerA", 6000);
        manager.AddEntry(newEntry);


        PlayerPrefs.DeleteKey("RankingData");
        PlayerPrefs.Save();
    }
}
