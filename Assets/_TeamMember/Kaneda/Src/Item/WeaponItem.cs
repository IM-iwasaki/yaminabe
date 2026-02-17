using Mirror;
using UnityEngine;
using static CharacterEnum;

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
        MainWeaponController playerWeaponData = player.GetComponent<MainWeaponController>();
        CharacterBase playerBase = player.GetComponent<CharacterBase>();
        if (playerWeaponData == null) {
            Debug.LogWarning("プレイヤーの中にNetworkWeaponが見つかりませんでした");
            return;
        }

        if (!playerWeaponData.CanUseWeapon(playerWeaponData.charaterType, weaponData.type)) {
            AudioManager.Instance.CmdPlayUISE("武器取得失敗");
            return;
        }

        //  持っている武器データをプレイヤーに受け渡す
        playerWeaponData.CmdSetWeaponData(weaponData.WeaponID);
        //  キャラクター側のフラグをリセットする
        playerBase.action.ResetCanPickFlag();

        //効果音を流す
        AudioManager.Instance.CmdPlayUISE("武器取得");

        // 使用後にアイテムを削除
        if (canDestroy) CmdRequestDestroy();
    }

    /// <summary>
    /// 破棄処理
    /// </summary>
    [Command(requiresAuthority = false)]
    public override void CmdRequestDestroy() {
        NetworkServer.Destroy(gameObject);
    }
}
