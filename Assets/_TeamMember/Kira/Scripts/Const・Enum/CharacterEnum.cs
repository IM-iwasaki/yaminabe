using UnityEngine;

public class CharacterEnum {
    /// <summary>
    /// キャラクターの職業を判別するための列挙体
    /// </summary>
    public enum CharaterType {
        Melee = 0,
        Wizard,
        Gunner,
    }

    /// <summary>
    /// メイン攻撃かサブ攻撃かを判別する列挙体
    /// </summary>
    public enum AttackType {
        Main = 0,
        Sub = 1,
    };

    /// <summary>
    /// セミオートかフルオートか判別する列挙体
    /// </summary>
    public enum AutoFireType {
        Invalid = -1,
        SemiAutomatic,
        FullAutomatic
    }
}
