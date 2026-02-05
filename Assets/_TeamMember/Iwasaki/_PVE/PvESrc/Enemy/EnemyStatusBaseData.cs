using UnityEngine;

/// <summary>
/// 敵ステータスのデータ定義
/// </summary>
[CreateAssetMenu(menuName = "Enemy/Status")]
public class EnemyStatusBaseData : ScriptableObject {

    [Header("エネミー名")]
    public string enemyName;            // 敵の名前

    [Header("敵プレハブ")]
    public GameObject enemyPrefab;

    [Header("説明")]
    [TextArea(5, 4)]
    public string description;          // 敵の説明文

    [Header("基本ステータス")]
    public int maxHp = 100;              // 最大HP
    public int attack = 10;              // 攻撃力

    [Header("移動 / NavMesh用")]
    public float moveSpeed = 3.5f;       // 移動速度
    public float acceleration = 50f;     // 加速
    public float angularSpeed = 720f;    // 旋回速度
    public float stoppingDistance = 1.2f;// 攻撃開始距離
}