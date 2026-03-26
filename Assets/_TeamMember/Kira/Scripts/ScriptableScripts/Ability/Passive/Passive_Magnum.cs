using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName = "Character/Passive/Magnum_生存本能")]
public class Passive_Magnum : PassiveBase {

    //
    // パッシブ名　：生存本能
    // タイプ      ：HP発動型
    // 効果        ：HPが減っている時、短い間速度↑↑↑、無敵になる。
    //               一度発動すると倒されるか20秒間は発動しない。


    public override void PassiveSetting() {
        //発動中でなかったら発動中の状態にする
        if (!isPassiveActive){
            isPassiveActive = true;
            //クールタイム計測をリセット
            coolTime = 0;
        }
    }

    public override void PassiveReflection(CharacterBase user) {
        if(isPassiveActive && user.parameter.HP < user.parameter.maxHP) {
            //攻撃を受けたら発動。無敵を一度解除し速度↑、2秒間の無敵を付与する。
            user.CmdUseSkill_MoveSpeed(2.0f,2.0f);
            // 無敵状態を開始（2秒間）
            user.CmdInvincibleRequast(user,2.0f);
            //発動状態を解除
            isPassiveActive= false;
        }        
        //発動中でなかったらクールタイムを計測
        if (!isPassiveActive) {
            coolTime += Time.deltaTime;
            //クールタイムがクールダウン以上になったら発動中にする
            if(coolTime >= cooldown) {
                isPassiveActive = true;
                //クールタイム計測をリセット
                coolTime = 0;
            }
        }

        //発動中でなかったらクールタイムを計測
        //if (!isPassiveActive) {
        //    coolTime += Time.deltaTime;
        //    //クールタイムがクールダウン以上になったら発動中にする
        //    if(coolTime >= cooldown) {
        //        isPassiveActive = true;
        //        //クールタイム計測をリセット
        //        coolTime = 0;
        //    }
        //}
        //
        ////発動中にHPが条件を満たしたら発動。
        //if (isPassiveActive && user.parameter.HP <= user.parameter.maxHP / 2 ) {
        //    //user.MoveSpeedBuff(2.0f,2.0f);
        //    user.CmdUseSkill_MoveSpeed(2.0f,2.0f);
        //    // 無敵状態を開始（2秒間）
        //    user.CmdInvincibleRequast(user,2.0f);
        //    //発動状態を解除
        //    isPassiveActive= false;
        //}
    }
}
