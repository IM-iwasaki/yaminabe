using Mirror;
using UnityEngine;

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
        if (playerWeaponData == null) {
            Debug.LogWarning("プレイヤーの中にNetworkWeaponが見つかりませんでした");
            return;
        }

        //  持っている武器データをプレイヤーに受け渡す
        playerWeaponData.SetWeaponData(weaponData.WeaponName);
        //  キャラクター側のフラグをリセットする
        player.GetComponent<CharacterBase>().ResetCanPickFlag();

        //見た目変更
        ChangeWeaponModel(player, weaponData.weaponModel);
        player.GetComponent<CharacterBase>().ChangeLayerWeight(GenerateWeaponIndex(weaponData.weaponName));

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

    /// <summary>
    /// 各役職共通でレイヤーのインデックスを返す
    /// </summary>
    /// <param name="_weaponName"></param>
    /// <returns></returns>
    public int GenerateWeaponIndex(string _weaponName) {
        return _weaponName switch {
            "HandGun" or "Punch" or "FireMagic" => 1,
            "Assult" or "BurstAssult" or "Spear" or "IceMagic" => 2,
            "RPG" => 3,
            "Sniper" => 4,
            "Minigun" => 5,
            _ => -1,
        };
    }

    private void ChangeWeaponModel(GameObject _player,GameObject _weaponModel) {
        //反映するプレイヤーの手の座標を取得
        Transform handRoot = _player.GetComponent<CharacterBase>().anim.GetBoneTransform(HumanBodyBones.RightHand);
        //実際に生成
        GameObject currentWeapon = Instantiate(_weaponModel, handRoot);
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;
        
    }
}
