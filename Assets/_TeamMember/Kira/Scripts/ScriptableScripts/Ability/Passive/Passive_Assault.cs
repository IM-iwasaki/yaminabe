using UnityEngine;

[CreateAssetMenu(menuName = "Character/Passive/Assault_背水の陣【癒】")]
public class Passive_Assault : PassiveBase {

    //
    // パッシブ名　：背水の陣【癒】
    // タイプ      ：HP発動型
    // 効果        ：自身の残り体力が20％以下になった時、2秒掛けてHPを30％回復する。
    //               この効果は一度発動すると35秒間は発動しない。
    //

    public override void PassiveSetting(CharacterBase user) {
        //発動中でなかったら発動中の状態にする
        if (!IsPassiveActive){
            //仮。
            Debug.Log("パッシブが発動できる状態になりました。");
            IsPassiveActive = true;
            //クールタイム計測をリセット
            CoolTime = 0;
        }
    }

    public override void PassiveReflection(CharacterBase user) {
        //発動中でなかったらクールタイムを計測
        if (!IsPassiveActive) {
            CoolTime += Time.deltaTime;
            //クールタイムがクールダウン以上になったら発動中にする
            if(CoolTime >= Cooldown) {
                //仮。
                Debug.Log("パッシブが発動できる状態になりました。");
                IsPassiveActive = true;
                //クールタイム計測をリセット
                CoolTime = 0;
            }
        }

        //発動中にHPが条件を満たしたら発動。
        if (IsPassiveActive && user.HP <= user.maxHP/5 ) {
            Debug.Log("パッシブが発動しました。");
            user.Heal(0.3f,2.0f);
            //発動状態を解除
            IsPassiveActive= false;
        }
    }
}
