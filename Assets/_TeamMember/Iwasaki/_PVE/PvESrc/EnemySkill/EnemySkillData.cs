using UnityEngine;

/// <summary>
/// 敵スキルの基底クラス
/// </summary>
public abstract class EnemySkillData : ScriptableObject {

    [Header("スキル名")]
    public string skillName;           // スキル名
    [Header("説明")]
    [TextArea(5, 4)]
    public string description;         // スキル説明

    [Header("挙動設定")]
    public float cooldown = 3f;        // クールタイム
    public float range = 2f;           // 有効距離

    /// <summary>
    /// スキル実行（サーバーから呼ばれる想定）
    /// </summary>
    public abstract void Execute(
        GameObject owner,        // 敵自身
        EnemyStatus status,      // 敵ステータス
        Transform target         // 対象プレイヤー
    );
}
