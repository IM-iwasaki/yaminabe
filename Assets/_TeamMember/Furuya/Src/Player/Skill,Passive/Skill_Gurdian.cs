using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Gurdian_決死の盾")]
public class Skill_Gurdian : SkillBase {

    //
    //  スキル名：決死の盾
    //  タイプ　：固定防御型
    //  効果    ：短い間その場で停止し、受けるダメージを大幅に軽減する。(80％軽減)
    //            効果終了時に全てのデバフを解除し
    //            損傷ダメージの50％を回復する。(最大回復量は50)
    //　CT      ：12秒
    //

    int SkillDamage = 50;

    public override void Activate(CharacterBase user) {
        //フラグを立てる
        isSkillUse = true;
        //停止
        user.parameter.moveSpeed = 0;
        //被ダメ大幅軽減
        user.DamageCut(20, 2.0f);
        //スキル使用後効果を予約
        user.StartCoroutine(SkillEndEffect(user));

        // 突進
        //user.MoveSpeedBuff(5.0f, 0.3f);
        // 被ダメ半減
        //user.DamageCut(50, 0.3f);
        // ヒットボックス（Serverのみ）
        //if (user.isServer) {
        //    CreateHitBox(user);
        //}
    }

    IEnumerator SkillEndEffect(CharacterBase user, float _delay = 2.0f) {
        yield return new WaitForSeconds(_delay);
        //移動速度を戻す。
        user.parameter.moveSpeed = user.parameter.defaultMoveSpeed;
        //全バフデバフを解除。
        user.RemoveBuff();
        //損傷HPの50％回復。最大回復量は50。
        int recoveryHP = (user.parameter.maxHP - user.parameter.HP) / 2;
        if (recoveryHP > 50) recoveryHP = 50;
        user.parameter.HP += recoveryHP;
        //フラグを下ろす
        isSkillUse = false;
    }

    void CreateHitBox(CharacterBase user) {

        GameObject hitBox = new GameObject("SkillHitbox");

        hitBox.transform.position =
            user.transform.position + user.transform.forward * 1.0f;
        hitBox.transform.rotation = user.transform.rotation;

        SphereCollider col = hitBox.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 1.5f;

        SkillHitbox hb = hitBox.AddComponent<SkillHitbox>();
        hb.Initialize(
            user.transform,
            user.parameter.TeamID,
            SkillDamage,
            user.parameter.PlayerName,
            user.parameter.playerId
        );

        Object.Destroy(hitBox, 0.3f);
    }
}
