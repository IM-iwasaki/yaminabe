using System.IO;
using UnityEngine;

/// <summary>
/// 実績データの保存・ロードを担当する静的クラス
/// </summary>
public static class AchievementSaveSystem {
    // JSON保存先パス
    private static string filePath => Path.Combine(Application.persistentDataPath, "achievements.json");

    /// <summary>
    /// プレイヤーの実績進行状況を保持するクラス
    /// </summary>
    [System.Serializable]
    public class PlayerAchievementProgress {
        public string id;        // 実績ID
        public int currentValue; // 現在の進行度
        public bool unlocked;    // 解除済みか
    }

    /// <summary>
    /// 実績全体の保存用データクラス
    /// </summary>
    [System.Serializable]
    public class AchievementSaveData {
        public System.Collections.Generic.List<PlayerAchievementProgress> achievements = new();
    }

    private static AchievementSaveData saveData;

    /// <summary>
    /// JSONから実績データをロードする
    /// ファイルがなければ空データを作成
    /// </summary>
    public static void Load() {
        if (!File.Exists(filePath)) {
            saveData = new AchievementSaveData();
            return;
        }

        string json = File.ReadAllText(filePath);
        saveData = JsonUtility.FromJson<AchievementSaveData>(json);
    }

    /// <summary>
    /// 実績データをJSONに保存する
    /// </summary>
    public static void Save() {
        if (saveData == null) return;
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// 現在の実績進行データを取得
    /// </summary>
    public static AchievementSaveData GetSaveData() {
        if (saveData == null) Load();
        return saveData;
    }
}