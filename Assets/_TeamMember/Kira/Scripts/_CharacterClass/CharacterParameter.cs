using Mirror;
using UnityEngine;
using System.Linq;

/// <summary>
/// Characterの変数管理
/// </summary>
public class CharacterParameter : NetworkBehaviour{
    #region ～キャラクターデータ管理変数～

    [Header("インポートするステータス")]
    [SerializeField]GeneralCharacterStatus inputStatus;
    //CharacterStatusをキャッシュ(ScriptableObjectを書き換えないための安全策)
    GeneralCharacterStatus runtimeStatus;
    public SkillBase[] equippedSkills{ get; private set; }
    public PassiveBase[] equippedPassives{ get; private set; }

    #endregion

    #region パラメータ変数

    //[Header("基本ステータス")]    
    //最大の体力
    public int maxHP { get; protected set; }
    //最大MP
    public int maxMP { get; protected set; }
    //現在の体力
    [SyncVar(hook = nameof(ChangeHP))] public int HP;
    //魔法職のみ：攻撃時に消費。時間経過で徐々に回復(攻撃中は回復しない)。
    [SyncVar(hook = nameof(ChangeMP))] public int MP;
    //リロード中か
    [SyncVar(hook = nameof(UpdateReloadIcon))] public bool isReloading = false;
    //基礎攻撃力
    [SyncVar] public float attack;
    //移動速度
    [SyncVar] public int moveSpeed = 5;
    
    
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
    
    public float defaultAttack { get; protected set; }
    public int defaultMoveSpeed { get; protected set; }

    // 味方検知用
    public bool HasNearbyAlly { get; private set; }

    #endregion

    #region Transform系変数

    //足元のTransform
    [SerializeField]public Transform footPoint;
    //射撃位置
    [SerializeField]private Transform firePoint;

    #endregion

    #region bool系変数＆時間管理系変数

    //死亡しているか
    [SyncVar] public bool isDead = false;
    //死亡した瞬間か
    public bool isDeadTrigger { get; private set; } = false;
    //復活後の無敵時間中であるか
    public bool isInvincible { get; private set; } = false;
    //復活してからの経過時間
    public float respownAfterTime { get; private set; } = 0.0f;
    //攻撃した瞬間か
    public bool AttackTrigger = false;
    //攻撃開始時間
    public float attackStartTime { get; private set; } = 0.0f;

    public bool canMove = true;
    //スキル使用後経過時間
    [System.NonSerialized] public float skillAfterTime = 0.0f;
    
    //接地しているか
    public bool IsGrounded { get; private set; }
    //GroundLayer
    public LayerMask GroundLayer { get; private set; }

    //LocalUIの参照だけ持つ
    private PlayerLocalUIController localUI;
    private MainWeaponController weaponController_main;
    private SubWeaponController weaponController_sub;

    #endregion

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(CharacterBase core) {
        //自身だけ
        if (!isLocalPlayer) return;

        //ステータスのインポート
        StatusInport(inputStatus);

        // "Ground" という名前のレイヤーを取得してマスク化
        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        GroundLayer = 1 << groundLayerIndex;

        localUI = core.GetComponent<PlayerLocalUIController>();        

        isDead = false;
        isInvincible = false;        
        respownAfterTime = 0;
        attackStartTime = 0;
        skillAfterTime = 0;

        //デスカメラのリセット(保険。要らないかも)
        gameObject.GetComponentInChildren<PlayerCamera>().ExitDeathView();

        //Passive関連の初期化
        equippedPassives[0].coolTime = 0;
        equippedPassives[0].isPassiveActive = false;
        //Skill関連の初期化
        equippedSkills[0].isSkillUse = false;
    }

    /// <summary>
    /// ステータスのインポート
    /// </summary>
     public void StatusInport(GeneralCharacterStatus _inport = null) {
        if (_inport == null) {
            Debug.Log("ステータス無いで");
            DefaultStatusInport();
            return;
        }

        inputStatus = _inport;

        runtimeStatus = _inport;
        maxHP = runtimeStatus.maxHP;
        HP = maxHP;
        maxMP = runtimeStatus.maxMP;
        MP = maxMP;
        attack = runtimeStatus.attack;
        moveSpeed = runtimeStatus.moveSpeed;
        equippedSkills = runtimeStatus.skills;
        equippedPassives = runtimeStatus.passives;
        /* xxx.Where() <= nullでないか確認する。 xxx.Select() <= 指定した変数を取り出す。 ※using System.Linq が必要。 */        
        Debug.Log("ステータス、パッシブ、スキルのインポートを行いました。\n" +
            "インポートしたステータス... キャラクター:" + runtimeStatus.displayName + "　maxHP:" + maxHP + "　attack:" + attack + "　moveSpeed:" + moveSpeed + "\n" +
            "インポートしたパッシブ..." + string.Join(", ", equippedPassives.Where(i => i != null).Select(i => i.passiveName)) +
            "　：　インポートしたスキル..." + string.Join(", ", equippedSkills.Where(i => i != null).Select(i => i.skillName)));
        // パッシブの初期セットアップ
        equippedPassives[0].PassiveSetting();
        // デフォルトステータスを代入
        InDefaultStatus();

        // メインウェポンとサブウェポンの参照を取得
        weaponController_main = GetComponent<MainWeaponController>();
        weaponController_sub = GetComponent<SubWeaponController>();

        //キャラクターの職業設定
        weaponController_main.SetCharacterType(runtimeStatus.chatacterType);

        //初期武器の設定
        var mainWeapon = runtimeStatus.MainWeapon.WeaponName;
        var subWeapon = runtimeStatus.SubWeapon.WeaponName;

        weaponController_main.CmdSetWeaponData(mainWeapon);
        //weaponController_main.SetWeaponDataInit(mainWeapon);
        weaponController_sub.SetWeaponData(subWeapon);
    }

