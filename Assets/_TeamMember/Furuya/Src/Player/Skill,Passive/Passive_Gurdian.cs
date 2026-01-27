using UnityEngine;

[CreateAssetMenu(menuName = "Character/Passive/Gurdian_軍隊形成")]
public class Passive_Gurdian : PassiveBase {

    //
    // パッシブ名　：軍隊形成
    // タイプ      ：人数発動
    // 効果        ：近くに味方がいる間、一定時間ごとにHPが少しずつ回復する。
    //               孤立している間は、移動速度が上昇する。
    //

    public override void PassiveSetting() {
        //発動中でなかったら発動中の状態にする
        if (!isPassiveActive){
            isPassiveActive = true;
            //クールタイム計測をリセット
            coolTime = 0;
        }
    }

    public override void PassiveReflection(CharacterBase user) {

        // 味方が近くにいる → 定期回復
        if (user.parameter.HasNearbyAlly) {

            // クールタイム計測
            coolTime += Time.deltaTime;

            if (coolTime >= cooldown) {
                user.Heal(0.3f, 2.0f);
                coolTime = 0;
            }

            // スキル使用中、速度バフ中以外は速度上昇効果を戻す
            if(!user.parameter.equippedSkills[0].isSkillUse && user.speedCoroutine == null) {
                user.parameter.OutDefaultStatus_MoveSpeed();
            }           
            return;
        }

        // スキル使用中、速度バフ中以外は孤立時に速度を増加
        if(!user.parameter.equippedSkills[0].isSkillUse && user.speedCoroutine == null) {
            user.parameter.moveSpeed =
        Mathf.RoundToInt(user.parameter.defaultMoveSpeed * 1.3f);
        }          
    }
}
