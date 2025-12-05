using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Mirror.BouncyCastle.Crypto.Digests.SkeinEngine;

/// <summary>
/// キャラクターの持つ変数を管理
/// </summary>
public class CharacterParamater : NetworkBehaviour {
    CharacterBase player;

    #region ～キャラクターデータ管理変数～

    [Header("インポートするステータス")]
    [SerializeField]GeneralCharacterStatus inputStatus;
    //CharacterStatusをキャッシュ(ScriptableObjectを書き換えないための安全策)
    GeneralCharacterStatus runtimeStatus;
    public SkillBase[] equippedSkills{ get; private set; }
    public PassiveBase[] equippedPassives{ get; private set; }

    #endregion

    #region パラメータ変数

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
    [SyncVar(hook = nameof(ChangeMP))] public int MP;
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

    #endregion   

    #region bool系変数＆時間管理系変数

    //死亡しているか
    [SyncVar] public bool isDead = false;
    //死亡した瞬間か
    public bool isDeadTrigger = false;
    //復活後の無敵時間中であるか
    public bool isInvincible = false;
    //復活してからの経過時間
    public float respownAfterTime = 0.0f;
    //移動中か
    public bool isMoving = false;
    //攻撃中か
    public bool isAttackPressed = false;
    //攻撃を押した瞬間か
    public bool isAttackTrigger = false;
    //攻撃開始時間
    public float attackStartTime = 0.0f;
    //アイテムを拾える状態か
    public bool isCanPickup = false;
    //インタラクトできる状態か
    public bool isCanInteruct = false;
    //リロード中か
    [SyncVar(hook = nameof(UpdateReloadIcon))] public bool isReloading = false;
    //スキルを使用できるか
    public bool isCanSkill = false;
    //スキル使用後経過時間
    [System.NonSerialized] public float skillAfterTime = 0.0f;
    //ジャンプ入力をしたか
    public bool IsJumpPressed = false;
    //接地しているか
    public bool IsGrounded;
    // 追加:タハラ プレイヤー準備完了状態
    [SyncVar] public bool ready = true;

    #endregion

    //武器を使用するため
    [Header("アクション用変数")]
    public MainWeaponController weaponController_main;
    public SubWeaponController weaponController_sub;

    /// <summary>
    /// 初期化(Baseが呼び出す。)
    /// </summary>
    /// <param name="_linkTarget">参照する対象</param>
    public void Initialize(CharacterBase _linkTarget) {
        player = _linkTarget;

        StatusInport();
        StateInitalize();

        //一定間隔でMPを回復する
        InvokeRepeating(nameof(MPRegeneration), 0.0f,0.1f);
    }

    /// <summary>
    /// MPを回復する
    /// </summary>
    void MPRegeneration() {  
        //攻撃してから短い間を置く。
        if (Time.time <= attackStartTime + 0.2f) return;
        //止まっているときは回復速度が早くなる。
        if(player.input.MoveInput == Vector2.zero)InvokeRepeating(nameof(MPExtraRegeneration), 0.5f,0.4f);
        else CancelInvoke(nameof(MPExtraRegeneration));

        MP++;
        //最大値を超えたら補正する
        if (MP > maxMP) MP = maxMP;
    }

    /// <summary>
    /// MPを回復する(追加効果による回復用)
    /// </summary>
    void MPExtraRegeneration() {
        //攻撃してから短い間を置く。
        if (Time.time <= attackStartTime + 0.2f) return;

        MP++;
        //最大値を超えたら補正する
        if (MP > maxMP) MP = maxMP;
    }

    /// <summary>
    /// ステータスのインポート
    /// </summary>
    public void StatusInport() {
        if (inputStatus == null) {
            DefaultStatusInport();
            return;
        }

        runtimeStatus = inputStatus;
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
        equippedPassives[0].PassiveSetting(player);
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
        weaponController_main.SetWeaponDataInit(mainWeapon);
        weaponController_sub.SetWeaponData(subWeapon);
    }

    /// <summary>
    /// ステータスのインポート
    /// </summary>
    public void StatusInport(GeneralCharacterStatus _inputStatus) {
        if (_inputStatus == null) {
            DefaultStatusInport();
            return;
        }

        runtimeStatus = _inputStatus;
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
        equippedPassives[0].PassiveSetting(player);
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
        weaponController_main.SetWeaponDataInit(mainWeapon);
        weaponController_sub.SetWeaponData(subWeapon);
    }

