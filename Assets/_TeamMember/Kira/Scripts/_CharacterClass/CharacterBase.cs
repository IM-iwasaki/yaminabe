using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static TeamData;
[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(NetworkTransformHybrid))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]

/// <summary>
/// 全てのキャラクターの基底
/// </summary>
public abstract class CharacterBase : NetworkBehaviour {
    #region Transform系変数
    //実際に移動する方向
    public Vector3 moveDirection { get ; private set; }
    //射撃位置
    public Transform firePoint;
    //足元の確認用Transform
    private Transform GroundCheck;
    //GroundLayer
    private LayerMask GroundLayer;

    #endregion

    //コンポーネント情報
    [Header("コンポーネント情報")]
    [System.NonSerialized]public Rigidbody rb;
    protected Collider useCollider;
    private string useTag;
    public PlayerLocalUIController localUI = null;
    public OptionMenu CameraMenu;
    public Animator anim = null;
    private string currentAnimation;
    #region バフ関連変数

    private Coroutine healCoroutine;
    private Coroutine speedCoroutine;
    private Coroutine attackCoroutine;
    public int defaultMoveSpeed;
    public int defaultAttack;
    [Header("バフに使用するエフェクトデータ")]
    [SerializeField] private EffectData buffEffect;

    private readonly string EFFECT_TAG = "Effect";
    private readonly int ATTACK_BUFF_EFFECT = 0;
    private readonly int SPEED_BUFF_EFFECT = 1;
    private readonly int HEAL_BUFF_EFFECT = 2;
    private readonly int DEBUFF_EFFECT = 3;

    #endregion

    public CharacterInput input { get; private set; }
    public CharacterParamater paramater { get; private set; }

    #region ～初期化関係関数～

    /// <summary>
    /// 初期化をここで行う。
    /// </summary>
    protected void Awake() {
        //シーン変わったりしても消えないようにする
        DontDestroyOnLoad(gameObject);

        //参照を取得
        input = GetComponent<CharacterInput>();
        paramater = GetComponent<CharacterParamater>();

        // 必要なら基底に初期化合図を送る
        input.Initialize(this);
        paramater.Initialize(this);

        rb = GetComponent<Rigidbody>();

        // "Ground" という名前のレイヤーを取得してマスク化
        int groundLayerIndex = LayerMask.NameToLayer("Ground");
        GroundLayer = 1 << groundLayerIndex;

        //GroundCheck変数をアタッチする。
        GroundCheck = transform.Find("FootRoot");

        // デフォルト値保存
        defaultMoveSpeed = paramater.moveSpeed;
        defaultAttack = paramater.attack;
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
#if UNITY_EDITOR
                Debug.LogWarning("PlayerSetup: No ReticleOptionUI found as child for local player.");
#endif
            }

