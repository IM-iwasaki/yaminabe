using UnityEngine;

[CreateAssetMenu(menuName = "Character/新規キャラクターデータ(全職業対応)")]
public class GeneralCharacterStatus : CharacterStatus {
    //職業の割り当て
    [Header("職業タイプを選択してください。\n・Melee(近接職)\n・Wizard(魔法職)\n・Gunner(間接職)")]
    public CharacterEnum.CharaterType ChatacterType;

    [Header("体力補正値。\nStatusBase + [maxHPCorrection] の値になります。")]
    [Range(-50, 100)] public int maxHPCorrection = 0;
    [Header("攻撃力補正値。\nStatusBase + [attackCorrection]の値になります。")]
    [Range(-7, 20)] public int attackCorrection = 0;
    [Header("移動速度補正値。\nStatusBase + [speedCorrection]の値になります。")]
    [Range(-3, 5)] public int speedCorrection = 0;
    [Header("魔力値。\n[maxMPCorrection] の値になります。")]
    [Range(10, 100)] public int maxMPCorrection = 10;
    [Header("弾倉値。\n[maxMagazine] の値になります。")]
    [Range(1, 50)] public int maxMagazine = 1;

    public override int maxHP => baseStatus.maxHP + maxHPCorrection;
    public override int attack => baseStatus.attack + attackCorrection;
    public override int moveSpeed => baseStatus.moveSpeed + speedCorrection;
}
