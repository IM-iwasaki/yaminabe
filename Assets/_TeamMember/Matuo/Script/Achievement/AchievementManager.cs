using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 実績の進行状況を管理する静的クラス（AchievementType対応版）
/// </summary>
public static class AchievementProgressManager {
    /// <summary>
    /// 実績進行を加算し、解除判定を行う
    /// </summary>
    public static void AddProgress(AchievementData achievement, int amount = 1) {
        var saveData = AchievementSaveSystem.GetSaveData();
        var progress = saveData.achievements.Find(a => a.id == achievement.id);
        if (progress == null) {
            progress = new AchievementSaveSystem.PlayerAchievementProgress {
                id = achievement.id,
                currentValue = 0,
                unlocked = false
            };
            saveData.achievements.Add(progress);
        }

        if (!progress.unlocked) {
            progress.currentValue += amount;
            if (progress.currentValue >= achievement.targetValue) {
                progress.currentValue = achievement.targetValue;
                progress.unlocked = true;
                Debug.Log($"実績解除！: {achievement.title}");
                // UIとかはここ
            }
            AchievementSaveSystem.Save();
        }
    }

    /// <summary>
    /// AchievementTypeごとにまとめて進行加算
    /// </summary>
    /// <param name="type">対象のAchievementType</param>
    /// <param name="amount">加算値</param>
    public static void AddProgressByType(AchievementDatabase database, AchievementType type, int amount = 1) {
        // データベースから指定タイプの実績を取得
        List<AchievementData> list = database.GetAchievementsByType(type);

        foreach (var ach in list) {
            AddProgress(ach, amount);
        }
    }

    /// <summary>
    /// 実績の現在進行度を取得
    /// </summary>
    public static int GetProgress(string id) {
        var saveData = AchievementSaveSystem.GetSaveData();
        var progress = saveData.achievements.Find(a => a.id == id);
        return progress != null ? progress.currentValue : 0;
    }

    /// <summary>
    /// 実績が解除済みかを取得
    /// </summary>
    public static bool IsUnlocked(string id) {
        var saveData = AchievementSaveSystem.GetSaveData();
        var progress = saveData.achievements.Find(a => a.id == id);
        return progress != null && progress.unlocked;
    }
}