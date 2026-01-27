using UnityEngine;

/// <summary>
/// 敵ステータスの基底クラス（継承元）
/// すべての敵はこれを継承する
/// </summary>
public abstract class EnemyBaseStatus : MonoBehaviour {
    [Header("基本ステータス")]
    [SerializeField] protected int maxHp = 100;     // 最大HP
    [SerializeField] protected int attack = 10;     // 攻撃力
    [SerializeField] protected float moveSpeed = 3.5f; // 移動速度

    protected int currentHp; // 現在HP

    /// <summary>
    /// 初期化
    /// </summary>
    protected virtual void Awake() {
        currentHp = maxHp;
    }

    /// <summary>
    /// ダメージを受ける処理
    /// </summary>
    public virtual void TakeDamage(int damage) {
        currentHp -= damage;

        if (currentHp <= 0) {
            Die();
        }
    }

    /// <summary>
    /// 死亡処理（継承先で拡張可能）
    /// </summary>
    protected virtual void Die() {
        Destroy(gameObject);
    }

    /// <summary>
    /// 外部参照用 Getter
    /// </summary>
    public int GetAttack() => attack;
    public float GetMoveSpeed() => moveSpeed;
}
