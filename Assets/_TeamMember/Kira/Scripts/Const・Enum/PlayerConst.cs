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
    //ジャンプ上昇時の重力補正
    public const float JUMP_UPFORCE = 1.8f;
    //ジャンプ上昇終了時の重力補正
    public const float JUMP_DOWNFORCE = 2.5f;
    //地面判定の距離(長くすると判定が甘くなる)
    public const float GROUND_DISTANCE = 0.2f;

    //リスポーンに必要な時間
    public const float RespownTime = 3.0f;
    //リスポーン後の無敵時間
    public const float RespownInvincibleTime = 1.5f;
}