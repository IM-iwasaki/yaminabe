using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static TeamData;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(NetworkTransformHybrid))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]

//
// @flie    First_CharacterClass
//
abstract class CharacterBase : NetworkBehaviour {
    #region ～変数宣言～

    #region ～ステータス～
    //現在の体力
    [SyncVar(hook = nameof(ChangeHP))] public int HP;
    //最大の体力
    public int MaxHP { get; protected set; }
    //基礎攻撃力
    public int Attack { get; protected set; }
    //移動速度
    public int MoveSpeed { get; protected set; } = 5;
    //持っている武器の文字列
    public string CurrentWeapon { get; protected set; }
    //所属チームの番号
    [SyncVar] public int TeamID;
    //プレイヤーの名前
    //TODO:仮。ここの実装は要相談。
    protected string PlayerName = "Player_Test";
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

    //プレイヤーの状態

    //死亡しているか
    protected bool IsDead { get; private set; } = false;
    //攻撃中か
    protected bool IsAttack { get; private set; } = false;

    //アイテムを拾える状態か
    protected bool IsCanPickup { get; private set; } = false;
    //インタラクトできる状態か
    protected bool IsCanInteruct { get; private set; } = false;

    //コンポーネント情報
    protected new Rigidbody rigidbody;
    [SerializeField] protected PlayerUIManager UI;
    [SerializeField] private InputActionAsset inputActions;

    #endregion

    #region ～アクション用変数～

    //武器を使用するため
    [SerializeField] protected NetworkWeapon weaponController;

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

