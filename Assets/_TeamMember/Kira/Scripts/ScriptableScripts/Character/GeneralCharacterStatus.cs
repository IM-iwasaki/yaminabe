using UnityEngine;

[CreateAssetMenu(menuName = "Character/新規キャラクターデータ(全職業対応)")]
public class GeneralCharacterStatus : CharacterStatus {
    //職業の割り当て
    [Header("職業タイプを選択してください。\n・Melee(近接職)\n・Wizard(魔法職)\n・Gunner(間接職)")]
    public CharacterEnum.CharaterType chatacterType;

    [Header("体力補正値。\nStatusBase + [maxHPCorrection] の値になります。")]
    [Range(-50, 100)] public int maxHPCorrection = 0;    
    [Header("移動速度補正値。\nStatusBase + [speedCorrection]の値になります。")]
    [Range(-3, 5)] public int speedCorrection = 0;
    [Header("攻撃力補正値。\n[attack]の値になります。")]
    [Range(0, 50)] public int attack = 0;
    [Header("魔力値。\n[maxMP] の値になります。")]
    [Range(10, 100)] public int maxMP = 10;

    public override int maxHP => baseStatus.maxHP + maxHPCorrection;
    public override int moveSpeed => baseStatus.moveSpeed + speedCorrection;

    //TODO:これも多分消す。
    public override int baseAttack => baseAttack + attack;
}
