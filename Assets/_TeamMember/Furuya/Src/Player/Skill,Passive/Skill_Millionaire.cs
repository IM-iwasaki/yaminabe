using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Millionaire_投資回収")]
public class Skill_Millionaire : SkillBase {

    //
    // パッシブ名　：投資回収
    // 効果        ：与えたダメージの一部をゴールドとして回収する
    //

    [SerializeField]private int money = 100;
    public override void Activate(CharacterBase user) {
        if (!user.isLocalPlayer) return;
        PlayerWallet.Instance.AddMoney(money);
    }
}
