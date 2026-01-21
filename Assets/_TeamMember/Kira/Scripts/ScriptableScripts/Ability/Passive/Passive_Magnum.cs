using UnityEngine;

[CreateAssetMenu(menuName = "Character/Passive/Magnum_生存本能")]
public class Passive_Magnum : PassiveBase {

    //
    // パッシブ名　：生存本能
    // タイプ      ：HP発動型
    // 効果        ：自身の残り体力が20％以下になった時、移動スピードが5秒間600%アップする
    //               この効果は一度発動すると15秒間は発動しない。


    public override void PassiveSetting() {
        //発動中でなかったら発動中の状態にする
        if (!isPassiveActive){
            isPassiveActive = true;
            //クールタイム計測をリセット
            coolTime = 0;
        }
    }

    public override void PassiveReflection(CharacterBase user) {
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

        //発動中にHPが条件を満たしたら発動。
        if (isPassiveActive && user.parameter.HP <= user.parameter.maxHP / 5 ) {

            user.MoveSpeedBuff(2f,5.0f);
            //発動状態を解除
            isPassiveActive= false;
        }
    }
}
