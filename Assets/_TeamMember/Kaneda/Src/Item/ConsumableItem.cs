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

    [Header("効果時間")]
    public float usingTime;

    /// <summary>
    /// 使用処理
    /// </summary>
    public override void Use(GameObject player) {
        // プレイヤー処理(プレイヤーが出来次第追加)
        CharacterBase character = player.GetComponent<CharacterBase>();
        if(character == null) {
            Debug.LogWarning("PlayerにCharacterBaseが見つかりませんでした");
            return;
        }

        switch (type) {
            case ConsumableType.Heal:
                character.Heal(value, usingTime);
                break;
            case ConsumableType.SpeedUp:
                character.MoveSpeedBuff(value, usingTime);
                break;
            case ConsumableType.AttackBoost:
                character.AttackBuff(value, usingTime);
                break;
        }

        // 使用後にアイテムを削除
        if(canDestroy) Destroy(gameObject);

    }
}
