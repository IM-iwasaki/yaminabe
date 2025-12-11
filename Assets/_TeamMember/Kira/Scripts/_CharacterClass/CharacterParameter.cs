using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;

/// <summary>
/// Characterの変数管理
/// </summary>
public class CharacterParameter : NetworkBehaviour{
    #region パラメータ変数

    //[Header("基本ステータス")]    
    //最大の体力
    public int maxHP { get; protected set; }
    //現在の体力
    [SyncVar(hook = nameof(ChangeHP))] public int HP;
    //魔法職のみ：攻撃時に消費。時間経過で徐々に回復(攻撃中は回復しない)。
    [SyncVar(hook = nameof(ChangeMP))] public int MP;
    //リロード中か
    [SyncVar(hook = nameof(UpdateReloadIcon))] public bool isReloading = false;
    //基礎攻撃力
    [SyncVar] public int attack;
    //移動速度
    [SyncVar] public int moveSpeed = 5;
    
    public int maxMP { get; protected set; }
    //持っている武器の文字列
    public string currentWeapon { get; protected set; }
    //所属チームの番号(-1は未所属。0、1はチーム所属。)
    [SyncVar] public int TeamID = -1;
    //プレイヤーの名前
    [SyncVar] public string PlayerName = "Default";
    //受けるダメージ倍率
    [System.NonSerialized] public int DamageRatio = 100;
    //サーバーが割り当てるプレイヤー番号（Player1～6）
    [SyncVar] public int playerId = -1;
    // 追加:タハラ プレイヤー準備完了状態
    [SyncVar] public bool ready = true;

    #endregion

    #region Transform系変数

    //視点を要求する方向
    protected Vector2 lookInput { get; private set; }
    //射撃位置
    public Transform firePoint;

    #endregion

    #region bool系変数＆時間管理系変数

    //死亡しているか
    [SyncVar] public bool isDead = false;
    //死亡した瞬間か
    public bool isDeadTrigger { get; protected set; } = false;
    //復活後の無敵時間中であるか
    protected bool isInvincible = false;
    //復活してからの経過時間
    protected float respownAfterTime { get; private set; } = 0.0f;
    //攻撃中か
    public bool isAttackPressed { get; private set; } = false;
    //攻撃を押した瞬間か
    public bool isAttackTrigger { get; protected set; } = false;
    //攻撃開始時間
    public float attackStartTime { get; private set; } = 0.0f;
    //アイテムを拾える状態か
    protected bool isCanPickup = false;
    //インタラクトできる状態か
    protected bool isCanInteruct = false;   
    //スキルを使用できるか
    public bool isCanSkill { get; protected set; } = false;
    //スキル使用後経過時間
    [System.NonSerialized] public float skillAfterTime = 0.0f;
    //ジャンプ入力をしたか
    private bool IsJumpPressed = false;
    
    //接地しているか
    public bool IsGrounded{ get; private set; }

    //LocalUIの参照だけ持つ
    PlayerLocalUIController localUI;

    #endregion

    #region バフ関連変数

    private Coroutine healCoroutine;
    private Coroutine speedCoroutine;
    private Coroutine attackCoroutine;
    private int defaultMoveSpeed;
    private int defaultAttack;
    [Header("バフに使用するエフェクトデータ")]
    [SerializeField] private EffectData buffEffect;

    private readonly string EFFECT_TAG = "Effect";
    private readonly int ATTACK_BUFF_EFFECT = 0;
    private readonly int SPEED_BUFF_EFFECT = 1;
    private readonly int HEAL_BUFF_EFFECT = 2;
    private readonly int DEBUFF_EFFECT = 3;

    #endregion

    public void Initialize(CharacterBase core) {
        localUI = core.GetComponent<PlayerLocalUIController>();

        // デフォルト値保存
        defaultMoveSpeed = moveSpeed;
        defaultAttack = attack;
        defaultAttack = attack;
    }

    /// <summary>
    /// プレイヤーが接地しているか確認する関数
    /// </summary>
    /// <param name="_checkPos"></param>
    public void GroundCheck(Vector3 _checkPos) {
        IsGrounded = Physics.Raycast(_checkPos, Vector3.down, 1.1f);
    }

    /// <summary>
    /// UI用のHP更新関数(第一引数は消せないため無名変数を使用。)
    /// </summary>
    private void ChangeHP(int _, int _newValue) {
        if (!isLocalPlayer && !isClient) return; // 自分のプレイヤーでなければUI更新しない
        if (localUI != null) localUI.ChangeHPUI(maxHP, _newValue);
        else {
#if UNITY_EDITOR
            Debug.LogWarning("UIが存在しないため、HP更新処理をスキップしました。");
#endif
        }
    }
    private void ChangeMP(int _, int _newValue) {
        if (!isLocalPlayer && !isClient) return; // 自分のプレイヤーでなければUI更新しない
        if (localUI != null) localUI.ChangeMPUI(maxMP, _newValue);
        else {
#if UNITY_EDITOR
            Debug.LogWarning("UIが存在しないため、MP更新処理をスキップしました。");
#endif
        }
    }

    /// <summary>
    /// リロードアイコンの処理を発火
    /// hook関数で呼び出す
    /// </summary>
    /// <param name="_">無名変数</param>
    /// <param name="_new">新たに変更された値(今回でいうとisReloading)</param>
    private void UpdateReloadIcon(bool _, bool _new) {
        if (_new)
            localUI.StartRotateReloadIcon();
    }
}
