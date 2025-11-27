using System.Collections;

using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Soldier_積極攻勢")]
public class Skill_Soldier : SkillBase {

    //
    //  スキル名：全力突撃
    //  タイプ　：連続攻撃型
    //  効果    ：前方に突進しながら怒涛の連続攻撃を行う。
    //　CT      ：20秒
    //

    public override void Activate(CharacterBase user) {
        //前方に力を加える
        user.MoveSpeedBuff(1.5f,1.0f);
        //TODO:マジックナンバー
        //user.GetComponent<Rigidbody>().velocity = user.transform.forward * 100 + Vector3.up * 10;
        Vector3 dashDir = ((user.transform.forward * 80) + user.transform.up * 4).normalized;
        //user.GetComponent<Rigidbody>().AddForce(dashDir, ForceMode.VelocityChange);        

        Vector3 attackDir = user.GetShootDirection();
        StartExtraAttackDelay(user, 0.03f, 36, attackDir);
    }

    public void StartExtraAttackDelay(CharacterBase user,float delay, int repeatCount, Vector3 dir) {
        user.StartCoroutine(ExtraAttackRoutine(user, delay, repeatCount, dir));
    }

    private IEnumerator ExtraAttackRoutine(CharacterBase user, float delay, int repeatCount, Vector3 dir) {
        for (int i = 0; i < repeatCount; i++) {
            yield return new WaitForSeconds(delay);
            ExtraAttack(dir,user);
        }
    }

    private void ExtraAttack(Vector3 dir, CharacterBase user) {
        user.weaponController_main.CmdRequestExtraAttack(dir);
    }

}
