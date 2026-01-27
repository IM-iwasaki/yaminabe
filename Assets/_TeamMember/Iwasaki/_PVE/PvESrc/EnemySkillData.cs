using UnityEngine;
using Mirror;

/// <summary>
/// 敵スキルの基底クラス
/// </summary>
public abstract class EnemySkillData : ScriptableObject {

    [Header("基本情報")]
    public string skillName;
    public float cooldown = 3f;
    public float range = 2f;

    /// <summary>
    /// スキル実行（サーバー専用）
    /// </summary>
    [Server]
    public abstract void Execute(
        GameObject owner,        // 敵自身
        EnemyStatus status,      // 敵ステータス
        Transform target         // 対象プレイヤー
    );
}
