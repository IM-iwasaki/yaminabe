using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Passive/Quasar_•XŒŒp")]
public class Passive_Quasar : PassiveBase {

    //
    // ƒpƒbƒVƒu–¼ F•XŒŒp
    // ƒ^ƒCƒv     FŽ©“®‰ñ•œŒ^
    // Œø‰Ê       FUŒ‚’†‚Å‚àMP‚ª‰ñ•œ‚·‚é‚æ‚¤‚É‚È‚èí‚ÉHP‚ª­‚µ‚¸‚Â‰ñ•œ‚·‚éB
    //              HP‚ªˆê’èˆÈã‚¾‚ÆMP‰ñ•œ—Ê‚ªAMP‚ªˆê’èˆÈã‚¾‚ÆHP‰ñ•œ—Ê‚ªªB
    //

    public override void PassiveSetting() {
        coolTime = cooldown;
        isPassiveActive = true;
    }

    public override void PassiveReflection(CharacterBase user) {
        //MP‚ÌŠî‘b‰ñ•œ—Ê
        int MPheal = 1;
        //HP50“ˆÈã‚ÅMP‰ñ•œ—Êã¸
        if (user.parameter.HP >= user.parameter.maxHP / 2) MPheal *= 2;
        //HP‚ÌŠî‘b‰ñ•œ—Ê
        float HPheal = 0.005f;
        //MP50“ˆÈã‚ÅHP‰ñ•œ—Êã¸
        if (user.parameter.MP >= user.parameter.maxMP / 2) HPheal *= 2;

        //UŒ‚’†‚Å‚àMP‚ª‰ñ•œ‚·‚éB‚»‚¤‚Å‚È‚¢‚Æ‚«AHP‚ª­‚µ‚¸‚Â‰ñ•œ‚·‚éB
        if (coolTime >= cooldown) {
            user.action.MPRegeneration(MPheal);
            user.CmdHealCharacter(HPheal,0.01f); 
            coolTime = 0;
        }

        coolTime += Time.deltaTime;
    }
}
