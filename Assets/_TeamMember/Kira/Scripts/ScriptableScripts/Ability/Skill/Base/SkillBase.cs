using UnityEngine;

/// <summary>
/// キャラクタースキルの基底クラス
/// </summary>
public abstract class SkillBase : ScriptableObject {
    //スキル名(ゲーム内で表示する名前)
    public string SkillName;
    //スキル説明(同じくゲーム内で表示)
    public string SkillDescription;
    //スキルのアイコン用
    public Sprite SkillIcon;
    //スキルのクールタイム
    [Range(0.1f,30.0f)]public float Cooldown;
    //スキルが発動中か
    public bool IsSkillUse;

    /// <summary>
    /// Abstruct : スキル固有の動作(引数はスキルの発動者。) 発火時1回のみ通過します。
    /// </summary>
    public abstract void Activate(CharacterBase user);
}
