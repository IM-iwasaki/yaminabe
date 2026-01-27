using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject {
    public string enemyName;
    public EnemyType enemyType;

    public int hp;
    public float moveSpeed;
    public float attackRange;
    public float attackInterval;
}
