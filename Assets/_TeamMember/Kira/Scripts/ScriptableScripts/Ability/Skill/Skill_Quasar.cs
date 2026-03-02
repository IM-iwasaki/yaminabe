using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Quasar_氷結列撃の術")]
public class Skill_Quasar : SkillBase {
    //
    //  スキル名：扇状氷結の術
    //  タイプ　：範囲攻撃型
    //  効果    ：一定時間氷の衝撃波で攻撃できる。
    //　CT      ：20秒
    //  
    //          「「「「「「霜踏み」」」」」」

    [SerializeField] private WeaponData weaponData;
    [SerializeField] private int time;

     public override void Activate(CharacterBase user) {
        StartExtraAttackDelay(user);
    }

    public void StartExtraAttackDelay(CharacterBase user) {
        user.StartCoroutine(ExtraAttackRoutine(user));
    }

    private IEnumerator ExtraAttackRoutine(CharacterBase user) {
        // 元武器を保存
        var originalWeapon = user.weaponController_main.weaponData.WeaponID;

        // スキル武器へ変更
        user.weaponController_main.CmdSetWeaponData(weaponData.WeaponID);

        // 指定時間維持
        yield return new WaitForSeconds(time);

        // 元に戻す
        if (user.weaponController_main.weaponData.WeaponID == weaponData.WeaponID) {
            user.weaponController_main.CmdSetWeaponData(originalWeapon);
        }
    }
}
