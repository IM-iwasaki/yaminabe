using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Millionaire_Œˆ€‚Ì‚")]
public class Skill_Millionaire : SkillBase {

    [SerializeField]private int money = 100;

    public override void Activate(CharacterBase user) {
        PlayerWallet.Instance.AddMoney(money);
    }
}
