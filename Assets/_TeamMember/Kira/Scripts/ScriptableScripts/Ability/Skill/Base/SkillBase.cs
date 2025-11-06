using UnityEngine;

/// <summary>
/// キャラクタースキルの基底クラス
/// </summary>
public abstract class SkillBase : ScriptableObject {
    //スキル名(ゲーム内で表示する名前)
    [Header("スキル名を入力してください。\n日本語でも構いません。")]
    public string SkillName;
    //スキル説明(同じくゲーム内で表示)
    [Header("スキルの説明文を入力してください。\n日本語でも構いません。")]
    [TextArea(3, 6)] public string SkillDescription;
    //スキルのアイコン用
    [Header("スキルのアイコンを割り当ててください。")]
    public Sprite SkillIcon;
    //スキルのクールダウン
    [Header("スキルのクールダウン時間を設定してください。")]
    [Range(0.1f,30.0f)]public float Cooldown;
    //スキルが発動中か
    public bool IsSkillUse;

    /// <summary>
    /// Abstruct : スキル固有の動作(引数はスキルの発動者。) 発火時1回のみ通過します。
    /// </summary>
    public abstract void Activate(CharacterBase user);

    /// <summary>
    /// Virtual : スキル固有の更新処理　毎フレーム呼ばれます。必要に応じて使用してください。
    /// </summary>
    /// <param name="user"></param>
    public virtual void SkillEffectUpdate(CharacterBase user) { }
}
