using UnityEngine;

/// <summary>
/// StatusBaseの受け皿になる、ステータスの基底クラス
/// </summary>
public abstract class CharacterStatus : ScriptableObject {
    [Tooltip("StatusBaseをアタッチしてください。")]
    public StatusBase BaseStatus;
    [Tooltip("キャラクター名(職業名)を記入してください。\n日本語でも構いません。")]
    public string DisplayName;
    [Tooltip("このキャラクターが使うスキルを割り当ててください。\n(複数アタッチできますが0番目のものしか使えません。)")]
    public SkillBase[] Skills;
    [Tooltip("このキャラクターが発動可能なパッシブを割り当ててください。\n(複数アタッチできますが0番目のものしか使えません。)")]
    public PassiveBase[] Passives;

    public virtual int MaxHP => BaseStatus.MaxHP;
    public virtual int Attack => BaseStatus.Attack;
    public virtual int MoveSpeed => BaseStatus.MoveSpeed;
}
