using UnityEngine;

/// <summary>
/// 敵ステータスのデータ定義
/// </summary>
[CreateAssetMenu(menuName = "Enemy/Status")]
public class EnemyStatusBaseData : ScriptableObject {

    [Header("エネミー名")]
    public string enemyName;          // 敵の名前
    [Header("説明")]
    [TextArea(5, 4)]
    public string description;        // 敵の説明文

    [Header("基本ステータス")]
    public int maxHp = 100;
    public int attack = 10;
    public float moveSpeed = 3.5f;
}
