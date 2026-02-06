using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PveBossAttackData : ScriptableObject {

    [Header("スキル名")]
    public string skillName;           // 攻撃スキル名
    [Header("説明")]
    [TextArea(5, 4)]
    public string description;         // 攻撃スキル説明

    /// <summary>
    /// Bossの攻撃をする際に必要、呼び出し先でmainかskillの攻撃呼び出し関数を入れておく
    /// </summary>
    /// <param name="weapon"></param>
    public abstract void StartAttack(
        EnemyWeaponController weapon,
        PveBossController boss,
        Transform target);

}
