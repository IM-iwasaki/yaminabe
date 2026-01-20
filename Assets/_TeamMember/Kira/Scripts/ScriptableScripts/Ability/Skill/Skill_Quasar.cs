using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Quasar_•XŒ‹—ñŒ‚‚Ìp")]
public class Skill_Quasar : SkillBase {
    //
    //  ƒXƒLƒ‹–¼Fîó•XŒ‹‚Ìp
    //  ƒ^ƒCƒv@F”ÍˆÍUŒ‚Œ^
    //  Œø‰Ê    F‘O•û‚É•X‚ÌÕŒ‚”g‚ğ”­¶‚³‚¹‚éB
    //@CT      F16•b
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
        //UŒ‚‚·‚é
        ExtraAttack(dir,user);
    }

    private void ExtraAttack(Vector3 dir, CharacterBase user) {
        user.weaponController_main.CmdRequestExtraAttack(dir);
    }
}
