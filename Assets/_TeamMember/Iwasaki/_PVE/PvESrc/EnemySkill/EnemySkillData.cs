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

    [Header("共通設定")]
    public float cooldown = 3f;        // クールタイム
    [Range(0f, 1f)]
    public float useRate = 1f;        // 発動確率（今は100%、これはスキル抽選時の発動確立）
    /// <summary>
    /// スキル実行（サーバーから呼ばれる想定）
    /// </summary>
    public abstract void Execute(
        GameObject owner,        // 敵自身
        EnemyStatusBase status,      // 敵ステータス
        Transform target         // 対象プレイヤー
    );
}