            //タハラ
            //準備状態を明示的に初期化
            //ホストでなければ非準備状態
            if (isLocalPlayer && !isServer)
                paramater.ready = false;
        }
    }
    public override void OnStartClient() {
        if (isLocalPlayer) {
            base.OnStartClient();

        }

        // ここを追加：クライアント側で TeamGlowManager に登録
        if (TeamGlowManager.Instance != null) {
            TeamGlowManager.Instance.RegisterPlayer(this);
        }
    }  

    /// <summary>
    /// プレイヤー名用セッター
    /// 名前をサーバー側で反映し、PlayerListManager に登録する
    /// </summary>
    [Command]
    public void CmdSetPlayerName(string name) {
        paramater.PlayerName = name;
#if UNITY_EDITOR
        Debug.Log($"[CharacterBase] 名前設定: {paramater.PlayerName}");
#endif

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
    /// 追加:タハラ クライアント用準備状態切り替え関数
    /// </summary>
    [Command]
    public void CmdChangePlayerReady() {
        if (SceneManager.GetActiveScene().name == GameSceneManager.Instance.gameSceneName) return;
        paramater.ready = !paramater.ready;
        ChatManager.instance.CmdSendSystemMessage(paramater.PlayerName + " ready :  " + paramater.ready);
    }

    /// <summary>
    /// 被弾・死亡判定関数
    /// </summary>
    [Server]
    public void TakeDamage(int _damage, string _name) {
        //既に死亡状態かロビー内なら帰る
        if (paramater.isDead || !GameManager.Instance.IsGameRunning()) return;

        //ダメージ倍率を適用
        float damage = _damage * ((float)paramater.DamageRatio / 100);
        //ダメージが0以下だったら1に補正する
        if (damage <= 0) damage = 1;
        //HPの減算処理
        paramater.HP -= (int)damage;

        //　nameをスコア加算関数に送る
        if (paramater.HP <= 0) {
            paramater.HP = 0;
            //  キルログを流す(最初の引数は一旦仮で海老の番号、本来はバナー画像の出したい番号を入れる)
            KillLogManager.instance.CmdSendKillLog(4, _name, paramater.PlayerName);
            KillLogManager.instance.CmdSendKillLog(4, _name, paramater.PlayerName);
            Dead(_name);
            if (PlayerListManager.Instance != null) {
                // スコア加算
                PlayerListManager.Instance.AddScoreByName(_name, 100);
            }
            // キル数加算
            if (PlayerListManager.Instance != null) PlayerListManager.Instance.AddKill(_name);
        }
    }

    /// <summary>
    /// PlayerLocalUIControllerの取得用ゲッター
    /// </summary>
    public PlayerLocalUIController GetPlayerLocalUI() { return GetComponent<PlayerLocalUIController>(); }

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
    /// 死亡時処理 サーバーで処理
    /// </summary>
    [Server]
    public void Dead(string _name) {
        if (paramater.isDead) return;
        //isLocalPlayerはサーバー処理に不必要らしいので消しました byタハラ
        //死亡フラグをたててHPを0にしておく
        paramater.isDead = true;
        ChatManager.instance.CmdSendSystemMessage(_name + " is Dead!!");
        //死亡トリガーを発火
        paramater.isDeadTrigger = true;
        //バフ全解除
        RemoveBuff();
        //ホコを所持していたらドロップ
        if (RuleManager.Instance.currentRule == GameRuleType.Hoko) DropHoko();
        //ローカルで死亡演出
        LocalDeadEffect();
        RespawnDelay();
        //アニメーションは全員に反映
        RpcDeadAnimation();
        // スコア計算にここから行きます
        if (TryGetComponent<PlayerCombat>(out var combat)) {
            int victimTeam = paramater.TeamID;
            NetworkIdentity killerIdentity = null;

            if (!string.IsNullOrEmpty(_name) && _name != paramater.PlayerName) {
                foreach (var p in FindObjectsOfType<CharacterBase>()) {
                    if (p.paramater.PlayerName == _name) {
                        killerIdentity = p.GetComponent<NetworkIdentity>();
                        break;
                    }
                }
            }
            // OnKill を呼ぶときに victimTeam を渡すように変更
            combat.OnKill(killerIdentity, victimTeam);
        }
        else {
#if UNITY_EDITOR
            Debug.LogWarning("スコア計算が正常に成功しませんでした。");
#endif
        }
        // 死亡回数を増やす
        if (PlayerListManager.Instance != null) PlayerListManager.Instance.AddDeath(paramater.PlayerName);
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
#if UNITY_EDITOR
            Debug.LogWarning("StageManager か Hoko が存在しません");
#endif
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
        paramater.HP = paramater.maxHP;
        paramater.isDead = false;
    }

    /// <summary>
    /// リスポーン関数
    /// 死亡した対象にのみ通知
    /// </summary>
    [TargetRpc]
    public void Respawn() {
        //死んでいなかったら即抜け
        if (!paramater.isDead) return;

        //パッシブのセットアップ
        paramater.equippedPassives[0].PassiveSetting(this);

        ChatManager.instance.CmdSendSystemMessage("isDead : " + paramater.isDead);
        //保険で明示的に処理
        paramater.ChangeHP(paramater.maxHP, paramater.HP);
        //リスポーン地点に移動させる
        if (GameManager.Instance.IsGameRunning()) {
            int currentTeamID = paramater.TeamID;
            paramater.TeamID = -1;
            NetworkTransformHybrid NTH = GetComponent<NetworkTransformHybrid>();
            var RespownPos = GameObject.FindGameObjectsWithTag("NormalRespawnPoint"); ;
            NTH.CmdTeleport(RespownPos[Random.Range(0, RespownPos.Length)].transform.position, Quaternion.identity);

            paramater.TeamID = currentTeamID;
        }

        //リスポーン後の無敵時間にする
        paramater.isInvincible = true;
        //経過時間をリセット
        paramater.respownAfterTime = 0;

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

    //ここから古谷が追加 エフェクト表示のための関数

    /// <summary>
    /// クライアントエフェクト表示
    /// </summary>
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
    /// チーム参加処理(TeamIDを更新)
    /// </summary>
    [Command]
    public void CmdJoinTeam(NetworkIdentity _player, TeamColor _color) {
        CharacterParamater player = _player.GetComponent<GeneralCharacter>().paramater;
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
        ChatManager.instance.CmdSendSystemMessage(_player.GetComponent<GeneralCharacter>().paramater.PlayerName + " is joined " + newTeam + " team ");
    }

    /// <summary>
    /// アニメーターのレイヤー切り替え
    /// </summary>
    [Server]
    public void ChangeLayerWeight(int _layerIndex) {
        //ベースのレイヤーを飛ばし、引数と一致したレイヤーを使うようにする
        for (int i = 1, max = anim.layerCount - 1; i < max; i++) {
            anim.SetLayerWeight(i, i == _layerIndex ? 1.0f : 0.0f);
        }
    }

    /// <summary>
    /// 追加:タハラ
    /// アイテムを取ったクライアントの武器の見た目変更
    /// </summary>
    /// <param name="_player"></param>
    /// <param name="_ID"></param>
    [Command]
    public void CmdChangeWeapon(int _ID) {
        RpcChangeWeapon(_ID);
    }

    /// <summary>
    /// 追加:タハラ
    /// 全クライアントに見た目変更を指令
    /// </summary>
    /// <param name="_playerID"></param>
    /// <param name="_ID"></param>
    [ClientRpc]
    private void RpcChangeWeapon(int _ID) {
        Transform handRoot = GetComponent<CharacterBase>().anim.GetBoneTransform(HumanBodyBones.RightHand);

        //今現在持っている武器に新たなメッシュを反映
        GameObject currentWeapon = handRoot.GetChild(3).gameObject;
        Destroy(currentWeapon);

        //新たに武器を生成
        WeaponModelList modelList = FindAnyObjectByType<WeaponModelList>();
        GameObject newWeapon = Instantiate(modelList.weaponModelList[_ID], handRoot);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.Euler(0.0f, 90.0f, 90.0f);
    }
    #endregion

    #region 入力受付・入力実行・判定関数

    

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
                paramater.isCanPickup = true;
                useCollider = _collider;
                localUI.OnChangeInteractUI();
                break;
            case "SelectCharacterObject":
                // フラグを立てる
                paramater.isCanInteruct = true;
                useCollider = _collider;
                useTag = "SelectCharacterObject";
                localUI.OnChangeInteractUI();
                break;
            case "Gacha":
                paramater.isCanInteruct = true;
                useCollider = _collider;
                useTag = "Gacha";
                localUI.OnChangeInteractUI();
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
                paramater.isCanPickup = false;
                useCollider = null;
                localUI.OffChangeInteractUI();
                break;
            case "SelectCharacterObject":
                // フラグを下ろす
                paramater.isCanInteruct = false;
                useCollider = null;
                useTag = null;
                localUI.OffChangeInteractUI();
                break;
            case "Gacha":
                paramater.isCanInteruct = false;
                useCollider = null;
                useTag = null;
                localUI.OffChangeInteractUI();
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
        paramater.isCanPickup = false;
        useCollider = null;
        localUI.OffChangeInteractUI();
    }

    /// <summary>
    /// 移動関数(死亡中は呼ばないでください。)
    /// </summary>
    protected void MoveControl() {
        //移動入力が行われている間は移動中フラグを立てる
        if (input.MoveInput != Vector2.zero) paramater.isMoving = true;
        else paramater.isMoving = false;

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
        moveDirection = forward * input.MoveInput.y + right * input.MoveInput.x;

        // カメラの向いている方向をプレイヤーの正面に
        Vector3 aimForward = forward; // 水平面だけを考慮
        if (aimForward != Vector3.zero) {
            Quaternion targetRot = Quaternion.LookRotation(aimForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, PlayerConst.TURN_SPEED * Time.deltaTime);
        }

        // 空中か地上で挙動を分ける
        Vector3 velocity = rb.velocity;
        Vector3 targetVelocity = new(moveDirection.x * paramater.moveSpeed, velocity.y, moveDirection.z * paramater.moveSpeed);

        //地面に立っていたら通常通り
        if (paramater.IsGrounded) {
            rb.velocity = targetVelocity;
        }
        else {
            // 空中では地上速度に向けてゆるやかに補間（慣性を残す）
            rb.velocity = Vector3.Lerp(velocity, targetVelocity, Time.deltaTime * 2f);
        }
    }

    /// <summary>
    /// 移動アニメーションの管理
    /// </summary>
    /// <param name="_x"></param>
    /// <param name="_z"></param>
    [Command]
    public void ControllMoveAnimation(float _x, float _z) {
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
    public void CmdResetAnimation() {
        ResetRunAnimation();
    }

    /// <summary>
    /// ジャンプ管理関数(死亡中は呼ばないでください。)
    /// </summary>
    protected void JumpControl() {
        // ジャンプ判定
        if (paramater.IsJumpPressed && paramater.IsGrounded) {
            // 現在の速度をリセットしてから上方向に力を加える
            Vector3 velocity = rb.velocity;
            velocity.y = 0f;
            rb.velocity = velocity;

            rb.AddForce(Vector3.up * PlayerConst.JUMP_FORCE, ForceMode.Impulse);
            paramater.IsJumpPressed = false; // 連打防止
        }

        //ベクトルが上方向に働いている時
        if (rb.velocity.y > 0) {
            //追加の重力補正を掛ける
            rb.velocity += (PlayerConst.JUMP_UPFORCE - 1) * Physics.gravity.y * Time.deltaTime * Vector3.up;
        }
        // ベクトルが下方向に働いている時
        else if (rb.velocity.y < 0) {
            //追加の重力補正を掛ける
            rb.velocity += (PlayerConst.JUMP_DOWNFORCE - 1) * Physics.gravity.y * Time.deltaTime * Vector3.up;
            anim.SetBool("Jump", false);
        }


        // 地面判定（下方向SphereCastでもOK。そこまで深く考えなくていいかも。）
        paramater.IsGrounded = Physics.CheckSphere(GroundCheck.position, PlayerConst.GROUND_DISTANCE, GroundLayer);
    }

    /// <summary>
    /// リスポーン管理関数(死亡中も呼んでください。)
    /// </summary>
    virtual protected void RespawnControl() {
        //死亡した瞬間の処理
        if (paramater.isDeadTrigger) {
            Invoke(nameof(Respawn), PlayerConst.RESPAWN_TIME);
        }
        //復活後であるときの処理
        if (paramater.isInvincible) {
            //復活してからの時間を加算
            paramater.respownAfterTime += Time.deltaTime;
            //規定時間経過後無敵状態を解除
            if (paramater.respownAfterTime >= PlayerConst.RESPAWN_INVINCIBLE_TIME) {
                paramater.isInvincible = false;
            }
        }
    }

    /// <summary>
    /// 攻撃入力のハンドル分岐
    /// </summary>
    public void HandleAttack(InputAction.CallbackContext context) {
        //死亡していたら攻撃できない
        if (paramater.isDead || !isLocalPlayer) return;

        //入力タイプで分岐
        switch (context.phase) {
            //押した瞬間から
            case InputActionPhase.Started:
                paramater.isAttackPressed = true;
                break;
            //離した瞬間まで
            case InputActionPhase.Canceled:
                paramater.isAttackPressed = false;
                StopShootAnim();
                break;
            //押した瞬間
            case InputActionPhase.Performed:
                paramater.isAttackTrigger = true;
                break;
        }

    }
    /// <summary>
    /// 攻撃関数
    /// </summary>
    virtual public void StartAttack() {
        if (paramater.weaponController_main == null) return;

        if (HostUI.isVisibleUI == true) return;
        
        //最後に攻撃した時間を記録
        paramater.attackStartTime = Time.time;
        // 武器が攻撃可能かチェックしてサーバー命令を送る(CmdRequestAttack武器種ごとの分岐も側で)
        Vector3 shootDir = GetShootDirection();
        paramater.weaponController_main.CmdRequestAttack(shootDir);
    }

    /// <summary>
    /// 追加:タハラ　入力がなくなったらショットアニメーション終了
    /// </summary>
    [Command]
    private void StopShootAnim() {
        //アニメーション終了
        anim.SetBool("Shoot", false);
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
        if (Physics.Raycast(firePoint.position, direction, out RaycastHit hit, 50f)) {
            // 壁や床に当たればその位置に補正
            return (hit.point - firePoint.position).normalized;
        }

        // 当たらなければそのままaimPoint方向
        return direction;
    }
    /// <summary>
    /// スキル呼び出し関数
    /// </summary>
    /// 
    /// <summary>
    /// インタラクト関数
    /// </summary>
    public void Interact() {
        if (paramater.isCanPickup) {
            ItemBase item = useCollider.GetComponent<ItemBase>();
            item.Use(gameObject);
            return;
        }
        if (paramater.isCanInteruct) {
            if (useTag == "SelectCharacterObject") {
                CharacterSelectManager select = useCollider.GetComponentInParent<CharacterSelectManager>();
                select.StartCharacterSelect(gameObject);
                return;
            }
            if (useTag == "Gacha") {
                GachaSystem gacha = useCollider.GetComponentInParent<GachaSystem>();
                gacha.StartGachaSelect(gameObject);
                localUI.gameObject.SetActive(false);
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
        float totalHeal = paramater.maxHP * _value;
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
            if (paramater.isDead) yield break; // 死亡時は即終了

            healBuffer += healPerSec * Time.deltaTime; // 累積
            if (healBuffer >= 1f) {
                int healInt = Mathf.FloorToInt(healBuffer); // 整数分だけ反映
                paramater.HP = Mathf.Min(paramater.HP + healInt, paramater.maxHP);
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
        paramater.attack = Mathf.RoundToInt(defaultAttack * _value);
        yield return new WaitForSeconds(_duration);
        paramater.attack = defaultAttack;
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
        paramater.moveSpeed = Mathf.RoundToInt(defaultMoveSpeed * _value);
        yield return new WaitForSeconds(_duration);
        paramater.moveSpeed = defaultMoveSpeed;
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
        paramater.moveSpeed = defaultMoveSpeed;
        paramater.attack = defaultAttack;
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