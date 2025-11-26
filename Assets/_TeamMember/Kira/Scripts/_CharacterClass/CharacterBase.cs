using Mirror;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static TeamData;
[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(NetworkTransformHybrid))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]

/// <summary>
/// 初期化をここで行う。
/// </summary>
public abstract class CharacterBase : NetworkBehaviour {

    [Header("基本ステータス")]
    //現在の体力
    [SyncVar(hook = nameof(ChangeHP))] public int HP;
    //最大の体力
    public int maxHP { get; protected set; }
    //基礎攻撃力
    [SyncVar] public int attack;
    //移動速度
    [SyncVar] public int moveSpeed = 5;
    //魔法職のみ：攻撃時に消費。時間経過で徐々に回復(攻撃中は回復しない)。
    public int MP { get; protected set; }
    public int maxMP { get; protected set; }
    //持っている武器の文字列
    public string currentWeapon { get; protected set; }
    //所属チームの番号(-1は未所属。0、1はチーム所属。)
    [SyncVar] public int TeamID = -1;
    //プレイヤーの名前
    [SyncVar] public string PlayerName = "Default";
    //受けるダメージ倍率
    [System.NonSerialized] public int DamageRatio = 100;

    //ランキング用変数の仮定義
    public int score = 0;


    //移動を要求する方向
    protected Vector2 MoveInput;
    //実際に移動する方向
    public Vector3 moveDirection { get; private set; }
    //視点を要求する方向
    protected Vector2 lookInput { get; private set; }
    //向いている方向
    public Vector3 lookDirection { get; private set; }

    //リスポーン地点
    public Vector3 respownPosition { get; protected set; }

    //射撃位置
    public Transform firePoint;


    //死亡しているか
    [SyncVar] protected bool isDead = false;
    //死亡した瞬間か
    public bool isDeadTrigger { get; protected set; } = false;
    //復活後の無敵時間中であるか
    protected bool isInvincible = false;
    //復活してからの経過時間
    protected float respownAfterTime { get; private set; } = 0.0f;

    //移動中か
    public bool isMoving { get; private set; } = false;
    //攻撃中か
    public bool isAttackPressed { get; private set; } = false;
    //攻撃を押した瞬間か
    public bool isAttackTrigger { get; protected set; } = false;
    //攻撃開始時間
    public float attackStartTime { get; private set; } = 0;
    //オート攻撃タイプ (デフォルトはフルオート)
    public bool isAutoAttackRunning { get; private set; }

    //アイテムを拾える状態か
    protected bool isCanPickup = false;
    //インタラクトできる状態か
    protected bool isCanInteruct = false;

    //リロード中か
    [SyncVar(hook = nameof(UpdateReloadIcon))] public bool isReloading = false;

    //スキルを使用できるか
    public bool isCanSkill { get; protected set; } = false;
    //スキル使用後経過時間
    [System.NonSerialized] public float skillAfterTime = 0.0f;

    //コンポーネント情報
    [Header("コンポーネント情報")]
    protected new Rigidbody rigidbody;
    protected Collider useCollider;
    private string useTag;
    [SerializeField] public PlayerUIController UI = null;
    public PlayerLocalUIController localUI = null;
    [SerializeField] private OptionMenu CameraMenu;
    [SerializeField] private InputActionAsset inputActions;
    public Animator anim = null;
    private string currentAnimation;

    [SyncVar] public int playerId = -1;  //  サーバーが割り当てるプレイヤー番号（Player1〜6）
    /// <summary>
    /// 追加:タハラ
    /// プレイヤー準備完了状態
    /// </summary>
    [SyncVar] public bool ready = true;

    //武器を使用するため
    [Header("アクション用変数")]
    public MainWeaponController weaponController_main;
    public SubWeaponController weaponController_sub;
    //ジャンプ入力をしたか
    private bool IsJumpPressed = false;
    //GroundLayer
    private LayerMask GroundLayer;
    //足元の確認用Transform
    private Transform GroundCheck;
    //接地しているか
    [SerializeField] private bool IsGrounded;

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


    #region ～初期化関係関数～

    /// <summary>
    /// 初期化をここで行う。
    /// </summary>
    protected void Awake() {
        //シーン変わったりしても消えないようにする
        DontDestroyOnLoad(gameObject);

        //コンテキストの登録
        var map = inputActions.FindActionMap("Player");
        foreach (var action in map.actions) {
            action.started += ctx => OnInputStarted(action.name, ctx);
            action.performed += ctx => OnInputPerformed(action.name, ctx);
            action.canceled += ctx => OnInputCanceled(action.name, ctx);
        }
        map.Enable();

        rigidbody = GetComponent<Rigidbody>();

        // "Ground" という名前のレイヤーを取得してマスク化
        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        GroundLayer = 1 << groundLayerIndex;

        //GroundCheck変数をアタッチする。
        GroundCheck = transform.Find("FootRoot");

        // デフォルト値保存
        defaultMoveSpeed = moveSpeed;
        defaultAttack = attack;
    }

