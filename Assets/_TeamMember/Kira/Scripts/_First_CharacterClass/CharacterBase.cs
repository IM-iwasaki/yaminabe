using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static TeamData;
[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(NetworkTransformHybrid))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]

/*
 @flie    First_CharacterClass

 本ファイルを改変する人へ・・・改変個所に改変者のイニシャルを記載の上、改変したところを//で囲ってください。
                               (コード自体はコメントアウトしなくていいです)

                           例：int test = 100;
                               // K.W.
                               test = 256;
                               //
*/
public abstract class CharacterBase : NetworkBehaviour {
    #region ～変数宣言～

    #region ～ステータス～
    //現在の体力
    [SyncVar(hook = nameof(ChangeHP))] public int HP;
    //最大の体力
    public int MaxHP { get; protected set; }
    //基礎攻撃力
    [SyncVar] public int Attack;
    //移動速度
    [SyncVar]public int MoveSpeed = 5;
    //持っている武器の文字列
    public string CurrentWeapon { get; protected set; }
    //所属チームの番号(-1は未所属。0、1はチーム所属。)
    [SyncVar] public int TeamID = -1;
    //プレイヤーの名前
    //TODO:プレイヤーセーブデータから取得できるようにする。
    protected string PlayerName = "Player_Test";

    //受けるダメージ倍率
    public int DamageRatio = 100;

    //ランキング用変数の仮定義
    public int Score { get; protected set; } = 0;

    #endregion

    #region ～Vector系統変数～

    //移動を要求する方向
    protected Vector2 MoveInput;
    //実際に移動する方向
    public Vector3 MoveDirection { get; private set; }
    //視点を要求する方向
    protected Vector2 LookInput { get; private set; }
    //向いている方向
    public Vector3 LookDirection { get; private set; }

    //リスポーン地点
    public Vector3 RespownPosition { get; protected set; }

    //射撃位置
    public Transform firePoint;

    #endregion

    #region ～状態管理・コンポーネント変数～

    //死亡しているか
    protected bool IsDead { get; private set; } = false;
    //死亡してからの経過時間
    protected float DeadAfterTime { get; private set; } = 0.0f;
    //復活後の無敵時間中であるか
    protected bool IsInvincible { get; private set; } = false;
    //復活してからの経過時間
    protected float RespownAfterTime { get; private set; } = 0.0f;

    //移動中か
    public bool IsMoving { get; private set; } = false;
    //攻撃中か
    public bool IsAttackPressed { get; private set; } = false;
    //攻撃開始時間
    public float AttackStartTime { get; private set; } = 0;
    //オート攻撃タイプ (デフォルトはフルオート)
    public PlayerConst.AutoFireType AutoFireType { get; protected set; } = PlayerConst.AutoFireType.FullAutomatic;
    // 連射間隔
    public float FireInterval { get; private set; } = 0.2f;

    //アイテムを拾える状態か
    protected bool IsCanPickup { get; private set; } = false;
    //インタラクトできる状態か
    protected bool IsCanInteruct { get; private set; } = false;

    //スキルを使用できるか
    public bool IsCanSkill { get; protected set; } = false;
    //スキル使用後経過時間
    public float SkillAfterTime { get; protected set; } = 0.0f;

    //コンポーネント情報
    protected new Rigidbody rigidbody;
    protected Collider useCollider;
    [SerializeField] protected PlayerUIController UI;
    [SerializeField] private InputActionAsset inputActions;

    #endregion

    #region ～アクション用変数～

    //武器を使用するため
    [SerializeField] protected MainWeaponController weaponController;

    //ジャンプ入力をしたか
    private bool IsJumpPressed = false;
    //GroundLayer
    private LayerMask GroundLayer;
    //足元の確認用Transform
    [SerializeField] private Transform GroundCheck;
    //接地しているか
    [SerializeField] private bool IsGrounded;

    //スタン、怯み(硬直する,カメラ以外操作無効化)

    #endregion

    #region ～バフ管理用変数～
    private Coroutine healCoroutine;
    private Coroutine speedCoroutine;
    private Coroutine attackCoroutine;
    private int defaultMoveSpeed;
    private int defaultAttack;
    #endregion

    #endregion

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