    /// <summary>
    /// StatusInportでnullが発生した時にデフォルトの値で初期化する
    /// </summary>
    protected void DefaultStatusInport() {
#if UNITY_EDITOR
        Debug.LogWarning("InputStatusに値が入っていなかったため、デフォルト値で初期化を行いました。");
#endif
        maxHP = PlayerConst.DEFAULT_MAXHP;
        HP = maxHP;
        attack = PlayerConst.DEFAULT_ATTACK;
        moveSpeed = PlayerConst.DEFAULT_MOVESPEED;
    }

    /// <summary>
    /// 初期値を保存する
    /// </summary>
    protected void InDefaultStatus() {
        player.defaultAttack = attack;
        player.defaultMoveSpeed = moveSpeed;
    }

    /// <summary>
    /// プレイヤー状態を初期化する関数
    /// </summary>
    public void StateInitalize() {
        //HPやフラグ関連などの基礎的な初期化
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
        //gameObject.GetComponentInChildren<PlayerCamera>().ExitDeathView();
        //MaxMPが0でなければ最大値で初期化
        if (maxMP != 0) MP = maxMP; 
        //弾倉が0でなければ最大値で初期化
        if (weaponController_main.weaponData.maxAmmo != 0)
            weaponController_main.weaponData.ammo = weaponController_main.weaponData.maxAmmo;
        //Passive関連の初期化
        equippedPassives[0].coolTime = 0;
        equippedPassives[0].isPassiveActive = false;
        //Skill関連の初期化
        equippedSkills[0].isSkillUse = false;        
    }

    /// <summary>
    /// トリガー変数のリセット
    /// </summary>
    public void ResetTrigger() {
        isAttackTrigger = false;
        isDeadTrigger = false;
    }

    /// <summary>
    /// UI用のHP更新関数(第一引数は消せないため無名変数を使用。)
    /// </summary>
    public void ChangeHP(int _, int _newValue) {
        if (!isLocalPlayer && !isClient) return; // 自分のプレイヤーでなければUI更新しない
        if (player.localUI != null) player.localUI.ChangeHPUI(maxHP, _newValue);
        else {
#if UNITY_EDITOR
            Debug.LogWarning("UIが存在しないため、HP更新処理をスキップしました。");
#endif
        }
    }
    /// <summary>
    /// UI用のMP更新関数(第一引数は消せないため無名変数を使用。)
    /// </summary>
    public void ChangeMP(int _, int _newValue) {
        if (!isLocalPlayer && !isClient) return; // 自分のプレイヤーでなければUI更新しない
        if (player.localUI != null) player.localUI.ChangeMPUI(maxMP, _newValue);
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
            player.localUI.StartRotateReloadIcon();
    }

    public void StartUseSkill() {
        if (isCanSkill) {
            equippedSkills[0].Activate(player);
            isCanSkill = false;
            //CT計測時間をリセット
            skillAfterTime = 0;
        }       
    }

    /// <summary>
    /// スキルとパッシブの制御用関数(死亡中は呼ばないでください。)
    /// </summary>
    public void AbilityControl() {
        //パッシブを呼ぶ(パッシブの関数内で判定、発動を制御。)
        equippedPassives[0].PassiveReflection(player);
        //スキル更新関数を呼ぶ(中身を未定義の場合は何もしない)
        equippedSkills[0].SkillEffectUpdate(player);

        //スキル使用不可中、スキルクールタイム中かつスキルがインポートされていれば時間を計測
        if (!isCanSkill
        && skillAfterTime <= equippedSkills[0].cooldown 
            && equippedSkills[0] != null)
            skillAfterTime += Time.deltaTime;
        //スキルクールタイムを過ぎていたら丁度になるよう補正
        else if (skillAfterTime > equippedSkills[0].cooldown)
            skillAfterTime = equippedSkills[0].cooldown;
        //スキルがインポートされていて、かつ規定CTが経過していればスキルを使用可能にする
        var Skill = equippedSkills[0];
        if (!isCanSkill && Skill != null && skillAfterTime >= Skill.cooldown) {
            isCanSkill = true;
            //経過時間を固定
            skillAfterTime = Skill.cooldown;
        }        
    }
}
