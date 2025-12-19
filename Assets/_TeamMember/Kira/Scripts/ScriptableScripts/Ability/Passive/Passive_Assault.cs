using UnityEngine;

[CreateAssetMenu(menuName = "Character/Passive/Assault_背水の陣【癒】")]
public class Passive_Assault : PassiveBase {

    //
    // パッシブ名　：背水の陣【癒】
    // タイプ      ：HP発動型
    // 効果        ：自身の残り体力が20％以下になった時、2秒掛けてHPを30％回復する。
    //               この効果は一度発動すると35秒間は発動しない。
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
            user.Heal(0.3f,2.0f);
            //発動状態を解除
            isPassiveActive= false;
        }
    }
}
