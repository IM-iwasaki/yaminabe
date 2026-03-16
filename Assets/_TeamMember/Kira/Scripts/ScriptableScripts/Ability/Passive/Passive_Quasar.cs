using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Passive/Quasar_氷血術")]
public class Passive_Quasar : PassiveBase {

    //
    // パッシブ名 ：氷血術
    // タイプ     ：シフト回復型
    // 効果       ：HPが50％以上の時、攻撃中でもMPが回復する。
    //              そうでないとき、HPが少しずつ回復する。
    //

    public override void PassiveSetting() {
        coolTime = cooldown;
        isPassiveActive = true;
    }

    public override void PassiveReflection(CharacterBase user) {
        //HPが50％以上の時、攻撃中でもMPが回復する。そうでないとき、HPが少しずつ回復する。
        if (user.parameter.HP >= user.parameter.maxHP / 2 && coolTime >= cooldown) {
            user.action.MPRegeneration(1);
            coolTime = 0;
        }
        else if (user.parameter.HP < user.parameter.maxHP / 2 && coolTime >= cooldown) {
            user.Heal(0.005f,0.01f); 
            coolTime = 0;
        }

        coolTime += Time.deltaTime;

        ////発動後のクールタイム管理
        //if (!isPassiveActive) {
        //    coolTime += Time.deltaTime;
        //    if (coolTime >= cooldown) {
        //        isPassiveActive = true;
        //        coolTime = 0;
        //    }
        //    return;
        //}
        //
        ////HPが50％以上でかつMPが33％未満の場合にHPを10％消費してMPを30％回復。
        //if (user.parameter.HP >= user.parameter.maxHP / 2 && user.parameter.MP < user.parameter.maxMP / 3) {
        //    user.parameter.HP -= user.parameter.maxHP / 10;
        //    user.parameter.MP += (user.parameter.maxMP / 10) * 3;
        //}
        ////HPが20％未満の時に攻撃するとMPを全消費し消費したMP*2の値分HPを回復。
        //if (user.parameter.HP < user.parameter.maxHP / 5 && user.input.AttackTriggered && isPassiveActive) {
        //    int RemoveMP = user.parameter.MP;
        //    user.parameter.MP -= RemoveMP;
        //    user.parameter.HP += RemoveMP * 2;
        //    isPassiveActive = false;
        //}
    }
}
