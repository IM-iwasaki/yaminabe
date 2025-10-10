using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerConst {
    //キャラクターのデフォルト最大体力値
    public const int DEFAULT_MAXHP = 100;
    //キャラクターのデフォルト攻撃力値
    public const int DEFAULT_ATTACK = 10;
    //キャラクターのデフォルト移動速度
    public const int DEFAULT_MOVESPEED = 6;

    //キャラクターの旋回速度
    public const float TURN_SPEED = 8.0f;

    //ジャンプ力
    public const float JUMP_FORCE = 10.0f;
    //地面判定の距離(長くすると判定が甘くなる)
    public const float GROUND_DISTANCE = 0.3f;

    /// <summary>
    /// メイン攻撃かサブ攻撃かを判別する列挙体
    /// </summary>
    public enum AttackType {
        Main = 0,
        Sub = 1,
    };
}

