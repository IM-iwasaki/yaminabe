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
public abstract class CharacterBase : CreatureBase {

    //コンポーネント情報
    [Header("コンポーネント情報")]
    [System.NonSerialized] public Rigidbody rb;
    public PlayerLocalUIController localUI = null;
    [SerializeField] private OptionMenu CameraMenu;

    //武器を使用するため
    [Header("アクション用変数")]
    public MainWeaponController weaponController_main;
    public SubWeaponController weaponController_sub;

    //スキル使用中か
    [SyncVar] public bool isSkillUse = false;

    // ホコを持っているか判定(PVE用)
    [SyncVar] private bool isHoldingHoko = false;

    // 世界の外の落下判定のy座標
    [SerializeField] private float fallYposition = -50.0f;

    // リスポーン後の無敵判定用
    [SyncVar] private bool isInvincible = false;

    public int bannerNum = 0;

    protected List<List<TemporaryBuff>> temporaryBuffs = new List<List<TemporaryBuff>>((int)ParamaterType.max);

    //各コンポーネントの参照
    public CharacterInput input { get; private set; }
    public CharacterActions action { get; private set; }
    public CharacterAnimationController animCon { get; private set; }

    private bool isInitialize = false;

    #region ～初期化関係関数～

    /// <summary>
    /// 初期化をここで行う。
    /// </summary>
    protected override void Awake() {
        //シーン変わったりしても消えないようにする
        DontDestroyOnLoad(gameObject);

        base.Awake();
        //各コンポーネントの参照取得と初期化
        rb = GetComponent<Rigidbody>();
        input = GetComponent<CharacterInput>();
        action = GetComponent<CharacterActions>();
        animCon = GetComponent<CharacterAnimationController>();

        //RpcChangeWeapon(weaponController_main.weaponData.appearanceID);
        for (int i = 0; i < (int)ParamaterType.max; i++) {
            temporaryBuffs.Add(new List<TemporaryBuff>());
        }
    }

    /// <summary>
    /// ネットワーク上での初期化。
    /// </summary>
    public override void OnStartLocalPlayer() {
        if (isLocalPlayer) {
            //二回以上呼ばれても弾くようにする
            if (isInitialize) return;
            isInitialize = true;


            input.Initialize(this);
            action.Initialize(this);
            parameter.Initialize(this);
            localUI.InitializeLocalUI(this);
            localUI.ChangeHPUI(parameter.maxHP, parameter.HP);
            localUI.ChangeMPUI(parameter.maxMP, parameter.MP);

            Camera camera = GetComponentInChildren<Camera>();
            camera.tag = "MainCamera";
            camera.enabled = true;
            PlayerCamera playerCamera = camera.GetComponent<PlayerCamera>();
            playerCamera.enabled = true;

            AudioListener listener = GetComponentInChildren<AudioListener>();
            listener.enabled = true;

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
            //準備状態を明示的に初期化。ホストでなければ非準備状態
            if (isClient && !isServer) parameter.ready = false;
        }
    }
    public override void OnStartClient() {
        if (isLocalPlayer) base.OnStartClient();
        //parameter.StatusInport();
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
        parameter.PlayerName = name;
#if UNITY_EDITOR
        Debug.Log($"[CharacterBase] 名前設定: {parameter.PlayerName}");
#endif

        // 名前が確定したタイミングで登録（サーバー側のみ）
        if (isServer && PlayerListManager.Instance != null) {
            PlayerListManager.Instance.RegisterPlayer(this);
        }
        //カネダ
        //準備完了か否かをリストUIとして表示する
        if (PlayerListUIManager.Instance != null) {
            PlayerListUIManager.Instance.UpdatePlayerList();
        }
    }
    /// <summary>
    /// 名前をリストから消す
    /// </summary>
    public override void OnStopServer() {
        base.OnStopServer();
        if (PlayerListManager.Instance != null) PlayerListManager.Instance.UnregisterPlayer(this);
        //カネダ
        //準備完了か否かをリストUIとして表示する
        if (PlayerListUIManager.Instance != null) {
            PlayerListUIManager.Instance.UpdatePlayerList();
        }
    }

    #endregion

    #region ～プレイヤー状態更新関数～

