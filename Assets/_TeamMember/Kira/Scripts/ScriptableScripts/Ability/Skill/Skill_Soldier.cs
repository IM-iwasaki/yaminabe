using System.Collections;

using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Soldier_積極攻勢")]
public class Skill_Soldier : SkillBase {

    //
    //  スキル名：ラウンドクラッシュ
    //  タイプ　：連続攻撃型
    //  効果    ：前方に突進しながら怒涛の連続攻撃を行う。
    //　CT      ：20秒
    //

    readonly float forwardPower = 27.0f;
    readonly float upPower = 3.5f;


    public override void Activate(CharacterBase user) {       
        Vector3 attackDir = user.parameter.GetShootDirection();
        StartExtraAttackDelay(user, 0.015f, 16, attackDir);
    }

    public void StartExtraAttackDelay(CharacterBase user,float delay, int repeatCount, Vector3 dir) {
        user.StartCoroutine(ExtraAttackRoutine(user, delay, repeatCount, dir));
    }

    private IEnumerator ExtraAttackRoutine(CharacterBase user, float delay, int repeatCount, Vector3 dir) {
        for (int i = 0; i < repeatCount; i++) {
            yield return new WaitForSeconds(delay);
            //前方に力を加える
            user.rb.velocity = user.transform.forward * forwardPower + user.transform.up * upPower;
            //攻撃する
            ExtraAttack(dir,user);
        }
    }

    private void ExtraAttack(Vector3 dir, CharacterBase user) {
        user.weaponController_main.CmdRequestExtraAttack(dir);
    }

}
