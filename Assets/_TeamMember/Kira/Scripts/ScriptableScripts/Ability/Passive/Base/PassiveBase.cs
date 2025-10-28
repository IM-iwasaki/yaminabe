using UnityEngine;

/// <summary>
/// キャラクターパッシブの基底クラス
/// </summary>
public abstract class PassiveBase : ScriptableObject {
    //パッシブ名(ゲーム内で表示する名前)
    public string PassiveName;
    //パッシブ説明(同じくゲーム内で表示)
    public string PassiveDescription;
    //パッシブのアイコン用
    public Sprite PassiveIcon;
    //パッシブのクールタイム
    public float Cooldown;
    //パッシブが発動中か
    public bool IsPassiveActive;

    /// <summary>
    /// Abstruct : パッシブ固有の動作(引数はパッシブの発動者) 毎フレーム呼ばれます。
    /// </summary>
    public abstract void PassiveReflection(CharacterBase user);
}