    /// <summary>
    /// 追加:タハラ クライアント用準備状態切り替え関数
    /// </summary>
    [Command]
    public void CmdChangePlayerReady() {
        parameter.ready = !parameter.ready;

        ChatManager.Instance.CmdSendSystemMessage(parameter.PlayerName + " ready : " + parameter.ready);
    }
    [Command]
    public void CmdChangePlayerReadyFalse() {
        parameter.ready = false;

        ChatManager.Instance.CmdSendSystemMessage(parameter.PlayerName + " ready : " + parameter.ready);
    }

    /// <summary>
    /// 被弾・死亡判定関数
    /// </summary>
    [Server]
    public override void TakeDamage(int _damage, string _name, int _ID) {
        // 無敵中はダメージを受けない
        if (isInvincible) return;

        //既に死亡状態かロビー内なら帰る
        if (parameter.isDead || !GameManager.Instance.IsGameRunning()) return;

        //ダメージ倍率を適用
        float damage = _damage * ((float)parameter.damageRatio / 100);
        //ダメージが0以下だったら1に補正する
        if (damage <= 0) damage = 1;
        //HPの減算処理
        parameter.HP -= (int)damage;

        if (_ID != -1)
            // hitSE 再生
            PlayHitSE(_ID);

        //自身の体力が0以下になったらログを送信して死亡処理
        if (parameter.HP <= 0) {
            parameter.HP = 0;

            KillLogManager.instance.CmdSendKillLog(
                bannerNum, _name, parameter.PlayerName
            );

            Dead(_name);

            if (PlayerListManager.Instance != null && _ID != -1) {
                // スコア加算
                PlayerListManager.Instance.AddScoreById(_ID, 100);
                PlayerListManager.Instance.AddKillById(_ID);
            }
        }
    }

    /// <summary>
    /// PlayerLocalUIControllerの取得用ゲッター
    /// </summary>
    public PlayerLocalUIController GetPlayerLocalUI() { return GetComponent<PlayerLocalUIController>(); }

    ///--------------------変更:タハラ---------------------

