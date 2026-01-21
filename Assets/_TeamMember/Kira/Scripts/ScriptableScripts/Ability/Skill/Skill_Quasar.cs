using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Quasar_氷結列撃の術")]
public class Skill_Quasar : SkillBase {
    //
    //  スキル名：扇状氷結の術
    //  タイプ　：範囲攻撃型
    //  効果    ：前方に氷の衝撃波を発生させる。
    //　CT      ：16秒
    //

    [SerializeField]WeaponData weaponData;

     public override void Activate(CharacterBase user) {       
        Vector3 attackDir = user.parameter.GetShootDirection();
        StartExtraAttackDelay(user, attackDir);
    }

    public void StartExtraAttackDelay(CharacterBase user, Vector3 dir) {
        user.StartCoroutine(ExtraAttackRoutine(user, dir));
    }

    private IEnumerator ExtraAttackRoutine(CharacterBase user, Vector3 dir) {
        yield return null;
        //攻撃する
        ExtraAttack(dir,user);
    }

    private void ExtraAttack(Vector3 dir, CharacterBase user) {
        //元の武器情報をキャッシュ
        var SkillCash = user.parameter.weaponController_main.weaponData;
        //スキル用武器に切り替えて攻撃
        user.parameter.weaponController_main.weaponData = weaponData;
        user.parameter.weaponController_main.CmdRequestSkillAttack(dir, weaponData);
        //武器を戻す
        user.parameter.weaponController_main.weaponData = SkillCash;
    }
}
