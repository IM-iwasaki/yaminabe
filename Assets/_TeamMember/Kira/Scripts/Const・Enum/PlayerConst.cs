using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConst {
    public const int DEFAULT_MAXHP = 100;
    public const int DEFAULT_ATTACK = 10;
    public const int DEFAULT_MOVESPEED = 6;

    public const float TURN_SPEED = 8.0f;

    //ƒWƒƒƒ“ƒv—Í
    public const float JUMP_FORCE = 10.0f;
    //’n–Ê”»’è‚Ì‹——£(’·‚­‚·‚é‚Æ”»’è‚ªŠÃ‚­‚È‚é)
    public const float GROUND_DISTANCE = 0.3f;

    public enum AttackType {
        Main = 0,
        Sub = 1,
    };
}

