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
    //  武器データ保存
    [Header("作製したデータをこの中に入れる")]
    [SerializeField]
    private WeaponData weaponData = null;

    /// <summary>
    /// 使用処理
    /// </summary>
    public override void Use(GameObject player) {
        //  プレイヤー処理(プレイヤーが出来次第追加)
        NetworkWeapon playerWeapon = player.GetComponent<NetworkWeapon>();
        if (playerWeapon != null) {
            Debug.LogWarning("プレイヤーの中にNetworkWeaponが見つかりませんでした");
            return;
        }

        //  持っている武器データをプレイヤーに受け渡す


        // 使用後にアイテムを削除
        Destroy(gameObject);
        //  ネットワーク処理後にコメントを外してこっちを使用する
        //SpawnManager.Instance.DestroyNetworkObject(gameObject);
    }
}