    /// <summary>
    /// ネットワーク上での初期化。
    /// </summary>
    public override void OnStartLocalPlayer() {
        if (isLocalPlayer) {
            Camera camera = GetComponentInChildren<Camera>();
            camera.tag = "MainCamera";
            camera.enabled = true;
            PlayerCamera playerCamera = camera.GetComponent<PlayerCamera>();
            playerCamera.enabled = true;

            PlayerData data = PlayerSaveData.Load();
            if (!string.IsNullOrEmpty(data.playerName)) {
                CmdSetPlayerName(data.playerName);
            }

            //古谷
            // 子に既に配置されている ReticleOptionUI を探して初期化（ローカル用）
            var option = GetComponentInChildren<ReticleOptionUI>(true);
            if (option != null) {
                option.Initialize(true);
            }
            else {
                Debug.LogWarning("PlayerSetup: No ReticleOptionUI found as child for local player.");
            }

            //タハラ
            //準備状態を明示的に初期化
            //ホストでなければ非準備状態
            if (isLocalPlayer && !isServer)
                ready = false;
        }
    }
    public override void OnStartClient() {
        if (isLocalPlayer) {
            base.OnStartClient();
            GameObject GameUIRoot = GameObject.Find("GameUI");
            var playerUI = Instantiate(UI, GameUIRoot.transform);
            UI = playerUI.GetComponent<PlayerUIController>();
            UI.Initialize(HP);
        }

        // ここを追加：クライアント側で TeamGlowManager に登録
        if (TeamGlowManager.Instance != null) {
            TeamGlowManager.Instance.RegisterPlayer(this);
        }



    }

    /// <summary>
    /// ステータスのインポート
    /// </summary>
    public abstract void StatusInport(GeneralCharacterStatus _inport = null);

    /// <summary>
    /// StatusInportでnullが発生した時にデフォルトの値で初期化する
    /// </summary>
    protected void DefaultStatusInport() {
        Debug.LogWarning("InputStatusに値が入っていなかったため、デフォルト値で初期化を行いました。");
        maxHP = PlayerConst.DEFAULT_MAXHP;
        HP = maxHP;
        attack = PlayerConst.DEFAULT_ATTACK;
        moveSpeed = PlayerConst.DEFAULT_MOVESPEED;
    }

    /// <summary>
    /// 初期値を保存する
    /// </summary>
    protected void InDefaultStatus() {
        defaultAttack = attack;
        defaultMoveSpeed = moveSpeed;
    }

    /// <summary>
    /// プレイヤー名用セッター
    /// 名前をサーバー側で反映し、PlayerListManager に登録する
    /// </summary>
    [Command]
    public void CmdSetPlayerName(string name) {
        PlayerName = name;
        Debug.Log($"[CharacterBase] 名前設定: {PlayerName}");

        // 名前が確定したタイミングで登録（サーバー側のみ）
        if (isServer && PlayerListManager.Instance != null) {
            PlayerListManager.Instance.RegisterPlayer(this);
        }
    }
    // 名前をリストから消す
    public override void OnStopServer() {
        base.OnStopServer();
        if (PlayerListManager.Instance != null) PlayerListManager.Instance.UnregisterPlayer(this);
    }


    #endregion

    #region ～プレイヤー状態更新関数～

    /// <summary>
    /// プレイヤー状態を初期化する関数
    /// (職業限定ステータスの初期化はoverrideを使用してください。)
    /// </summary>
    public virtual void Initalize() {
        HP = maxHP;

        isDead = false;
        isInvincible = false;
        isMoving = false;
        isAttackPressed = false;
        isAttackTrigger = false;
        isCanPickup = false;
        isCanInteruct = false;
        isCanSkill = false;

        respownAfterTime = 0;
        attackStartTime = 0;
        skillAfterTime = 0;

        //デスカメラのリセット(保険。要らないかも)
        gameObject.GetComponentInChildren<PlayerCamera>().ExitDeathView();
    }
    /// <summary>
    /// 追加:タハラ
    /// クライアント用準備状態切り替え関数
    /// </summary>
    [Command]
    private void CmdChangePlayerReady() {
        if (SceneManager.GetActiveScene().name == GameSceneManager.Instance.gameSceneName)
            return;
        ready = !ready;
        ChatManager.instance.CmdSendSystemMessage(PlayerName + " ready :  " + ready);
    }

    /// <summary>
    /// 被弾・死亡判定関数
    /// </summary>
    [Server]
    public void TakeDamage(int _damage, string _name) {
        //既に死亡状態かロビー内なら帰る
        if (isDead || !GameManager.Instance.IsGameRunning()) return;

        //ダメージ倍率を適用
        float damage = _damage * ((float)DamageRatio / 100);
        //ダメージが0以下だったら1に補正する
        if (damage <= 0) damage = 1;
        //HPの減算処理
        HP -= (int)damage;

        //　nameをスコア加算関数み送る
        if (HP <= 0) {
            HP = 0;
            //  キルログを流す(最初の引数は一旦仮で海老の番号、本来はバナー画像の出したい番号を入れる)
            KillLogManager.instance.CmdSendKillLog(4, _name, PlayerName);
            Dead(_name);
            if (PlayerListManager.Instance != null) {
                // スコア加算
                PlayerListManager.Instance.AddScoreByName(_name, 100);
            }
            // キル数加算
            PlayerListManager.Instance?.AddKill(_name);
        }
    }

    /// <summary>
    /// PlayerLocalUIControllerの取得用ゲッター
    /// </summary>
    /// <returns></returns>
    public PlayerLocalUIController GetPlayerLocalUI() { return GetComponent<PlayerLocalUIController>(); }

