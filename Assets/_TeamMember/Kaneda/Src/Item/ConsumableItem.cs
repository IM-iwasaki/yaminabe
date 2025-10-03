using UnityEngine;

/// <summary>
/// 消費アイテムの種類
/// </summary>
public enum ConsumableType {
    Heal,           //  回復
    SpeedUp,        //  速度アップ
    AttackBoost     //  攻撃力アップ
}

/// <summary>
/// 消費アイテムクラス
/// </summary>
public class ConsumableItem : ItemBase {
    [Header("消費アイテムの種類")]
    public ConsumableType type;

    [Header("効果量（回復値・上昇率など）")]
    public float value;

    /// <summary>
    /// 使用処理
    /// </summary>
    public override void Use(GameObject player) {
        // プレイヤー処理(プレイヤーが出来次第追加)
        
        switch (type) {
            case ConsumableType.Heal:
                break;
            case ConsumableType.SpeedUp:
                break;
            case ConsumableType.AttackBoost:
                break;
        }

        // 使用後にアイテムを削除
        Destroy(gameObject);

    }
}
