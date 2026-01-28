using UnityEngine;

/// <summary>
/// 敵スキル：テスト用　どんな感じでやるか見るためだけのやつ
/// </summary>
[CreateAssetMenu(
    fileName = "EnemyTestSkill",
    menuName = "Enemy/Skill/TestSkill"
)]
public class EnemyTestSkill : EnemySkillData {

    [Header("テスト用パラメータ")]
    public int testValue;        // 仮の数値
    public float testRadius;     // 仮の範囲

    /// <summary>
    /// スキル実行
    /// </summary>
    public override void Execute(
        GameObject owner,   // 敵自身
        EnemyStatus status, // 敵ステータス
        Transform target    // 対象プレイヤー
    ) {
        // ここに後で処理を書く
        // 例：範囲攻撃 / 突進 / 状態異常付与 など
    }
}
