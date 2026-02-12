using UnityEngine;

[CreateAssetMenu(menuName = "Character/Passive/Millionaire_投資回収")]
public class Passive_Millionaire : PassiveBase {

    //
    // パッシブ名　：投資回収
    // 効果        ：試合に勝利した場合、その試合で消費した金額を少し増やして回収する。
    //

    [Header("倍率")]
    [SerializeField] public float multiple = 1.2f;

    public override void PassiveSetting() {
        //発動中でなかったら発動中の状態にする
        if (!isPassiveActive) {
            isPassiveActive = true;
            //クールタイム計測をリセット
            coolTime = 0;
        }
    }

    public override void PassiveReflection(CharacterBase user) {

    }

}
