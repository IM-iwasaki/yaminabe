using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 実績データをまとめて管理する ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "AchievementData", menuName = "Achieve/AchievementData")]
public class AchievementDatabase : ScriptableObject {
    [Header("実績リスト")]
    public List<AchievementData> achievements = new();

    /// <summary>
    /// 実績IDから対応する実績データを取得する
    /// </summary>
    public AchievementData GetAchievementById(string id) {
        return achievements.Find(a => a.id == id);
    }

    /// <summary>
    /// 実績タイプから対応する実績データをすべて取得する
    /// </summary>
    public List<AchievementData> GetAchievementsByType(AchievementType type) {
        return achievements.FindAll(a => a.type == type);
    }
}