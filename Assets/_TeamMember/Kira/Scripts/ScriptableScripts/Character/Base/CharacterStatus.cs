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

    public virtual int maxHP => baseStatus.maxHP;
    public virtual int attack => baseStatus.attack;
    public virtual int moveSpeed => baseStatus.moveSpeed;
}
