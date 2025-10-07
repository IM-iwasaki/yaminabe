using UnityEngine;

[CreateAssetMenu(menuName = "Character/MeleeCharacterStatus(近接職)")]
public class MeleeCharacterStatus : CharacterStatus {
    //職業の割り当て
    public CharacterTypeEnum.CharaterType ChatacterType = CharacterTypeEnum.CharaterType.Melee;
    //キャラクターID。いらないかも。コメントアウト。
    //[Tooltip("キャラクターID。(int型)\nキャラクターを識別するのに使用します。\n(基本、他と同IDを割り当てないでください。)")]
    //[SerializeField] int CharacterID = 0; 

    [Tooltip("体力補正値。\nStatusBase + [MaxHPCorrection] の値になります。")]
    [Range(-50, 50)] public int MaxHPCorrection = 0;
    [Tooltip("攻撃力補正値。\nStatusBase + [AttackCorrection]の値になります。")]
    [Range(-5, 10)] public int AttackCorrection = 0;
    [Tooltip("移動速度補正値。\nStatusBase + [SpeedCorrection]の値になります。")]
    [Range(-1, 2)] public int SpeedCorrection = 0;

    public override int MaxHP => BaseStatus.MaxHP + MaxHPCorrection;
    public override int Attack => BaseStatus.Attack + AttackCorrection;
    public override int MoveSpeed => BaseStatus.MoveSpeed + SpeedCorrection;

    
}