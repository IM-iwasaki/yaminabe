using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeCharacterStatus : ScriptableObject {
    public StatusBase BaseStatus;
    [Range(-50, 50)]public int MaxHPCorrection = 0;
    [Range(-50, 50)]public int AttackCorrection = 0;
    [Range(-50, 50)]public int MoveSpeedCorrection = 0;
    [Range(1, 10)]public int AttackSpeed;
    [Range(1, 10)]public int MaxAttackSpeed;
}
