using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Gurdian_猪突猛進")]
public class Skill_Gurdian : SkillBase {

    //
    //  スキル名：猪突猛進
    //  タイプ　：攻撃型
    //  効果    ：前方に力強く突進する、使用中は受けるダメージを半減する
    //　CT      ：12秒
    //

    public WeaponData weaponData;

    public int SkillDamage = 50;

    public override void Activate(CharacterBase user) {
        // 突進
        user.MoveSpeedBuff(5.0f, 0.3f);

        // 被ダメ半減
        user.DamageCut(50, 0.3f);

        // ヒットボックス（Serverのみ）
        if (user.isServer) {
            CreateHitBox(user);
        }
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
            user.parameter.PlayerName
        );

        Object.Destroy(hitBox, 0.3f);
    }
}
