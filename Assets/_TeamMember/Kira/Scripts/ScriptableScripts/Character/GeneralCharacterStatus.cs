using UnityEngine;

[CreateAssetMenu(menuName = "Character/新規キャラクターデータ(全職業対応)")]
public class GeneralCharacterStatus : CharacterStatus {
    //職業の割り当て
    [Header("職業タイプを選択してください。\n・Melee(近接職)\n・Wizard(魔法職)\n・Gunner(間接職)")]
    public CharacterEnum.CharaterType ChatacterType;

    [Header("体力補正値。\nStatusBase + [MaxHPCorrection] の値になります。")]
    [Range(-50, 100)] public int MaxHPCorrection = 0;
    [Header("攻撃力補正値。\nStatusBase + [AttackCorrection]の値になります。")]
    [Range(-7, 20)] public int AttackCorrection = 0;
    [Header("移動速度補正値。\nStatusBase + [SpeedCorrection]の値になります。")]
    [Range(-3, 5)] public int SpeedCorrection = 0;
    [Header("魔力値。\n[MaxMPCorrection] の値になります。")]
    [Range(10, 100)] public int MaxMPCorrection = 10;
    [Header("弾倉値。\n[maxMagazine] の値になります。")]
    [Range(1, 50)] public int MaxMagazine = 1;

    public override int MaxHP => BaseStatus.MaxHP + MaxHPCorrection;
    public override int Attack => BaseStatus.Attack + AttackCorrection;
    public override int MoveSpeed => BaseStatus.MoveSpeed + SpeedCorrection;
}
