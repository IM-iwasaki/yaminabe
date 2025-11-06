using UnityEngine;

/// <summary>
/// StatusBaseの受け皿になる、ステータスの基底クラス
/// </summary>
public abstract class CharacterStatus : ScriptableObject {
    [Header("StatusBaseをアタッチしてください。")]
    public StatusBase BaseStatus;
    [Header("キャラクター名(職業名)を記入してください。\n日本語でも構いません。")]
    public string DisplayName;
    [Header("このキャラクターが使うスキルを割り当ててください。\n(複数アタッチできますが0番目のものしか使えません。)")]
    public SkillBase[] Skills;
    [Header("このキャラクターが発動可能なパッシブを割り当ててください。\n(複数アタッチできますが0番目のものしか使えません。)")]
    public PassiveBase[] Passives;

    public virtual int MaxHP => BaseStatus.MaxHP;
    public virtual int Attack => BaseStatus.Attack;
    public virtual int MoveSpeed => BaseStatus.MoveSpeed;
}
