using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/Skill/Assault_迅速切込")]
public class Skill_Assault : SkillBase {

    //
    //  スキル名：迅速切込
    //  タイプ　：自己強化型
    //  効果    ：一瞬の間、移動速度を大幅に上昇。
    //　CT      ：14秒
    //

    //前に移動する力の強さ
    private readonly float forwardPower = 70.0f;
    //上に移動する力の強さ
    private readonly float upPower = 10.0f;

    private readonly float stayAddPower = 0.1f;

    public override void Activate(CharacterBase user) {

        user.StartCoroutine(DashCharacter(user));

    }

    private IEnumerator DashCharacter(CharacterBase user) {
        //  前に押し出す前に少し上に力を加える
        user.rb.velocity = user.transform.up * upPower;
        //  前方に力を加えるまでの時間に遅延を与える
        yield return new WaitForSeconds(stayAddPower);
        //前方に力を加える
        user.rb.velocity = user.transform.forward * forwardPower;
    }

}