    /// <summary>
    /// StatusInportでnullが発生した時にデフォルトの値で初期化する
    /// </summary>
    private void DefaultStatusInport() {
#if UNITY_EDITOR
        Debug.LogWarning("InputStatusに値が入っていなかったため、デフォルト値で初期化を行いました。");
#endif
        maxHP = PlayerConst.DEFAULT_MAXHP;
        HP = maxHP;
        attack = PlayerConst.DEFAULT_ATTACK;
        moveSpeed = PlayerConst.DEFAULT_MOVESPEED;
        // デフォルトステータスを代入
        InDefaultStatus();
    }

    /// <summary>
    /// 初期値を保存する
    /// </summary>
    private void InDefaultStatus() {
        defaultAttack = attack;
        defaultMoveSpeed = moveSpeed;
    }

    /// <summary>
    /// 現在の攻撃力をデフォルトにリセットする
    /// </summary>
    private void OutDefaultStatus_Attack() {
        attack = defaultAttack;
    }

    /// <summary>
    /// 現在の速度をデフォルトにリセットする
    /// </summary>
    public void OutDefaultStatus_MoveSpeed() {
        moveSpeed = defaultMoveSpeed;
    }

    /// <summary>
    /// プレイヤーが接地しているか確認する関数
    /// </summary>
    /// <param name="_checkPos"></param>
    public void GroundCheck(Vector3 _checkPos) {
        // 地面判定（下方向SphereCastでもOK。そこまで深く考えなくていいかも。）
        IsGrounded = Physics.CheckSphere(_checkPos, PlayerConst.GROUND_DISTANCE, GroundLayer);
    }

    /// <summary>
    /// UI用のHP更新関数(第一引数は消せないため無名変数を使用。)
    /// </summary>
    public void ChangeHP(int _, int _newValue) {
        if (!isLocalPlayer && !isClient) return; // 自分のプレイヤーでなければUI更新しない
        if (localUI != null) localUI.ChangeHPUI(maxHP, _newValue);
        else {
#if UNITY_EDITOR
            Debug.LogWarning("UIが存在しないため、HP更新処理をスキップしました。");
#endif
        }
    }
    public void ChangeMP(int _, int _newValue) {
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
        if (!isLocalPlayer) return;

        if (_new)
            localUI.StartRotateReloadIcon();
    }

    /// <summary>
    /// 最後に攻撃した時間を取得する
    /// </summary>
    public void AttackStartTimeRecord() {
        if (!isLocalPlayer) return;
        attackStartTime = Time.time;
    }

    /// <summary>
    /// 攻撃に使用する向いている方向を取得する関数
    /// </summary>
    public Vector3 GetShootDirection() {
        Camera cam = Camera.main;
        Vector3 screenCenter = new(Screen.width / 2f, Screen.height / 2f, 0f);

        // カメラ中心から遠方の目標点を決める（壁は無視）
        Ray camRay = cam.ScreenPointToRay(screenCenter);
        Vector3 aimPoint = camRay.GetPoint(30f); // 30m先に仮のターゲット
        // firePoint から aimPoint 方向にレイを飛ばして壁判定
        Vector3 direction = (aimPoint - firePoint.position).normalized;
        if (Physics.Raycast(firePoint.position, direction, out RaycastHit hit, 1.0f)) {
            // 壁や床に当たればその位置に補正
            return (hit.point - firePoint.position).normalized;
        }
        // 当たらなければそのままaimPoint方向
        return direction;
    }

    /// <summary>
    /// 近くに味方がいるか判別　古谷
    /// </summary>
    public void UpdateNearbyAlly(float radius, LayerMask allyLayer) {
        HasNearbyAlly = Physics.CheckSphere(
            transform.position,
            radius,
            allyLayer
        );
    }

    /// <summary>
    /// 死亡トリガーを発火する
    /// </summary>
    public void StartDeadTrigger() {
        isDeadTrigger = true;
    }

    /// <summary>
    /// 復活時の無敵状態の初期化
    /// </summary>
    public void StartInvincible() {
        //リスポーン後の無敵時間にする
        isInvincible = true;
        //経過時間をリセット
        respownAfterTime = 0;
    }
}