        // デフォルト値保存
        defaultMoveSpeed = MoveSpeed;
        defaultAttack = Attack;
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

    }

    /// <summary>
    /// ステータスのインポート
    /// </summary>
    public abstract void StatusInport(CharacterStatus _inport = null);

    /// <summary>
    /// StatusInportでnullが発生した時にデフォルトの値で初期化する
    /// </summary>
    protected void DefaultStatusInport() {
        Debug.LogWarning("InputStatusに値が入っていなかったため、デフォルト値で初期化を行いました。");
        MaxHP = PlayerConst.DEFAULT_MAXHP;
        HP = MaxHP;
        Attack = PlayerConst.DEFAULT_ATTACK;
        MoveSpeed = PlayerConst.DEFAULT_MOVESPEED;
    }

    #endregion

    #region ～プレイヤー状態更新関数～

    /// <summary>
    /// 被弾・死亡判定関数
    /// </summary>
    [Server]public void TakeDamage(int _damage) {
        //既に死亡状態なら帰る
        if (IsDead) return;

        //ダメージ倍率を適用
        _damage *= DamageRatio / 100;
        //ダメージが0以下だったら1に補正する
        if (_damage <= 0) _damage = 1;
        //HPの減算処理
        HP -= _damage;
        //HPが0以下になったとき死亡していなかったら死亡処理を行う
        if (HP <= 0 && !IsDead) Dead();
    }

    /// <summary>
    /// UI用のHP更新関数(第一引数は消せないため無名変数を使用。)
    /// </summary>
    public void ChangeHP(int _, int newValue) {
        if (!isLocalPlayer) return; // 自分のプレイヤーでなければUI更新しない
        if (UI != null) UI.ChangeHPUI(MaxHP, newValue);
        else Debug.LogWarning("UIが存在しないため、HP更新処理をスキップしました。");
    }

    /// <summary>
    /// 死亡時処理
    /// </summary>
    public void Dead() {
        //死亡フラグをたててHPを0にしておく
        IsDead = true;
        HP = 0;
        //バフ全解除
        RemoveBuff();       

        //不具合防止のためフラグをいろいろ下ろす。
        IsAttackPressed = false;
        IsCanInteruct = false;
        IsCanPickup = false;
        IsCanSkill = false;
        IsJumpPressed = false;
        IsMoving = false;
    }

    /// <summary>
    /// リスポーン関数
    /// </summary>
    [Command]public void Respawn() {
        //死んでいなかったら即抜け
        if (!IsDead) return;

        //復活させてHPを全回復
        IsDead = false;
        HP = MaxHP;

        //リスポーン地点に移動させる
        var RespownPos = StageManager.Instance.GetTeamSpawnPoints((teamColor)TeamID);
        transform.position = RespownPos[TeamID].transform.position;

        //リスポーン後の無敵時間にする
        IsInvincible = true;

        //経過時間をリセット
        DeadAfterTime = 0;
        RespownAfterTime = 0;
    }

    /// <summary>
    /// チーム参加処理(TeamIDを更新)
    /// </summary>
    [Command]public void CmdJoinTeam(NetworkIdentity _player, teamColor _color) {
        CharacterBase player = _player.GetComponent<CharacterBase>();
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
        ServerManager.instance.teams[_player.GetComponent<CharacterBase>().TeamID].teamPlayerList.Remove(_player);
        player.TeamID = -1;
        //新しいチームに加入
        ServerManager.instance.teams[newTeam].teamPlayerList.Add(_player);
        player.TeamID = newTeam;
        //ログを表示
        ChatManager.instance.CmdSendSystemMessage(_player.ToString() + "is joined" + newTeam + "team");
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
                    ? PlayerConst.AttackType.Main
                    : PlayerConst.AttackType.Sub);
                break;
            case "Fire_Sub":
                HandleAttack(ctx, actionName == "Attack_Sub"
                    ? PlayerConst.AttackType.Main
                    : PlayerConst.AttackType.Sub);
                break;
            case "ShowHostUI":
                OnShowHostUI(ctx);
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
                    ? PlayerConst.AttackType.Main
                    : PlayerConst.AttackType.Sub);
                break;
            case "Fire_Sub":
                HandleAttack(ctx, actionName == "Attack_Sub"
                    ? PlayerConst.AttackType.Main
                    : PlayerConst.AttackType.Sub);
                break;
            case "Skill":
                OnUseSkill(ctx);
                break;
            case "Interact":
                OnInteract(ctx);
                break;
        }
    }
    private void OnInputCanceled(string actionName, InputAction.CallbackContext ctx) {
        switch (actionName) {
            case "Move":
                MoveInput = Vector2.zero;
                break;
            case "Fire_Main":
            case "Fire_Sub":
                HandleAttack(ctx, actionName == "Attack_Main"
                    ? PlayerConst.AttackType.Main
                    : PlayerConst.AttackType.Sub);
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
                IsCanPickup = true;
                useCollider = _collider;

                break;
            case "SelectCharacterObject":
                // フラグを立てる
                IsCanInteruct = true;
                useCollider = _collider;
                break;
            case "RedTeam":
                CmdJoinTeam(netIdentity, teamColor.Red);
                break;
            case "BlueTeam":
                CmdJoinTeam(netIdentity, teamColor.Blue);
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
                IsCanPickup = false;
                useCollider = null;
                break;
            case "SelectCharacterObject":
                // フラグを下ろす
                IsCanInteruct = false;
                useCollider = null;
                break;
            case "RedTeam":
                //抜けたときは処理しない。何か処理があったら追加。
                break;
            case "BlueTeam":
                //抜けたときは処理しない。何か処理があったら追加。
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 移動
    /// </summary>
    public void OnMove(InputAction.CallbackContext context) {
        MoveInput = context.ReadValue<Vector2>();
    }
    /// <summary>
    /// 視点(現在未使用)
    /// </summary>
    public void OnLook(InputAction.CallbackContext context) {
        LookInput = context.ReadValue<Vector2>();
    }
    /// <summary>
    /// ジャンプ
    /// </summary>
    public void OnJump(InputAction.CallbackContext context) {
        // ボタンが押された瞬間だけ反応させる
        if (context.performed && IsGrounded) {
            IsJumpPressed = true;
        }
    }
    /// <summary>
    /// メイン攻撃(現在未使用)
    /// </summary
    public void OnAttack_Main(InputAction.CallbackContext context) {
        HandleAttack(context, PlayerConst.AttackType.Main);
    }
    /// <summary>
    /// サブ攻撃(現在未使用)
    /// </summary
    public void OnAttack_Sub(InputAction.CallbackContext context) {
        HandleAttack(context, PlayerConst.AttackType.Sub);
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

    public void OnShowHostUI(InputAction.CallbackContext context) {
        if (!isServer || !isLocalPlayer || SceneManager.GetActiveScene().name == "GameScene") return;
        if (context.started) HostUI.instance.isVisibleUI = !HostUI.instance.isVisibleUI;
    }

    /// <summary>
    /// 移動関数
    /// </summary>
    protected void MoveControl() {
        //移動入力が行われている間は移動中フラグを立てる
        if (MoveInput != Vector2.zero)  IsMoving = true;
        else IsMoving = false;


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
        MoveDirection = forward * MoveInput.y + right * MoveInput.x;

        // カメラの向いている方向をプレイヤーの正面に
        Vector3 aimForward = forward; // 水平面だけを考慮
        if (aimForward != Vector3.zero) {
            Quaternion targetRot = Quaternion.LookRotation(aimForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, PlayerConst.TURN_SPEED * Time.deltaTime);
        }

        // 空中か地上で挙動を分ける
        Vector3 velocity = rigidbody.velocity;
        Vector3 targetVelocity = new(MoveDirection.x * MoveSpeed, velocity.y, MoveDirection.z * MoveSpeed);

        //地面に立っていたら通常通り
        if (IsGrounded) {
            rigidbody.velocity = targetVelocity;
        } else {
            // 空中では地上速度に向けてゆるやかに補間（慣性を残す）
            rigidbody.velocity = Vector3.Lerp(velocity, targetVelocity, Time.deltaTime * 2f);
        }
    }

    /// <summary>
    /// ジャンプ管理関数
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
        }


        // 地面判定（下方向SphereCastでもOK。そこまで深く考えなくていいかも。）
        IsGrounded = Physics.CheckSphere(GroundCheck.position, PlayerConst.GROUND_DISTANCE, GroundLayer);
    }

    /// <summary>
    /// リスポーン管理関数
    /// </summary>
    virtual protected void RespawnControl() {
        //死亡中であるときの処理
        if (IsDead) {
            //死亡してからの時間を加算
            DeadAfterTime += Time.deltaTime;
            //死亡後経過時間がリスポーンに必要な時間を過ぎたら
            if (DeadAfterTime >= PlayerConst.RespownTime)  Respawn();
        }
        //復活後であるときの処理
        if (IsInvincible) {
            //復活してからの時間を加算
            RespownAfterTime += Time.deltaTime;
            //規定時間経過後無敵状態を解除
            if (RespownAfterTime >= PlayerConst.RespownInvincibleTime) {
                IsInvincible = false;
            }
        }
    }

    virtual protected void AbilityControl() {}

    /// <summary>
    /// 攻撃入力のハンドル分岐
    /// </summary>
    private void HandleAttack(InputAction.CallbackContext context, PlayerConst.AttackType _type) {
        switch (context.phase) {
            //押した瞬間
            case InputActionPhase.Started:
                IsAttackPressed = true;
                //入力開始時間を記録
                AttackStartTime = Time.time;           

                //フルオート状態の場合コルーチンで射撃間隔を調整する
                if (AutoFireType == PlayerConst.AutoFireType.FullAutomatic){
                    Debug.Log("フルオート攻撃を開始しました。");

                    StartCoroutine(AutoFire(_type)); 
                }
                break;
            //離した瞬間
            case InputActionPhase.Canceled:
                IsAttackPressed = false;
                //入力終了時間を記録
                float heldTime = Time.time - AttackStartTime;
            

                //セミオート状態の場合入力時間が短ければ一回攻撃
                if (AutoFireType == PlayerConst.AutoFireType.SemiAutomatic && heldTime < 0.3f){
                    Debug.Log("セミオート攻撃です。");

                    StartAttack(_type);
                }
                break;
        }
    }

    /// <summary>
    /// オート攻撃のコルーチン
    /// </summary>
    private IEnumerator AutoFire(PlayerConst.AttackType _type) {
        while (IsAttackPressed) {
            Debug.Log("オート攻撃中...");
            StartAttack(_type);
            yield return new WaitForSeconds(FireInterval);
        }
    }

    /// <summary>
    /// 攻撃関数
    /// </summary>
    virtual protected void StartAttack(PlayerConst.AttackType _type = PlayerConst.AttackType.Main){
        if (weaponController == null) return;

        // 武器が攻撃可能かチェックしてサーバー命令を送る
        Vector3 shootDir = GetShootDirection();
        weaponController.CmdRequestAttack(shootDir);
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
        if (IsCanPickup) {
            ItemBase item = useCollider.GetComponent<ItemBase>();
            item.Use(gameObject);
            return;
        }
        if (IsCanInteruct) {
            CharacterSelectManager select = useCollider.GetComponentInParent<CharacterSelectManager>();
            select.StartCharacterSelect(gameObject);
            return;
        }
    }

    #endregion

    #region ～バフ・ステータス操作系～
    /// <summary>
    /// HP回復(時間経過で徐々に回復)発動 [_valueは1.0fを100％とした相対値]
    /// </summary>
    [Command]public void Heal(float _value, float _usingTime) {
        if (healCoroutine != null) StopCoroutine(healCoroutine);

        // 総回復量を MaxHP の割合で計算（例：_value=0.2 → 20％回復）
        float totalHeal = MaxHP * _value;
        //  回復実行（コルーチンで回す）
        healCoroutine = StartCoroutine(HealOverTime(totalHeal, _usingTime));
    }

    /// <summary>
    ///  時間まで徐々に回復させていく実行処理(コルーチン)
    /// </summary>
    private IEnumerator HealOverTime(float totalHeal, float duration) {
        float elapsed = 0f;
        float healPerSec = totalHeal / duration;
        float healBuffer = 0f; //   小数の回復を蓄積

        while (elapsed < duration) {
            if (IsDead) yield break; // 死亡時は即終了

            healBuffer += healPerSec * Time.deltaTime; // 累積
            if (healBuffer >= 1f) {
                int healInt = Mathf.FloorToInt(healBuffer); // 整数分だけ反映
                HP = Mathf.Min(HP + healInt, MaxHP);
                healBuffer -= healInt; // 余りを保持
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        healCoroutine = null;
    }

    /// <summary>
    /// 攻撃力上昇バフ発動 [_valueは1.0fを100％とした相対値]
    /// </summary>
    [Command]
    public void AttackBuff(float _value, float _usingTime) {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackBuffRoutine(_value, _usingTime));
    }

    /// <summary>
    ///  時間まで攻撃力を上げておく実行処理(コルーチン)
    /// </summary>
    private IEnumerator AttackBuffRoutine(float value, float duration) {
        Attack = Mathf.RoundToInt(defaultAttack * value);
        yield return new WaitForSeconds(duration);
        Attack = defaultAttack;
        attackCoroutine = null;
    }

    /// <summary>
    /// 移動速度上昇バフ発動 [_valueは1.0fを100％とした相対値]
    /// </summary>
    [Command]
    public void MoveSpeedBuff(float _value, float _usingTime) {
        if (speedCoroutine != null) StopCoroutine(speedCoroutine);
        speedCoroutine = StartCoroutine(SpeedBuffRoutine(_value, _usingTime));
    }

    /// <summary>
    ///  時間まで移動速度を上げておく実行処理(コルーチン)
    /// </summary>
    private IEnumerator SpeedBuffRoutine(float value, float duration) {
        MoveSpeed = Mathf.RoundToInt(defaultMoveSpeed * value);
        yield return new WaitForSeconds(duration);
        MoveSpeed = defaultMoveSpeed;
        speedCoroutine = null;
    }

    /// <summary>
    /// すべてのバフを即解除
    /// </summary>
    [Command]public void RemoveBuff() {
        StopAllCoroutines();
        MoveSpeed = defaultMoveSpeed;
        Attack = defaultAttack;
        healCoroutine = speedCoroutine = attackCoroutine = null;
    }
    #endregion
}