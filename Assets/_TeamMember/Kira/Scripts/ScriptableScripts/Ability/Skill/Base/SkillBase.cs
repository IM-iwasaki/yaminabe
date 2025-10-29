using UnityEngine;

/// <summary>
/// キャラクタースキルの基底クラス
/// </summary>
public abstract class SkillBase : ScriptableObject {
    //スキル名(ゲーム内で表示する名前)
    [Tooltip("スキル名を入力してください。\n日本語でも構いません。")]
    public string SkillName;
    //スキル説明(同じくゲーム内で表示)
    [Tooltip("スキルの説明文を入力してください。\n日本語でも構いません。")]
    [TextArea(3, 6)] public string SkillDescription;
    //スキルのアイコン用
    [Tooltip("スキルのアイコンを割り当ててください。")]
    public Sprite SkillIcon;
    //スキルのクールダウン
    [Tooltip("スキルのクールダウン時間を設定してください。")]
    [Range(0.1f,30.0f)]public float Cooldown;
    //スキルが発動中か
    public bool IsSkillUse;

    /// <summary>
    /// Abstruct : スキル固有の動作(引数はスキルの発動者。) 発火時1回のみ通過します。
    /// </summary>
    public abstract void Activate(CharacterBase user);
}
