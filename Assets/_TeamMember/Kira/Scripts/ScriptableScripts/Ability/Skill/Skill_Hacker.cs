using UnityEngine;
using Mirror;

/// <summary>
/// スキル：ハッキング
/// ・敵全体の移動速度を大幅低下
/// ・受けるダメージを増加させる
/// </summary>
[CreateAssetMenu(menuName = "Character/Skill/Hacker_ハッキング")]
public class Skill_Hacker : SkillBase {

    public override void Activate(CharacterBase user) {
        if (!user.isLocalPlayer) return;
        Debug.Log("スキル発火");
        user.CmdHackingActivate();    
    }    
}