    /// <summary>
    /// UI用のHP更新関数(第一引数は消せないため無名変数を使用。)
    /// </summary>
    public void ChangeHP(int _, int _newValue) {
        if (!isLocalPlayer && !isClient) return; // 自分のプレイヤーでなければUI更新しない
        if (UI != null) UI.ChangeHPUI(maxHP, _newValue);
        else Debug.LogWarning("UIが存在しないため、HP更新処理をスキップしました。");
    }
    #region 禁断の死亡処理(グロ注意)
    ///--------------------変更:タハラ---------------------

    /* なんかサーバーで処理できるようになったのでコマンド経由しなくていいです。
     * 読み解くにはこれを呼んでください
     * ①サーバーで被ダメージ処理。
     * ②HPが0以下ならTargetRPCで対象にのみ死亡通知。
     * ③TargetRPC内で死亡演出(ローカル)とCommand属性のリスポーン要求。
     * ④Commandからサーバーにリスポーンを要求。
     * ⑤Invokeはサーバーでのみ処理されるのでリスポーンとHPのリセットを一定時間後に処理。
     * ⑥リスポーンもTargetRPCで対象にのみ処理、SyncVarはサーバーでの変更のみ同期されるのでリスポーンとは別に関数を設けています。
     * --------------------------------------------------------------------------------------------------------------------------
     * ※大前提として死亡判定をプレイヤーが持っている設計自体Mirror的にはアウトらしいです。
     * ※今の諸々の死亡判定、演出、リスポーンを全て嚙合わせるためにとっても回りくどいことをしています。多分もう何も変えない方がいいゾ！
     */

    /// <summary>
    /// 死亡時処理
    /// サーバーで処理
    /// </summary>
    /// <summary>
    /// 死亡時処理
    /// 対象にのみ通知
    /// </summary>
    [Server]
    public void Dead(string _name) {
        if (isDead) return;
        //isLocalPlayerはサーバー処理に不必要らしいので消しました byタハラ
        //死亡フラグをたててHPを0にしておく
        isDead = true;
        ChatManager.instance.CmdSendSystemMessage(_name + " is Dead!!");
        //死亡トリガーを発火
        isDeadTrigger = true;
        //バフ全解除
        RemoveBuff();
        //ホコを所持していたらドロップ
        if (RuleManager.Instance.currentRule == GameRuleType.Hoko)
            DropHoko();
        //不具合防止のためフラグをいろいろ下ろす。
        isAttackPressed = false;
        isCanInteruct = false;
        isCanPickup = false;
        isCanSkill = false;
        IsJumpPressed = false;
        isMoving = false;
        //ローカルで死亡演出
        LocalDeadEffect();
        RespawnDelay();
        //アニメーションは全員に反映
        RpcDeadAnimation();
        // スコア計算にここから行きます
        var combat = GetComponent<PlayerCombat>();
        if (combat != null) {
            int victimTeam = TeamID;
            NetworkIdentity killerIdentity = null;

            if (!string.IsNullOrEmpty(_name) && _name != PlayerName) {
                foreach (var p in FindObjectsOfType<CharacterBase>()) {
                    if (p.PlayerName == _name) {
                        killerIdentity = p.GetComponent<NetworkIdentity>();
                        break;
                    }
                }
            }

            // OnKill を呼ぶときに victimTeam を渡すように変更
            combat.OnKill(killerIdentity, victimTeam);
        }
        // 死亡回数を増やす
        PlayerListManager.Instance?.AddDeath(this.PlayerName);

    }

    /// <summary>
    /// サーバーにリスポーンしたい意思を伝える
    /// </summary>
    [Server]
    private void RespawnDelay() {
        RpcPlayDeathEffect();
        //サーバーに通知する
        TargetRespawnDelay();
    }
    /// <summary>
    /// サーバーにホコをドロップしたいことを通知
    /// 死んだらホコを取得するようにします
    /// </summary>
    [Server]
    private void DropHoko() {
        var stageManager = StageManager.Instance;
        if (stageManager == null || stageManager.currentHoko == null) {
            Debug.LogWarning("StageManager か Hoko が存在しません");
            return;
        }

        CaptureHoko hoko = stageManager.currentHoko;

        if (hoko.holder != null && hoko.holder.gameObject == gameObject) {
            hoko.Drop();
        }
    }

    /// <summary>
    /// HPリセット関数
    /// </summary>
    [Server]
    private void TargetRespawnDelay() {
        //リスポーン要求
        Invoke(nameof(Respawn), PlayerConst.RESPAWN_TIME);
        Invoke(nameof(ResetHealth), PlayerConst.RESPAWN_TIME + 0.01f);
    }
    /// <summary>
    /// ローカル上で死亡演出
    /// 可読性向上のためまとめました
    /// </summary>
    /// <param name="_name"></param>
    [TargetRpc]
    private void LocalDeadEffect() {
        //カメラを暗くする
        gameObject.GetComponentInChildren<PlayerCamera>().EnterDeathView();
        //フェードアウトさせる
        FadeManager.Instance.StartFadeOut(2.5f);
    }
    /// <summary>
    /// NetworkAnimatorを使用した結果
    /// ローカルでの変更によってアニメーション変更がかかるため制作
    /// </summary>
    [ClientRpc]
    private void RpcDeadAnimation() {
        anim.SetTrigger("Dead");
    }

