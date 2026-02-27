using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Millionaire_戦費転用")]
public class Skill_Millionaire : SkillBase {

    //
    // パッシブ名　：戦費転用
    // 効果        ：所持している武器をマネーガンに変更し、すでにマネーガンを持っている場合は消費金額と与ダメージを増やす。
    //               最大まで強化済みの場合、ごく少量のお金を得る。
    //

    [SerializeField] private WeaponData[] weaponData;

    [SerializeField] private int amount = 100;

    public override void Activate(CharacterBase user) {
        if (!user.isLocalPlayer) return;
        WeaponData weapon = user.weaponController_main.weaponData;

        if(weapon.type == WeaponType.MoneyGun) {
            for (int i = 0; i < weaponData.Length; i++) {
                if (weaponData[i].WeaponID == weapon.WeaponID) {
                    int nextIndex = i + 1;

                    if (nextIndex < weaponData.Length) {
                        user.weaponController_main.CmdSetWeaponData(weaponData[nextIndex].WeaponID);
                    }
                    else {
                        // 最後だった場合
                        PlayerWallet.Instance.AddMoney(amount);
                    }

                    return;
                }
            }
        }
        //マネーガンにする
        else
            user.weaponController_main.CmdSetWeaponData(weaponData[0].WeaponID);


    }
}
