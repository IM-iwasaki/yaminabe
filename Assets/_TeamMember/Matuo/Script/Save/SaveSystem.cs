using System.IO;
using UnityEngine;

public static class SaveSystem {
    private static string filePath => Path.Combine(Application.persistentDataPath, "playerData.json");

    // 保存
    public static void Save(PlayerData data) {
#if UNITY_EDITOR
        // エディターで実行中はセーブしない
        if (Application.isPlaying) return;

#endif
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log("データを保存しました: " + filePath);
    }

    // 読み込み
    public static PlayerData Load() {
#if UNITY_EDITOR
        // エディターで実行中はロードしない
        if (Application.isPlaying)
            return new PlayerData { currentMoney = 0, obtainedItems = new System.Collections.Generic.List<string>() };
#endif
        if (!File.Exists(filePath)) {
            // セーブファイルが無かったら新規作成
            return new PlayerData { currentMoney = 0, obtainedItems = new System.Collections.Generic.List<string>() };
        }

        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<PlayerData>(json);
    }
}