    [Server]
    private void ResetHealth() {
        //ここで体力と死亡状態を戻す
        HP = maxHP;
        isDead = false;
    }

    /// <summary>
    /// リスポーン関数
    /// 死亡した対象にのみ通知
    /// </summary>
    [TargetRpc]
    virtual public void Respawn() {
        //死んでいなかったら即抜け
        if (!isDead) return;

        ChatManager.instance.CmdSendSystemMessage("isDead : " + isDead);
        //保険で明示的に処理
        ChangeHP(maxHP, HP);
        //リスポーン地点に移動させる
        if (GameManager.Instance.IsGameRunning()) {
            int currentTeamID = TeamID;
            TeamID = -1;
            NetworkTransformHybrid NTH = GetComponent<NetworkTransformHybrid>();
            var RespownPos = GameObject.FindGameObjectsWithTag("NormalRespawnPoint");
            NTH.CmdTeleport(RespownPos[Random.Range(0, RespownPos.Length)].transform.position, Quaternion.identity);

            TeamID = currentTeamID;
        }

        //リスポーン後の無敵時間にする
        isInvincible = true;
        //経過時間をリセット
        respownAfterTime = 0;

        LoaclRespawnEffect();
    }

    /// <summary>
    /// ローカル上での演出
    /// 可読性向上のためまとめました
    /// </summary>
    private void LoaclRespawnEffect() {
        //カメラを明るくする
        gameObject.GetComponentInChildren<PlayerCamera>().ExitDeathView();
        //フェードインさせる
        FadeManager.Instance.StartFadeIn(1.0f);
    }

    ///--------------------------ここまで----------------------------------

    //ここから古谷が追加
    //エフェクト表示のための関数

    /// <summary>
    /// クライアントエフェクト表示
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="effectType"></param>
    [ClientRpc(includeOwner = true)]
    void RpcPlayDeathEffect() {

        GameObject prefab = EffectPoolRegistry.Instance.GetDeathEffect(default);
        if (prefab != null) {
            var fx = EffectPool.Instance.GetFromPool(prefab, transform.position, Quaternion.identity);
            fx.SetActive(true);
            EffectPool.Instance.ReturnToPool(fx, 1.5f);
        }
    }

