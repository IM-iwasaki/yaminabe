using UnityEngine;

/// <summary>
/// 敵ステータスのデータ定義
/// （PrefabやAIから完全分離）
/// </summary>
[CreateAssetMenu(menuName = "Enemy/Status")]
public class EnemyStatusData : ScriptableObject {
    [Header("基本ステータス")]
    public int maxHp = 100;
    public int attack = 10;
    public float moveSpeed = 3.5f;
}
