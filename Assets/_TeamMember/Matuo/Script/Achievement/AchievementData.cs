using UnityEngine;

/// <summary>
/// 実績の情報を保持するクラス
/// </summary>
[System.Serializable]
public class AchievementData {
    [Header("基本情報")]
    public string title;            // 実績タイトル
    public string id;               // 実績ID
    [TextArea]
    public string description;      // 実績の説明
    public Sprite icon;             // 実績アイコン

    [Header("達成条件")]
    public int targetValue = 1;     // 達成に必要な値
    public AchievementType type;    // 実績タイプ
}