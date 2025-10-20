using UnityEngine;

[CreateAssetMenu(menuName = "Character/MeleeCharacterStatus(近接職)")]
public class MeleeCharacterStatus : CharacterStatus {
    //職業の割り当て
    [Tooltip("職業タイプを選択してください。\n・Melee(近接職)\n・Wizard(魔法職)\n・Gunner(間接職)")]
    public CharacterTypeEnum.CharaterType ChatacterType = CharacterTypeEnum.CharaterType.Melee;
    //キャラクターID。いらないかも、要相談。コメントアウト。
    //[Tooltip("キャラクターID。(int型)\nキャラクターを識別するのに使用します。\n(基本、他と同IDを割り当てないでください。)")]
    //[SerializeField] int CharacterID = 0; 

    [Tooltip("体力補正値。\nStatusBase + [MaxHPCorrection] の値になります。")]
    [Range(-50, 100)] public int MaxHPCorrection = 0;
    [Tooltip("攻撃力補正値。\nStatusBase + [AttackCorrection]の値になります。")]
    [Range(-5, 20)] public int AttackCorrection = 0;
    [Tooltip("移動速度補正値。\nStatusBase + [SpeedCorrection]の値になります。")]
    [Range(-3, 5)] public int SpeedCorrection = 0;

    public override int MaxHP => BaseStatus.MaxHP + MaxHPCorrection;
    public override int Attack => BaseStatus.Attack + AttackCorrection;
    public override int MoveSpeed => BaseStatus.MoveSpeed + SpeedCorrection;

    
}