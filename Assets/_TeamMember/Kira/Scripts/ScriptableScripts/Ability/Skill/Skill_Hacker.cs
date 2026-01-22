using UnityEngine;

/// <summary>
/// スキル：ハッキング
/// ・敵全体の移動速度を大幅低下
/// ・受けるダメージを増加させる
/// </summary>
[CreateAssetMenu(menuName = "Character/Skill/Hacker_ハッキング")]
public class Skill_Hacker : SkillBase {

    public override void Activate(CharacterBase user) {

        CharacterParameter selfParam = user.GetComponent<CharacterParameter>();
        if (selfParam == null) return;

        // 全キャラクター取得
        CharacterParameter[] allPlayers =
            FindObjectsOfType<CharacterParameter>();

        foreach (CharacterParameter target in allPlayers) {

            // 自分は除外
            if (target == selfParam) continue;

            // 未所属 or 同チームは除外
            if (target.TeamID == -1) continue;
            if (target.TeamID == selfParam.TeamID) continue;

            CharacterBase enemy = target.GetComponent<CharacterBase>();
            if (enemy == null) continue;

            // 移動速度を30%に低下（4秒）
            enemy.MoveSpeedBuff(0.3f, 4.0f);

            // 被ダメージ1.25倍（4秒）
            enemy.DamageCut(125, 4.0f);
        }
    }
}
