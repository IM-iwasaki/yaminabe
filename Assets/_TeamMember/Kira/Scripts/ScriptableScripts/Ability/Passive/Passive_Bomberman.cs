using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName = "Character/Passive/Bomberman_爆風回避")]
public class Passive_Bomberman : PassiveBase {

    public override void PassiveSetting() {
        //発動中でなかったら発動中の状態にする
        if (!isPassiveActive){
            isPassiveActive = true;
            //クールタイム計測をリセット
            coolTime = 0;
        }
    }

    public override void PassiveReflection(CharacterBase user) {

    }
}