    /* なんかサーバーで処理できるようになったのでコマンド経由しなくていいです。
     * 読み解くにはこれを読んでください
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
        if (parameter.isDead) return;
        //isLocalPlayerはサーバー処理に不必要らしいので消しました byタハラ
        //死亡フラグをたててHPを0にしておく
        parameter.isDead = true;
        //死亡トリガーを発火
        parameter.StartDeadTrigger();
        //バフ全解除
        RemoveBuff();
        //ホコを所持していたらドロップ
        if (RuleManager.Instance.currentRule == GameRuleType.Hoko) DropHoko();
        //不具合防止のためフラグをいろいろ下ろす。
        animCon.RpcStopShootAnim();
        //アニメーションは全員に反映
        animCon.RpcDeadAnimation();
        //ローカルで死亡演出
        TargetDeadEffect();
        RespawnDelay();

        // スコア計算にここから行きます
        if (TryGetComponent<PlayerCombat>(out var combat)) {
            int victimTeam = parameter.TeamID;
            NetworkIdentity killerIdentity = null;

            if (!string.IsNullOrEmpty(_name) && _name != parameter.PlayerName) {
                foreach (var p in FindObjectsOfType<CharacterBase>()) {
                    if (p.parameter.PlayerName == _name) {
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
        //// 死亡回数を増やす
        if (PlayerListManager.Instance != null) PlayerListManager.Instance.AddDeathById(parameter.playerId);
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
        Invoke(nameof(TargetRespawnEffect), PlayerConst.RESPAWN_TIME);
    }

    /// <summary>
    /// ローカル上で死亡演出 可読性向上のためまとめました
    /// </summary>
    [TargetRpc]
    private void TargetDeadEffect() {
        //カメラを暗くする
        gameObject.GetComponentInChildren<PlayerCamera>().EnterDeathView();
        //フェードアウトさせる
        FadeManager.Instance.StartFadeOut(2.5f);
    }

    /// <summary>
    /// HPの初期化、死亡状態の解除
    /// </summary>
    [Server]
    public void ResetHealth() {
        //ここで体力と死亡状態を戻す
        parameter.HP = parameter.maxHP;
        parameter.isDead = false;
    }

    /// <summary>
    /// リスポーン関数 死亡した対象にのみ通知
    /// </summary>
    [Server]
    virtual public void Respawn() {
        if (!parameter.isDead) return;

        if (!GameManager.Instance.IsGameRunning()) return;

        var stageManager = StageManager.Instance;
        if (stageManager == null) return;

        var teamColor = (TeamData.TeamColor)parameter.TeamID;
        Transform spawnPoint = stageManager.GetSpawnPoint(teamColor);

        if (spawnPoint == null) {
#if UNITY_EDITOR
            Debug.LogWarning("スポーン地点が取得できませんでした");
#endif
            return;
        }
        Vector3 bufferPos = new Vector3(Random.Range(-3.0f, 3.0f), 1.0f, Random.Range(-3.0f, 3.0f));
        NetworkTransformHybrid NTH = GetComponent<NetworkTransformHybrid>();
        NTH.ServerTeleport(spawnPoint.position + bufferPos, spawnPoint.rotation);

        parameter.StartInvincible();
        ResetHealth();
        // 無敵状態を開始（3秒間）
        StartCoroutine(InvincibleRoutine(3.0f));
    }

    /// <summary>
    /// 無敵状態の付与
    /// </summary>
    /// <param name="duration">効果時間</param>
    /// <returns></returns>
    [Server]
    private IEnumerator InvincibleRoutine(float duration) {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }

    /// <summary>
    /// ローカル上での演出
    /// 可読性向上のためまとめました
    /// </summary>
    [TargetRpc]
    private void TargetRespawnEffect() {
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

    /// <summary>
    /// チーム参加処理(TeamIDを更新)
    /// </summary>
    [Command]
    public void CmdJoinTeam(NetworkIdentity _player, TeamColor _color) {
        GeneralCharacter player = _player.GetComponent<GeneralCharacter>();
        int currentTeam = player.parameter.TeamID;
        int newTeam = (int)_color;

        //チームから抜けるとき
        if (newTeam == -1) {
            ChatManager.Instance.CmdSendSystemMessage("you leave team now");
            if (currentTeam == -1)
                return;
            ServerManager.instance.teams[currentTeam].teamPlayerList.Remove(_player);
            player.parameter.TeamID = -1;
            return;
        }

        //加入しようとしてるチームが埋まっていたら
        if (ServerManager.instance.teams[newTeam].teamPlayerList.Count >= ServerManager.instance.teammateMax) {
            ChatManager.Instance.CmdSendSystemMessage("team member is over");
            return;
        }
        //既に同じチームに入っていたら
        if (newTeam == currentTeam) {
            ChatManager.Instance.CmdSendSystemMessage("you join same team now");
            return;
        }


        //新たなチームに加入する時
        //今加入しているチームから抜けてIDをリセット
        if (player.parameter.TeamID != -1) {
            ServerManager.instance.teams[player.parameter.TeamID].teamPlayerList.Remove(_player);
            player.parameter.TeamID = -1;
        }

        


        //新しいチームに加入
        ServerManager.instance.teams[newTeam].teamPlayerList.Add(_player);
        player.parameter.TeamID = newTeam;
        player.teamID = newTeam;
        //ログを表示
        ChatManager.Instance.CmdSendSystemMessage(_player.GetComponent<CharacterParameter>().PlayerName + " is joined " + newTeam + " team ");

        //カネダ
        //チームカラーを反映させるために設置
        if (PlayerListUIManager.Instance != null) {
            PlayerListUIManager.Instance.UpdatePlayerList();
        }
    }

    /// <summary>
    /// 追加:タハラ
    /// アイテムを取ったクライアントの武器の見た目変更
    /// </summary>
    [Command]
    public void CmdChangeWeapon(int _ID) {
        RpcChangeWeapon(_ID);
    }

    /// <summary>
    /// 追加:タハラ
    /// 全クライアントに見た目変更を指令
    /// </summary>
    [ClientRpc]
    public void RpcChangeWeapon(int _ID) {
        Debug.Log($"RpcChangeWeapon received on {gameObject.name}");
        StartCoroutine(WaitChangeWeapon(_ID));
    }

    /// <summary>
    /// ボーン取得のため一フレーム待って武器変更
    /// </summary>
    /// <param name="_ID"></param>
    /// <returns></returns>
    private IEnumerator WaitChangeWeapon(int _ID) {
        yield return null;
        float timeout = 3f;
        while ((animCon == null || animCon.anim == null) && timeout > 0.0f) {
            timeout -= Time.deltaTime;
            yield return null;
        }
        if (timeout <= 0.0f) yield break;
        Transform handRoot = GetComponent<CharacterAnimationController>().anim.GetBoneTransform(HumanBodyBones.RightHand);
        while (handRoot == null) yield return null;
        //武器専用ルートを探す
        Transform weaponRoot = handRoot.Find("WeaponRoot");
        //見つかるまで待つ
        while (weaponRoot == null) yield return null;

        // 武器ルートに子が存在していれば全て削除
        foreach (Transform child in weaponRoot) {
            Destroy(child.gameObject);
        }

        //モデルリストはあるか
        while (WeaponModelList.instance == null) {
            Debug.LogWarning("モデルリストないで");
            yield return null;
        }
        WeaponModelList modelList = WeaponModelList.instance;

        //IDが既定範囲内にあるか
        if (_ID < 0 || _ID >= modelList.weaponModelList.Count) {
            Debug.LogWarning("IDおかしいで");
            yield break;
        }

        Instantiate(modelList.weaponModelList[_ID], weaponRoot);

        animCon.SetWeaponLayer(weaponController_main.GenerateWeaponIndex(weaponController_main.weaponData.weaponName));
    }

    // ホコを持っているか判定の切り替え用
    public bool HasHoko() => isHoldingHoko;

    [Server]
    public void SetHoldingHoko(bool value) => isHoldingHoko = value;

    /// <summary>
    /// MP消費量のコントロール
    /// </summary>
    /// <param name="magicData">参照する魔法のデータ</param>
    /// <returns>最終的なMP消費量を返します。</returns>
    public int GetCurrentMPCost(MainMagicData magicData) {
        if (parameter.equippedSkills[0] is Skill_Chaser && isSkillUse) return 0;

        return magicData.MPCost;
    }

    /// <summary>
    /// バフの総合管理
    /// </summary>
    protected void BuffUpdate() {
        //保険。大丈夫なはずだがバフリストのnullチェック
        if (temporaryBuffs == null) {
#if UNITY_EDITOR
            Debug.LogWarning("なんでtemporaryBuffsないの！");
#endif
            return;
        }

        //バフデバフの処理
        for (int paramIndex = (int)ParamaterType.max - 1; 0 <= paramIndex; paramIndex--) {
            //倍率の累計
            float influxValue = 1.0f;
            //倍率の最高値
            float maxInfluxValue = 0.0f;

            //各バフの反映と時間経過処理
            //【走査中に途中で要素を削除するなら末尾から見る！】
            for (int buffIndex = temporaryBuffs[paramIndex].Count - 1; 0 <= buffIndex; buffIndex--) {
                if (maxInfluxValue <= temporaryBuffs[paramIndex][buffIndex].amount)
                    maxInfluxValue = temporaryBuffs[paramIndex][buffIndex].amount;

                //倍率の累計に掛けていく
                influxValue *= temporaryBuffs[paramIndex][buffIndex].amount;
                //反映したら効果時間を減らす
                temporaryBuffs[paramIndex][buffIndex].duration -= Time.deltaTime;

                //効果時間が切れたら
                if (temporaryBuffs[paramIndex][buffIndex].duration <= 0.0f) {
                    //該当する要素を削除。
                    temporaryBuffs[paramIndex].RemoveAt(buffIndex);
                    DestroyEffect();
                }
            }

            //倍率が一定を超えたら該当バフ内の最高値のみを参照する
            if (influxValue >= 2.0f) influxValue = maxInfluxValue;

            //値を反映(switchで分岐)
            switch ((ParamaterType)paramIndex) {
                case ParamaterType.Attack:
                    parameter.attack = parameter.defaultAttack * influxValue;
                    break;
                case ParamaterType.moveSpeed:
                    parameter.moveSpeed = parameter.defaultMoveSpeed * influxValue;
                    break;
                case ParamaterType.damageRate:
                    parameter.damageRatio = parameter.defaultDamageRatio * influxValue;
                    break;
                default:
#if UNITY_EDITOR
                    Debug.LogWarning("実行されないべきであるコードが実行されました。\n発生：CharacterBase.BuffUpdate：551");
#endif
                    break;
            }
        }
    }

    #endregion

    #region 入力受付・入力実行・判定関数

    /// <summary>
    /// 追加:タハラ UI表示
    /// </summary>
    public void OnShowHostUI(InputAction.CallbackContext context) {
        if (!isServer || !isLocalPlayer || LoadingUI.instance.isLoading
            || SceneManager.GetActiveScene().name == "GameScene"
            || SceneManager.GetActiveScene().name == "PvEScene") return;
        if (context.started) {
            if (CameraMenu.isOpen)
                CameraMenu.ToggleMenu();
            HostUI.ShowOrHideUI();
        }
    }

    public void OnShowCameraMenu(InputAction.CallbackContext context) {
        if (!isLocalPlayer)
            return;
        if (context.started) {
            if (HostUI.isVisibleUI) {
                HostUI.ShowOrHideUI();
            }

            CameraMenu.ToggleMenu();
        }
    }

    /// <summary>
    /// 追加:カネダ 世界の外に落ちた際の判定と処理
    /// </summary>
    public void CheckOutsideAndFall() {
        if (transform.position.y <= fallYposition) {
            TakeDamage(9999, "Fall", -1);
        }
    }

    #endregion

    #region おれのじゃないやつ

    #region チーム管理関連

    /// <summary>
    /// 追加:タハラ プレイヤーの準備状態切り替え
    /// </summary>
    /// <param name="context"></param>
    public void OnReadyPlayer(InputAction.CallbackContext context) {
        if (!isLocalPlayer || SceneManager.GetActiveScene().name == "GameScene")
            return;

        if (context.started) {
            CmdChangePlayerReady();
        }
    }

    /// <summary>
    /// 追加:タハラ チャット送信
    /// </summary>
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
        ChatManager.Instance.CmdSendChatMessage(parameter.TeamID, "<" + parameter.PlayerName + "> " + sendMessage);
    }

    /// <summary>
    /// 追加:タハラ スタンプ送信
    /// </summary>
    public void OnSendStamp(InputAction.CallbackContext context) {
        if (!isLocalPlayer) return;
        //チャット送信
        if (context.started) {
            int stampIndex = Random.Range(0, 4);
            ChatManager.Instance.CmdSendStamp(parameter.TeamID, stampIndex, parameter.PlayerName);
        }
    }

    #endregion

    #region バフ関連

    public Coroutine healCoroutine { get; private set; }
    public Coroutine speedCoroutine { get; private set; }
    public Coroutine attackCoroutine { get; private set; }
    public Coroutine damageCutCoroutine { get; private set; }

    [Header("バフに使用するエフェクトデータ")]
    [SerializeField] private EffectData buffEffect;

    private readonly string EFFECT_TAG = "Effect";
    private readonly int ATTACK_BUFF_EFFECT = 0;
    private readonly int SPEED_BUFF_EFFECT = 1;
    private readonly int HEAL_BUFF_EFFECT = 2;
    private readonly int DEBUFF_EFFECT = 3;

    //近くにいるか判別
    [SerializeField] public float allyCheckRadius = 8f;
    [SerializeField] public LayerMask allyLayer;

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
        float totalHeal = parameter.maxHP * _value;
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
            if (parameter.isDead) yield break; // 死亡時は即終了

            healBuffer += healPerSec * Time.deltaTime; // 累積
            if (healBuffer >= 1f) {
                int healInt = Mathf.FloorToInt(healBuffer); // 整数分だけ反映
                parameter.HP = Mathf.Min(parameter.HP + healInt, parameter.maxHP);
                healBuffer -= healInt; // 余りを保持
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        DestroyEffect();
        healCoroutine = null;
    }

    /// <summary>
    /// 攻撃力上昇バフ発動 [_valueは1.0fを100％とした相対値]
    /// </summary>
    [Command]
    public void AttackBuff(float _value, float _usingTime) {
        temporaryBuffs[(int)ParamaterType.Attack].Add(new TemporaryBuff(_value, _usingTime));

        if (attackCoroutine != null) StopCoroutine(attackCoroutine);

        //  エフェクト再生
        PlayEffect(ATTACK_BUFF_EFFECT);

        //attackCoroutine = StartCoroutine(AttackBuffRoutine(_value, _usingTime));
    }

    /// <summary>
    ///  時間まで攻撃力を上げておく実行処理(コルーチン)
    /// </summary>
    //private IEnumerator AttackBuffRoutine(float _value, float _duration) {
    //    parameter.attack = Mathf.RoundToInt(parameter.defaultAttack * _value);
    //    yield return new WaitForSeconds(_duration);
    //    parameter.attack = parameter.defaultAttack;
    //    DestroyEffect();
    //    attackCoroutine = null;
    //}

    /// <summary>
    /// 移動速度上昇バフ発動 [_valueは1.0fを100％とした相対値]
    /// </summary>
    //[Command]

    [Server]
    public void MoveSpeedBuff(float _value, float _usingTime) {
        temporaryBuffs[(int)ParamaterType.moveSpeed].Add(new TemporaryBuff(_value, _usingTime));

        if (speedCoroutine != null) StopCoroutine(speedCoroutine);
        //  エフェクト再生
        PlayEffect(SPEED_BUFF_EFFECT);

        //speedCoroutine = StartCoroutine(SpeedBuffRoutine(_value, _usingTime));
    }

    /// <summary>
    ///  時間まで移動速度を上げておく実行処理(コルーチン)
    /// </summary>
    //private IEnumerator SpeedBuffRoutine(float _value, float _duration) {
    //    parameter.moveSpeed = Mathf.RoundToInt(parameter.defaultMoveSpeed * _value);
    //    yield return new WaitForSeconds(_duration);
    //    parameter.moveSpeed = parameter.defaultMoveSpeed;
    //    DestroyEffect();
    //    speedCoroutine = null;
    //}

    /// <summary>
    /// 古谷作成
    /// 被ダメージ倍率変更処理
    /// </summary>
    [Command]
    public void DamageCut(float _value, float _usingTime) {
        temporaryBuffs[(int)ParamaterType.damageRate].Add(new TemporaryBuff(_value, _usingTime));

        if (damageCutCoroutine != null) StopCoroutine(damageCutCoroutine);

        //damageCutCoroutine = StartCoroutine(DamageCutRoutine(_value, _usingTime));
    }

    /// <summary>
    /// 古谷
    /// 時間まで被ダメを下げておく実行処理(コルーチン)
    /// </summary>
    //private IEnumerator DamageCutRoutine(int _value, float _duration) {
    //    parameter.damageRatio = _value;
    //    Debug.Log("被ダメージ倍率変更中");
    //    yield return new WaitForSeconds(_duration);
    //    parameter.damageRatio = 100;
    //    damageCutCoroutine = null;
    //}

    [Command]
    public void CmdUseConsumable(ConsumableType type, float value, float time) {

        switch (type) {
            case ConsumableType.SpeedUp:
                MoveSpeedBuff(value, time);
                break;
        }
    }

    [Command]
    public void CmdUseSkill(float value, float time) {
        MoveSpeedBuff(value, time);
    }

    /// <summary>
    /// すべてのバフを即解除
    /// </summary>
    [Command]
    public void RemoveBuff() {
        StopAllCoroutines();
        DestroyEffect();
        parameter.moveSpeed = parameter.defaultMoveSpeed;
        parameter.attack = parameter.defaultAttack;
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

    /// <summary>
    /// 追加　マツオ:エフェクト削除
    /// </summary>
    private void DestroyEffect() {
        if (isServer)
            RpcDestroyEffect();
        else
            CmdDestroyEffect();
    }

    [Command]
    private void CmdDestroyEffect() {
        RpcDestroyEffect();
    }

    [ClientRpc]
    private void RpcDestroyEffect() {
        DestroyChildrenWithTag(EFFECT_TAG);
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

    /// <summary>
    /// スキルの状態変化をサーバーに送信
    /// </summary>
    /// <param name="_newState">状態の真偽</param>    
    [Command]
    public void CmdSkillState(bool _newState) {
        ServerSkillState(_newState);
    }

    /// <summary>
    /// スキルの状態変化をサーバー上で反映
    /// </summary>
    [Server]
    public void ServerSkillState(bool _newState) {
        //効果発動
        isSkillUse = _newState;
    }

    #endregion

    #endregion

    #endregion
}

/// <summary>
/// バフ管理用クラス
/// </summary>
[System.Serializable]
public class TemporaryBuff {
    /// <summary>
    /// 効果値
    /// </summary>
    public float amount;
    /// <summary>
    /// 効果時間
    /// </summary>
    public float duration;

    public TemporaryBuff(float amount, float duration) {
        this.amount = amount;
        this.duration = duration;
    }
}