    #endregion
    /// <summary>
    /// トリガー変数のリセット
    /// </summary>
    protected void ResetTrigger() {
        isAttackTrigger = false;
        isDeadTrigger = false;
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

    /// <summary>
    /// チーム参加処理(TeamIDを更新)
    /// </summary>
    [Command]
    public void CmdJoinTeam(NetworkIdentity _player, TeamColor _color) {
        GeneralCharacter player = _player.GetComponent<GeneralCharacter>();
        int currentTeam = player.TeamID;
        int newTeam = (int)_color;

        //加入しようとしてるチームが埋まっていたら
        if (ServerManager.instance.teams[newTeam].teamPlayerList.Count >= TEAMMATE_MAX) {
            ChatManager.instance.CmdSendSystemMessage("team member is over");
            return;
        }
        //既に同じチームに入っていたら
        if (newTeam == currentTeam) {
            ChatManager.instance.CmdSendSystemMessage("you join same team now");
            return;
        }
        //新たなチームに加入する時
        //今加入しているチームから抜けてIDをリセット
        if (player.TeamID != -1) {
            ServerManager.instance.teams[player.TeamID].teamPlayerList.Remove(_player);
            player.TeamID = -1;
        }

        //新しいチームに加入
        ServerManager.instance.teams[newTeam].teamPlayerList.Add(_player);
        player.TeamID = newTeam;
        //ログを表示
        ChatManager.instance.CmdSendSystemMessage(_player.GetComponent<GeneralCharacter>().PlayerName + " is joined " + newTeam + " team ");
    }

    /// <summary>
    /// アニメーターのレイヤー切り替え
    /// </summary>
    /// <param name="_layerIndex"></param>
    [Server]
    public void ChangeLayerWeight(int _layerIndex) {
        //ベースのレイヤーを飛ばし、引数と一致したレイヤーを使うようにする
        for(int i = 1, max = anim.layerCount; i < max; i++) {
            anim.SetLayerWeight(i, i == _layerIndex ? 1.0f : 0.0f);
        }
    }

    #endregion

    #region 入力受付・入力実行・判定関数

    /// <summary>
    /// 入力の共通ハンドラ
    /// </summary>
    private void OnInputStarted(string actionName, InputAction.CallbackContext ctx) {
        switch (actionName) {
            case "Jump":
                OnJump(ctx);
                break;
            case "Fire_Main":
                HandleAttack(ctx, actionName == "Attack_Main"
                    ? CharacterEnum.AttackType.Main
                    : CharacterEnum.AttackType.Sub);
                break;
            case "Fire_Sub":
                HandleAttack(ctx, actionName == "Attack_Sub"
                    ? CharacterEnum.AttackType.Main
                    : CharacterEnum.AttackType.Sub);
                break;
            case "SubWeapon":
                weaponController_sub.TryUseSubWeapon();
                break;
            case "ShowHostUI":
                OnShowHostUI(ctx);
                break;
            case "CameraMenu":
                OnShowCameraMenu(ctx);
                break;
            case "Ready":
                OnReadyPlayer(ctx);
                break;
            case "SendMessage":
                OnSendMessage(ctx);
                break;
            case "SendStamp":
                OnSendStamp(ctx);
                break;
        }
    }
    private void OnInputPerformed(string actionName, InputAction.CallbackContext ctx) {
        switch (actionName) {
            case "Move":
                OnMove(ctx);
                break;
            case "Jump":
                OnJump(ctx);
                break;
            case "Fire_Main":
                HandleAttack(ctx, actionName == "Attack_Main"
                    ? CharacterEnum.AttackType.Main
                    : CharacterEnum.AttackType.Sub);
                break;
            case "Fire_Sub":
                HandleAttack(ctx, actionName == "Attack_Sub"
                    ? CharacterEnum.AttackType.Main
                    : CharacterEnum.AttackType.Sub);
                break;
            case "Skill":
                OnUseSkill(ctx);
                break;
            case "Interact":
                OnInteract(ctx);
                break;
            case "Reload":
                OnReload(ctx);
                break;
        }
    }
    private void OnInputCanceled(string actionName, InputAction.CallbackContext ctx) {
        switch (actionName) {
            case "Move":
                MoveInput = Vector2.zero;
                CmdResetAnimation();
                break;
            case "Fire_Main":
            case "Fire_Sub":
                HandleAttack(ctx, actionName == "Attack_Main"
                    ? CharacterEnum.AttackType.Main
                    : CharacterEnum.AttackType.Sub);
                break;
        }
    }

    /// <summary>
    /// 当たり判定の中に入った瞬間に発動
    /// </summary>
    protected void OnTriggerEnter(Collider _collider) {
        //早期return
        if (!isLocalPlayer) return;

        //switchで分岐。ここに順次追加していく。
        switch (_collider.tag) {
            case "Item":
                // フラグを立てる
                isCanPickup = true;
                useCollider = _collider;

                break;
            case "SelectCharacterObject":
                // フラグを立てる
                isCanInteruct = true;
                useCollider = _collider;
                useTag = "SelectCharacterObject";
                break;
            case "Gacha":
                isCanInteruct = true;
                useCollider = _collider;
                useTag = "Gacha";
                break;
            case "RedTeam":
                CmdJoinTeam(netIdentity, TeamColor.Red);
                break;
            case "BlueTeam":
                CmdJoinTeam(netIdentity, TeamColor.Blue);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 当たり判定から抜けた瞬間に発動
    /// </summary>
    protected void OnTriggerExit(Collider _collider) {
        //早期return
        if (!isLocalPlayer) return;

        //switchで分岐。ここに順次追加していく。
        switch (_collider.tag) {
            case "Item":
                // フラグを下ろす
                isCanPickup = false;
                useCollider = null;
                break;
            case "SelectCharacterObject":
                // フラグを下ろす
                isCanInteruct = false;
                useCollider = null;
                useTag = null;
                break;
            case "Gacha":
                isCanInteruct = false;
                useCollider = null;
                useTag = null;
                break;
            case "RedTeam":
                //抜けたときは処理しない。何か処理があったら追加。
                CmdJoinTeam(netIdentity, TeamColor.Red);
                break;
            case "BlueTeam":
                //抜けたときは処理しない。何か処理があったら追加。
                CmdJoinTeam(netIdentity, TeamColor.Blue);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// アイテム取得関連のフラグをリセットする
    /// </summary>
    public void ResetCanPickFlag() {
        // フラグを下ろす
        isCanPickup = false;
        useCollider = null;
    }

    /// <summary>
    /// 移動
    /// </summary>
    public void OnMove(InputAction.CallbackContext context) {
        MoveInput = context.ReadValue<Vector2>();
        float moveX = MoveInput.x;
        float moveZ = MoveInput.y;
        //アニメーション管理
        ControllMoveAnimation(moveX, moveZ);

    }
    /// <summary>
    /// 視点(現在未使用)
    /// </summary>
    public void OnLook(InputAction.CallbackContext context) {
        lookInput = context.ReadValue<Vector2>();
    }
    /// <summary>
    /// ジャンプ
    /// </summary>
    public void OnJump(InputAction.CallbackContext context) {
        // ボタンが押された瞬間だけ反応させる
        if (context.performed && IsGrounded) {
            IsJumpPressed = true;
            bool isJumping = !IsGrounded;
            anim.SetBool("Jump", isJumping);
        }
    }
    /// <summary>
    /// メイン攻撃(現在未使用)
    /// </summary
    public void OnAttack_Main(InputAction.CallbackContext context) {
        HandleAttack(context, CharacterEnum.AttackType.Main);
    }
    /// <summary>
    /// サブ攻撃(現在未使用)
    /// </summary
    public void OnAttack_Sub(InputAction.CallbackContext context) {
        HandleAttack(context, CharacterEnum.AttackType.Sub);
    }
    /// <summary>
    /// スキル
    /// </summary
    public void OnUseSkill(InputAction.CallbackContext context) {
        if (context.performed)
            StartUseSkill();
    }
    /// <summary>
    /// インタラクト
    /// </summary>
    public void OnInteract(InputAction.CallbackContext context) {
        if (context.performed) Interact();
    }
    /// <summary>
    /// リロード
    /// </summary>
    public void OnReload(InputAction.CallbackContext context) {
        if (context.performed && weaponController_main.ammo < weaponController_main.weaponData.maxAmmo) {
            weaponController_main.CmdReloadRequest();
        }
    }
    /// <summary>
    /// 追加:タハラ
    /// UI表示
    /// </summary>
    public void OnShowHostUI(InputAction.CallbackContext context) {
        if (!isServer || !isLocalPlayer || SceneManager.GetActiveScene().name == "GameScene") return;
        if (context.started) {
            if (CameraMenu.isOpen)
                CameraMenu.ToggleMenu();
            HostUI.ShowOrHideUI();
        }
    }

    public void OnShowCameraMenu(InputAction.CallbackContext context) {
        if (!isLocalPlayer )
            return;
        if (context.started) {
            if (HostUI.isVisibleUI) {
                HostUI.ShowOrHideUI();
            }

            CameraMenu.ToggleMenu();
        }
    }
    /// <summary>
    /// 追加:タハラ
    /// プレイヤーの準備状態切り替え
    /// </summary>
    /// <param name="context"></param>
    public void OnReadyPlayer(InputAction.CallbackContext context) {
        if (!isLocalPlayer || SceneManager.GetActiveScene().name == "GameScene")
            return;
        //内部の準備状態を更新
        if (context.started) {
            if (!isServer)
                CmdChangePlayerReady();
            else {
                ready = !ready;
                ChatManager.instance.CmdSendSystemMessage(PlayerName + " ready :  " + ready);
            }
        }
    }

    /// <summary>
    /// 追加:タハラ
    /// チャット送信
    /// </summary>
    /// <param name="context"></param>
    public void OnSendMessage(InputAction.CallbackContext context) {
        if (!isLocalPlayer)
            return;
        //チャット送信
        var key = context.control.name;
        string sendMessage;
        switch (key) {
            case "upArrow":
                sendMessage = "?";
                break;
            case "leftArrow":
                sendMessage = "ggEZ";
                break;
            case "rightArrow":
                sendMessage = "WTF";
                break;
            default:
                sendMessage = "4649";
                break;
        }
        ChatManager.instance.CmdSendSystemMessage(PlayerName + ":" + sendMessage);
    }

    /// <summary>
    /// 追加:タハラ
    /// スタンプ送信
    /// </summary>
    /// <param name="context"></param>
    public void OnSendStamp(InputAction.CallbackContext context) {
        if (!isLocalPlayer)
            return;
        //チャット送信
        if (context.started) {
            int stampIndex = Random.Range(0, 4);
            ChatManager.instance.CmdSendStamp(stampIndex, PlayerName);
        }
    }

    /// <summary>
    /// 移動関数(死亡中は呼ばないでください。)
    /// </summary>
    protected void MoveControl() {
        //移動入力が行われている間は移動中フラグを立てる
        if (MoveInput != Vector2.zero) isMoving = true;
        else isMoving = false;


        //カメラの向きを取得
        Transform cameraTransform = Camera.main.transform;
        //進行方向のベクトルを取得
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();
        //右方向のベクトルを取得
        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();
        //2つのベクトルを合成
        moveDirection = forward * MoveInput.y + right * MoveInput.x;

        // カメラの向いている方向をプレイヤーの正面に
        Vector3 aimForward = forward; // 水平面だけを考慮
        if (aimForward != Vector3.zero) {
            Quaternion targetRot = Quaternion.LookRotation(aimForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, PlayerConst.TURN_SPEED * Time.deltaTime);
        }

        // 空中か地上で挙動を分ける
        Vector3 velocity = rigidbody.velocity;
        Vector3 targetVelocity = new(moveDirection.x * moveSpeed, velocity.y, moveDirection.z * moveSpeed);

        //地面に立っていたら通常通り
        if (IsGrounded) {
            rigidbody.velocity = targetVelocity;
        }
        else {
            // 空中では地上速度に向けてゆるやかに補間（慣性を残す）
            rigidbody.velocity = Vector3.Lerp(velocity, targetVelocity, Time.deltaTime * 2f);
        }
    }

    /// <summary>
    /// 移動アニメーションの管理
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_z"></param>
    [Command]
    private void ControllMoveAnimation(float _x, float _z) {
        ResetRunAnimation();
        //斜め入力の場合
        if (_x != 0 && _z != 0) {
            anim.SetBool("RunL", false);
            anim.SetBool("RunR", false);
            if (_z > 0) {
                currentAnimation = "RunF";
            }
            if (_z < 0) {
                currentAnimation = "RunB";
            }
            anim.SetBool(currentAnimation, true);
            return;

        }

        if (_x > 0 && _z == 0) {
            currentAnimation = "RunR";
        }
        if (_x < 0 && _z == 0) {
            currentAnimation = "RunL";
        }
        if (_x == 0 && _z > 0) {
            currentAnimation = "RunF";
        }
        if (_x == 0 && _z < 0) {
            currentAnimation = "RunB";
        }
        anim.SetBool(currentAnimation, true);
    }

    /// <summary>
    /// 移動アニメーションのリセット
    /// </summary>
    private void ResetRunAnimation() {
        anim.SetBool("RunF", false);
        anim.SetBool("RunR", false);
        anim.SetBool("RunL", false);
        anim.SetBool("RunB", false);

        currentAnimation = null;
    }

    [Command]
    private void CmdResetAnimation() {
        ResetRunAnimation();
    }

    /// <summary>
    /// ジャンプ管理関数(死亡中は呼ばないでください。)
    /// </summary>
    protected void JumpControl() {
        // ジャンプ判定
        if (IsJumpPressed && IsGrounded) {
            // 現在の速度をリセットしてから上方向に力を加える
            Vector3 velocity = rigidbody.velocity;
            velocity.y = 0f;
            rigidbody.velocity = velocity;

            rigidbody.AddForce(Vector3.up * PlayerConst.JUMP_FORCE, ForceMode.Impulse);
            IsJumpPressed = false; // 連打防止
        }

        //ベクトルが上方向に働いている時
        if (rigidbody.velocity.y > 0) {
            //追加の重力補正を掛ける
            rigidbody.velocity += (PlayerConst.JUMP_UPFORCE - 1) * Physics.gravity.y * Time.deltaTime * Vector3.up;
        }
        // ベクトルが下方向に働いている時
        else if (rigidbody.velocity.y < 0) {
            //追加の重力補正を掛ける
            rigidbody.velocity += (PlayerConst.JUMP_DOWNFORCE - 1) * Physics.gravity.y * Time.deltaTime * Vector3.up;
            anim.SetBool("Jump", false);
        }


        // 地面判定（下方向SphereCastでもOK。そこまで深く考えなくていいかも。）
        IsGrounded = Physics.CheckSphere(GroundCheck.position, PlayerConst.GROUND_DISTANCE, GroundLayer);
    }

    /// <summary>
    /// リスポーン管理関数(死亡中も呼んでください。)
    /// </summary>
    virtual protected void RespawnControl() {
        //死亡した瞬間の処理
        if (isDeadTrigger) {
            Invoke(nameof(Respawn), PlayerConst.RESPAWN_TIME);
        }
        //復活後であるときの処理
        if (isInvincible) {
            //復活してからの時間を加算
            respownAfterTime += Time.deltaTime;
            //規定時間経過後無敵状態を解除
            if (respownAfterTime >= PlayerConst.RESPAWN_INVINCIBLE_TIME) {
                isInvincible = false;
            }
        }
    }

    /// <summary>
    /// Abstruct : スキルとパッシブの制御用関数(死亡中は呼ばないでください。)
    /// </summary>
    abstract protected void AbilityControl();

    /// <summary>
    /// 攻撃入力のハンドル分岐
    /// </summary>
    private void HandleAttack(InputAction.CallbackContext context, CharacterEnum.AttackType _type) {
        //死亡していたら攻撃できない
        if (isDead || !isLocalPlayer) return;

        //入力タイプで分岐
        switch (context.phase) {
            //押した瞬間から
            case InputActionPhase.Started:
                isAttackPressed = true;
                break;
            //離した瞬間まで
            case InputActionPhase.Canceled:
                isAttackPressed = false;
                //アニメーション終了
                anim.SetBool("Shoot", false);
                break;
            //押した瞬間
            case InputActionPhase.Performed:
                isAttackTrigger = true;
                break;
        }

    }
    /// <summary>
    /// 攻撃関数
    /// </summary>
    virtual public void StartAttack() {
        if (weaponController_main == null) return;

        if (HostUI.isVisibleUI == true) return;

        // 武器が攻撃可能かチェックしてサーバー命令を送る(CmdRequestAttack武器種ごとの分岐も側で)
        Vector3 shootDir = GetShootDirection();
        weaponController_main.CmdRequestAttack(shootDir);        
    }
    /// <summary>
    /// 攻撃に使用する向いている方向を取得する関数
    /// </summary>
    protected Vector3 GetShootDirection() {
        Camera cam = Camera.main;
        Vector3 screenCenter = new(Screen.width / 2f, Screen.height / 2f, 0f);

        // カメラ中心から遠方の目標点を決める（壁は無視）
        Ray camRay = cam.ScreenPointToRay(screenCenter);
        Vector3 aimPoint = camRay.GetPoint(50f); // 50m先に仮のターゲット

        // firePoint から aimPoint 方向にレイを飛ばして壁判定
        Vector3 direction = (aimPoint - firePoint.position).normalized;
        if (Physics.Raycast(firePoint.position, direction, out RaycastHit hit, 100f)) {
            // 壁や床に当たればその位置に補正
            return (hit.point - firePoint.position).normalized;
        }

        // 当たらなければそのままaimPoint方向
        return direction;
    }
    /// <summary>
    /// スキル呼び出し関数
    /// </summary>
    abstract protected void StartUseSkill();
    /// <summary>
    /// インタラクト関数
    /// </summary>
    protected void Interact() {
        if (isCanPickup) {
            ItemBase item = useCollider.GetComponent<ItemBase>();
            item.Use(gameObject);
            return;
        }
        if (isCanInteruct) {
            if (useTag == "SelectCharacterObject") {
                CharacterSelectManager select = useCollider.GetComponentInParent<CharacterSelectManager>();
                select.StartCharacterSelect(gameObject);
                return;
            }
            if (useTag == "Gacha") {
                GachaSystem gacha = useCollider.GetComponentInParent<GachaSystem>();
                gacha.StartGachaSelect(gameObject);
                UI.gameObject.SetActive(false);
                return;
            }

        }
    }

    #endregion

    #region ～バフ・ステータス操作系～
    /// <summary>
    /// HP回復(時間経過で徐々に回復)発動 [_valueは1.0fを100％とした相対値]
    /// </summary>
    [Command]
    public void Heal(float _value, float _usingTime) {
        if (healCoroutine != null) StopCoroutine(healCoroutine);

        //  エフェクト再生
        PlayEffect(HEAL_BUFF_EFFECT);

        // 総回復量を maxHP の割合で計算（例：_value=0.2 → 20％回復）
        float totalHeal = maxHP * _value;
        //  回復実行（コルーチンで回す）
        healCoroutine = StartCoroutine(HealOverTime(totalHeal, _usingTime));
    }

    /// <summary>
    ///  時間まで徐々に回復させていく実行処理(コルーチン)
    /// </summary>
    private IEnumerator HealOverTime(float _totalHeal, float _duration) {
        float elapsed = 0f;
        float healPerSec = _totalHeal / _duration;
        float healBuffer = 0f; //   小数の回復を蓄積

        while (elapsed < _duration) {
            if (isDead) yield break; // 死亡時は即終了

            healBuffer += healPerSec * Time.deltaTime; // 累積
            if (healBuffer >= 1f) {
                int healInt = Mathf.FloorToInt(healBuffer); // 整数分だけ反映
                HP = Mathf.Min(HP + healInt, maxHP);
                healBuffer -= healInt; // 余りを保持
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        DestroyChildrenWithTag(EFFECT_TAG);
        healCoroutine = null;
    }

    /// <summary>
    /// 攻撃力上昇バフ発動 [_valueは1.0fを100％とした相対値]
    /// </summary>
    [Command]
    public void AttackBuff(float _value, float _usingTime) {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        //  エフェクト再生
        PlayEffect(ATTACK_BUFF_EFFECT);

        attackCoroutine = StartCoroutine(AttackBuffRoutine(_value, _usingTime));
    }

    /// <summary>
    ///  時間まで攻撃力を上げておく実行処理(コルーチン)
    /// </summary>
    private IEnumerator AttackBuffRoutine(float _value, float _duration) {
        attack = Mathf.RoundToInt(defaultAttack * _value);
        yield return new WaitForSeconds(_duration);
        attack = defaultAttack;
        DestroyChildrenWithTag(EFFECT_TAG);
        attackCoroutine = null;
    }

    /// <summary>
    /// 移動速度上昇バフ発動 [_valueは1.0fを100％とした相対値]
    /// </summary>
    [Command]
    public void MoveSpeedBuff(float _value, float _usingTime) {
        if (speedCoroutine != null) StopCoroutine(speedCoroutine);
        //  エフェクト再生
        PlayEffect(SPEED_BUFF_EFFECT);

        speedCoroutine = StartCoroutine(SpeedBuffRoutine(_value, _usingTime));
    }

    /// <summary>
    ///  時間まで移動速度を上げておく実行処理(コルーチン)
    /// </summary>
    private IEnumerator SpeedBuffRoutine(float _value, float _duration) {
        moveSpeed = Mathf.RoundToInt(defaultMoveSpeed * _value);
        yield return new WaitForSeconds(_duration);
        moveSpeed = defaultMoveSpeed;
        DestroyChildrenWithTag(EFFECT_TAG);
        speedCoroutine = null;
    }

    /// <summary>
    /// すべてのバフを即解除
    /// </summary>
    [Command]
    public void RemoveBuff() {
        StopAllCoroutines();
        DestroyChildrenWithTag(EFFECT_TAG);
        moveSpeed = defaultMoveSpeed;
        attack = defaultAttack;
        healCoroutine = speedCoroutine = attackCoroutine = null;
    }

    /// <summary>
    /// エフェクト再生用関数
    /// </summary>
    private void PlayEffect(int effectNum) {
        if (isServer) RpcPlayEffect(effectNum); // サーバー側なら直接全員に通知
        else CmdPlayEffect(effectNum); // クライアントならサーバーへ命令
    }

    /// <summary>
    /// 指定の親オブジェクトのタグ付き子オブジェクトを削除する
    /// </summary>
    private void DestroyChildrenWithTag(string tag) {
        if (isServer) RpcDestroyChildrenWithTag(tag); // サーバーなら全員に通知
        else CmdDestroyChildrenWithTag(tag); // クライアントならサーバーへ命令
    }

    #region Command,ClientRpcの関数
    /// <summary>
    /// エフェクト生成
    /// </summary>
    [Command]
    private void CmdPlayEffect(int effectNum) {
        RpcPlayEffect(effectNum);
    }
    [ClientRpc]
    private void RpcPlayEffect(int effectNum) {
        //  ローカルで一度子オブジェクトを参照して破棄
        DestroyChildrenWithTagLocal(EFFECT_TAG);

        //  ここで生成
        Instantiate(buffEffect.effectInfos[effectNum].effect, transform);
    }

    /// <summary>
    /// エフェクト破棄
    /// </summary>
    [Command]
    private void CmdDestroyChildrenWithTag(string tag) {
        RpcDestroyChildrenWithTag(tag);
    }
    [ClientRpc]
    private void RpcDestroyChildrenWithTag(string tag) {
        DestroyChildrenWithTagLocal(tag);
    }
    private void DestroyChildrenWithTagLocal(string tag) {
        if (tag == null) return;

        foreach (Transform child in transform) {
            if (child.CompareTag(tag)) {
                Destroy(child.gameObject);
            }
        }
    }

    #endregion

    #endregion
}