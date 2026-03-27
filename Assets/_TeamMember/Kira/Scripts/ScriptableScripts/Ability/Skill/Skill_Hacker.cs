using UnityEngine;
using Mirror;

/// <summary>
/// スキル：ハッキング
/// ・敵全体の移動速度を大幅低下
/// ・受けるダメージを増加させる
/// </summary>
[CreateAssetMenu(menuName = "Character/Skill/Hacker_ハッキング")]
public class Skill_Hacker : SkillBase {

    private bool hasSavedPosition = false;
    private Vector3 savedPosition;

    public override void SkillSetting() {
        hasSavedPosition = false;
    }

    public override void Activate(CharacterBase user) {
        if (!user.isLocalPlayer) return;

        if (!hasSavedPosition) {
            //保存
            savedPosition = user.transform.position;
            hasSavedPosition = true;

            Debug.Log("位置保存");
        }
        else {
            user.CmdUseSkill_MoveSpeed(0.0001f, 2.0f);
            //ワープ
            user.CmdWarp(savedPosition);

            user.DamageCut(1.2f, 5.0f);

            //リセット
            hasSavedPosition = false;

            Debug.Log("ワープ＆リセット");
        }
    }
}