using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Passive/Quasar_氷血術")]
public class Passive_Quasar : PassiveBase {

    //
    // パッシブ名 ：氷血術
    // タイプ     ：代償回復型
    // 効果       ：HPが50％以上でMPが33％未満になる度に、
    //              HPを10％消費してMPを即座に30％回復する。
    //              HPが20％未満の時に攻撃を行った場合、
    //              MPをすべて消費して消費したMP*2の値分HPを回復する。
    //              このHP回復効果は一度発動すると10秒は発動しない。
    //

    public override void PassiveSetting() {
        coolTime = cooldown;
        isPassiveActive = true;
    }

    public override void PassiveReflection(CharacterBase user) {
        //発動後のクールタイム管理
        if (!isPassiveActive) {
            coolTime += Time.deltaTime;
            if (coolTime >= cooldown) {
                isPassiveActive = true;
                coolTime = 0;
            }
            return;
        }

        //HPが50％以上でかつMPが33％未満の場合にHPを10％消費してMPを30％回復。
        if (user.parameter.HP >= user.parameter.maxHP / 2 && user.parameter.MP < user.parameter.maxMP / 3) {
            user.parameter.HP -= user.parameter.maxHP / 10;
            user.parameter.MP += (user.parameter.maxMP / 10) * 3;
        }
        //HPが20％未満の時に攻撃するとMPを全消費し消費したMP*2の値分HPを回復。
        if (user.parameter.HP < user.parameter.maxMP / 5 && user.input.AttackTriggered && isPassiveActive) {
            int RemoveMP = user.parameter.MP;
            user.parameter.MP -= RemoveMP;
            user.parameter.HP += RemoveMP * 2;
            isPassiveActive = false;
        }
    }
}