    /// <summary>
    /// 初期化をここで行う。
    /// </summary>
    protected void Awake() {
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
            GameObject GameUIRoot = GameObject.Find("GameUI");
            var playerUI = Instantiate(UI,GameUIRoot.transform);
            UI = playerUI.GetComponent<PlayerUIManager>();

            Camera camera = GetComponentInChildren<Camera>();
            camera.tag = "MainCamera";
            camera.enabled = true;
        }

    }

    /// <summary>
    /// ステータスのインポート
    /// </summary>
    protected abstract void StatusInport();

    /// <summary>
    /// StatusInportでnullが発生した時にデフォルトの値で初期化する
    /// </summary>
    protected void DefaultStatusInport() {
        MaxHP = PlayerConst.DEFAULT_MAXHP;
        HP = MaxHP;
        Attack = PlayerConst.DEFAULT_ATTACK;
        MoveSpeed = PlayerConst.DEFAULT_MOVESPEED;
    }

    /// <summary>
    /// 当たり判定関係
    /// </summary>
    protected void OnTriggerStay(Collider _collider) {
        if (isLocalPlayer) {
            if (_collider.CompareTag("Item")) {
                //アイテム使用キー入力入れる

                ItemBase item = _collider.GetComponent<ItemBase>();
                //仮。挙動確認。
                item.Use(gameObject);
            }
            if (_collider.CompareTag("SelectCharacterObject")) {
                // なんかここにイントラクトのやつ呼んで
                // CharacterSelectManager select = _collider.GetComponent<CharacterSelectManager>();
                // select.StartCharacterSelect(gameObject);
            }
            if (_collider.CompareTag("RedTeam")) {
                CmdJoinTeam(netIdentity, teamColor.Red);
            }
            if (_collider.CompareTag("BlueTeam")) {
                CmdJoinTeam(netIdentity, teamColor.Blue);
            }
        }
    }

    #region ～プレイヤー状態更新関数～

    /// <summary>
    /// 被弾・死亡判定関数
    /// </summary>
    [Command]public void TakeDamage(int _damage) {
        //ダメージが0以下だったら帰る
        if (_damage <= 0)
            return;
        //HPの減算処理
        HP -= _damage;
        //HPが0以下になったらisDeadを真にする
        if (HP <= 0)
            RemoveBuff();
        IsDead = true;
    }

    public void ChangeHP(int oldValue, int newValue) {
        if (isLocalPlayer) {
            UI.ChangeHPUI(MaxHP, newValue);
        }
    }

    /// <summary>
    /// リスポーン関数
    /// </summary>
    [Command]public void Respown() {
        //死んでいなかったら即抜け
        if (!IsDead)
            return;

        //死亡状態解除
        IsDead = false;
        //リスポーン地点に移動させる
        transform.position = RespownPosition;
        //リスポーン時に向きをリセットしたほうがいいかも。その場合ここに書く。

    }

    /// <summary>
    /// チーム参加処理(TeamIDを更新)
    /// </summary>
    [Command]public void CmdJoinTeam(NetworkIdentity _player, teamColor _color) {
        CharacterBase player = _player.GetComponent<CharacterBase>();
        int currentTeam = player.TeamID;
        int newTeam = (int) _color;

        //加入しようとしてるチームが埋まっていたら
        if (ServerManager.instance.teams[(newTeam)].teamPlayerList.Count >= TEAMMATE_MAX) {
            Debug.Log("チームの人数が最大です！");
            return;
        }
        //既に同じチームに入っていたら
        if (newTeam == currentTeam) {
            Debug.Log("今そのチームにいます!");
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
        Debug.Log(_player.ToString() + "は" + newTeam + "番目のチームに加入しました！");
    }

    #endregion

    #region 入力受付・入力実行関数

    /// <summary>
    /// 入力の共通ハンドラ
    /// </summary>
    private void OnInputStarted(string actionName, InputAction.CallbackContext ctx) {
        switch (actionName) {
            case "Jump":
                OnJump(ctx);
                break;
            case "Fire_Main":
            case "Fire_Sub":
                HandleAttack(ctx, actionName == "Attack_Main"
                    ? PlayerConst.AttackType.Main
                    : PlayerConst.AttackType.Sub);
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
            case "Fire_Sub":
                HandleAttack(ctx, actionName == "Attack_Main"
                    ? PlayerConst.AttackType.Main
                    : PlayerConst.AttackType.Sub);
                break;
            case "UseSkill":
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

    /// <summary>
    /// 移動関数
    /// </summary>
    protected void MoveControl() {
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

        // 移動方向にキャラクターを向ける
        //Quaternion targetRotation = Quaternion.LookRotation(MoveDirection);
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

        rigidbody.velocity = new Vector3(MoveDirection.x * MoveSpeed, rigidbody.velocity.y, MoveDirection.z * MoveSpeed);
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
            rigidbody.velocity += Vector3.up * Physics.gravity.y * (PlayerConst.JUMP_UPFORCE - 1) * Time.deltaTime;
        } 
        // ベクトルが下方向に働いている時
        else if (rigidbody.velocity.y < 0) {
            //追加の重力補正を掛ける
            rigidbody.velocity += Vector3.up * Physics.gravity.y * (PlayerConst.JUMP_DOWNFORCE - 1) * Time.deltaTime;
        }


        // 地面判定（下方向SphereCastでもOK。そこまで深く考えなくていいかも。）
        IsGrounded = Physics.CheckSphere(GroundCheck.position, PlayerConst.GROUND_DISTANCE, GroundLayer);
    }

    /// <summary>
    /// 攻撃入力のハンドル分岐
    /// </summary>
    private void HandleAttack(InputAction.CallbackContext context, PlayerConst.AttackType _type) {
        switch (context.phase) {
            case InputActionPhase.Started:
                IsAttack = true;
                break;
            case InputActionPhase.Performed:
                StartAttack(_type);
                break;
            case InputActionPhase.Canceled:
                IsAttack = false;
                break;
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
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = cam.ScreenPointToRay(screenCenter);

        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, 100f)) {
            targetPoint = hit.point;
        }
        else {
            targetPoint = ray.GetPoint(50f); // 当たらなければ50m先
        }

        // firePoint → レティクル命中点 の方向に補正
        return (targetPoint - firePoint.position).normalized;
    }


    //スキル使用関数
    abstract protected void StartUseSkill();

    //インタラクト関数
    protected void Interact() {
    }

    #endregion

    #region ～バフ・ステータス操作系～
    /// <summary>
    /// HP回復（時間経過で徐々に回復）発動
    /// </summary>
    [Command]public void Heal(float _value, float _usingTime) {
        if (healCoroutine != null) StopCoroutine(healCoroutine);

        // 総回復量を MaxHP の割合で計算（例：_value=0.2 → 20％回復）
        float totalHeal = MaxHP * _value;
        //  回復実行（コルーチンで回す）
        healCoroutine = StartCoroutine(HealOverTime(_value, _usingTime));
    }

    /// <summary>
    ///  時間まで徐々に回復させていく実行処理（コルーチン）
    /// </summary>
    private IEnumerator HealOverTime(float totalHeal, float duration) {
        float elapsed = 0f;
        float healPerSec = totalHeal / duration;

        while (elapsed < duration) {
            if (IsDead) yield break; // 死亡時は即終了
            HP = Mathf.Min(HP + Mathf.RoundToInt(healPerSec * Time.deltaTime), MaxHP);
            elapsed += Time.deltaTime;
            yield return null;
        }

        healCoroutine = null;
    }

    /// <summary>
    /// 攻撃力上昇バフ発動
    /// </summary>
    [Command]public void AttackBuff(float _value, float _usingTime) {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackBuffRoutine(_value, _usingTime));
    }

    /// <summary>
    ///  時間まで攻撃力を上げておく実行処理（コルーチン）
    /// </summary>
    private IEnumerator AttackBuffRoutine(float value, float duration) {
        Attack = Mathf.RoundToInt(defaultAttack * value);
        yield return new WaitForSeconds(duration);
        Attack = defaultAttack;
        attackCoroutine = null;
    }

    /// <summary>
    /// 移動速度上昇バフ発動
    /// </summary>
    [Command]public void MoveSpeedBuff(float _value, float _usingTime) {
        if (speedCoroutine != null) StopCoroutine(speedCoroutine);
        speedCoroutine = StartCoroutine(SpeedBuffRoutine(_value, _usingTime));
    }

    /// <summary>
    ///  時間まで移動速度を上げておく実行処理（コルーチン）
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