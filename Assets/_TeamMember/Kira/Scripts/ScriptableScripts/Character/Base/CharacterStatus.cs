using UnityEngine;

/// <summary>
/// StatusBaseの受け皿になる、ステータスの基底クラス
/// </summary>
public abstract class CharacterStatus : ScriptableObject {
    [Header("StatusBaseをアタッチしてください。")]
    public StatusBase baseStatus;
    [Header("キャラクター名(職業名)を記入してください。\n日本語でも構いません。")]
    public string displayName;
    [Header("このキャラクターが使うスキルを割り当ててください。\n(複数アタッチできますが0番目のものしか使えません。)")]
    public SkillBase[] skills;
    [Header("このキャラクターが発動可能なパッシブを割り当ててください。\n(複数アタッチできますが0番目のものしか使えません。)")]
    public PassiveBase[] passives;
    [Header("このキャラクターの初期メイン武器を割り当ててください。")]
    public WeaponData MainWeapon;
    [Header("このキャラクターの初期サブ武器を割り当ててください。")]
    public SubWeaponData SubWeapon;

    //
    // ※SkillBaseとPassiveBaseは配列ですが0番目のみ使用してください。
    // 　これは拡張性を持たせるためにやっていましたが
    // 　2つめは実装しない方向性になったので未使用になりました。
    //

    public virtual int maxHP => baseStatus.maxHP;
    public virtual int moveSpeed => baseStatus.moveSpeed;

    public virtual int baseAttack => baseStatus.attack;
}