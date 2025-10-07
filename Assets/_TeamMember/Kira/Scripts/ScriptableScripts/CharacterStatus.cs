using UnityEngine;

/// <summary>
/// StatusBaseの受け皿になる、ステータスの基底クラス
/// </summary>
public abstract class CharacterStatus : ScriptableObject {
    public StatusBase BaseStatus;
    public string DisplayName;
    //public SkillBase[] Skills; // ここに職業固有スキルを登録するけどまだ使わない。

    public virtual int MaxHP => BaseStatus.MaxHP;
    public virtual int Attack => BaseStatus.Attack;
    public virtual int MoveSpeed => BaseStatus.MoveSpeed;
}
