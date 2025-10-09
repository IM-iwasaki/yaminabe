using UnityEngine;

/// <summary>
/// StatusBaseの受け皿になる、ステータスの基底クラス
/// </summary>
public abstract class CharacterStatus : ScriptableObject {
    [Tooltip("StatusBaseをアタッチしてください。")]
    public StatusBase BaseStatus;
    [Tooltip("キャラクター名(職業名)を記入してください。\n日本語でも構いません。")]
    public string DisplayName;
    public SkillBase[] Skills; // ここに職業固有スキルを登録する。

    public virtual int MaxHP => BaseStatus.MaxHP;
    public virtual int Attack => BaseStatus.Attack;
    public virtual int MoveSpeed => BaseStatus.MoveSpeed;
}
