using UnityEngine;

public abstract class SkillBase : ScriptableObject {
    //スキル名(ゲーム内で表示する名前)
    public string SkillName;
    //スキル説明(同じくゲーム内で表示)
    public string SkillDescription;
    //スキルのアイコン用
    public Sprite SkillIcon;
    //スキルのクールタイム
    public float Cooldown;
    //スキルが発動中か
    public bool IsSkillUse;

    // 抽象メソッド：スキル固有の動作を実装する
    public abstract void Activate(GameObject user);
}
