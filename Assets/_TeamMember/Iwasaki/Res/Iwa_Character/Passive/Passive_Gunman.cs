using UnityEngine;

[CreateAssetMenu(menuName = "Character/Passive/Gunman_逃げ足")]
public class Passive_Gunman : PassiveBase {

    //
    // パッシブ名　：逃げ足
    // タイプ      ：HP発動型
    // 効果        ：自身の残り体力が20％以下になった時、10秒間の30％のスピードバフが入る
    //               この効果は一度発動すると35秒間は発動しない。
    //

    public override void PassiveSetting(CharacterBase user) {
        //発動中でなかったら発動中の状態にする
        if (!isPassiveActive) {
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
            if (coolTime >= cooldown) {
                isPassiveActive = true;
                //クールタイム計測をリセット
                coolTime = 0;
            }
        }

        //発動中にHPが条件を満たしたら発動。
        if (isPassiveActive && user.HP <= user.maxHP / 5) {
            user.MoveSpeedBuff(0.3f, 10.0f);
            //発動状態を解除
            isPassiveActive = false;
        }
    }
}
