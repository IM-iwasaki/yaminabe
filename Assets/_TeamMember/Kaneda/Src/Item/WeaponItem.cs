using UnityEngine;

/// <summary>
/// 武器の種類
/// </summary>
public enum WeaponType {
    Melee,      //  近接
    Gun,        //  銃
    Magic,      //  魔法
}

/// <summary>
/// 武器アイテムクラス
/// </summary>
public class WeaponItem : ItemBase {
    [Header("武器タイプ")]
    public WeaponType weaponType;
    //  武器で取得する能力値などを追加する


    /// <summary>
    /// 使用処理
    /// </summary>
    public override void Use(GameObject player) {
        //  プレイヤー処理(プレイヤーが出来次第追加)


        // 使用後にアイテムを削除
        Destroy(gameObject);
    }
}
