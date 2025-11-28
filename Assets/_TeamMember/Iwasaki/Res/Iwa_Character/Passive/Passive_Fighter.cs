using UnityEngine;

[CreateAssetMenu(menuName = "Character/Passive/Fighter_根性")]
public class Passive_Fighter : PassiveBase {

    //
    // パッシブ名　：根性
    // タイプ      ：HP発動型
    // 効果        ：HPが一定以下には受けるダメージを軽減。
    //クールタイム ：30

    public override void PassiveReflection(CharacterBase user) {
        //発動中でなかったらクールタイムを計測
        if (!isPassiveActive) {
            coolTime += Time.deltaTime;
            //クールタイムがクールダウン以上になったら発動中にする
            if (coolTime >= Cooldown) {
                isPassiveActive = true;
                //クールタイム計測をリセット
                coolTime = 0;
                // 
               
            }
            // 10秒で元に戻るはず
            else if (coolTime >=Cooldown/3) {
                user.DamageRatio = 100;
            }

        }


        //発動中にHPが条件を満たしたら発動。
        if (isPassiveActive && user.HP <= user.maxHP / 4) {
            isPassiveActive=false;
            user.DamageRatio = 50;
        }
       
    }
}
