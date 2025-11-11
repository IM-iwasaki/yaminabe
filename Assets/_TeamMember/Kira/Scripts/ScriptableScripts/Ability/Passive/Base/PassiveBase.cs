using UnityEngine;

/// <summary>
/// キャラクターパッシブの基底クラス
/// </summary>
public abstract class PassiveBase : ScriptableObject {
    //パッシブ名(ゲーム内で表示する名前)
    [Header("パッシブ名を入力してください。\n日本語でも構いません。")]
    public string PassiveName;
    //パッシブ説明(同じくゲーム内で表示)
    [Header("パッシブの説明文を入力してください。\n日本語でも構いません。")]
    [TextArea(3, 6)]public string PassiveDescription;
    //パッシブのアイコン用
    [Header("パッシブのアイコンを割り当ててください。")]
    public Sprite passiveIcon;
    //パッシブが再発動可能になるまでの時間
    [Header("[任意]クールダウンを設定できます。\n(必要に応じて入力してください。)")]
    public float Cooldown;
    //パッシブクールタダウン計測用
    [System.NonSerialized]public float CoolTime;
    //パッシブが発動中か
    public bool IsPassiveActive;

    /// <summary>
    /// Virtual : パッシブのセッティング用関数。(必要に応じて定義してください。)
    /// </summary>
    public virtual void PassiveSetting(CharacterBase user) { 
        
    }

    /// <summary>
    /// Abstruct : パッシブ固有の動作(引数はパッシブの発動者) 毎フレーム呼ばれます。
    /// </summary>
    public abstract void PassiveReflection(CharacterBase user);
}
