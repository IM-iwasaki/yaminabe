using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;
using static TeamData;
using UnityEngine.SocialPlatforms;

public class CharacterActions : NetworkBehaviour {
    private CharacterBase core;
    private CharacterParameter param;
    private CharacterInput input;
    private Rigidbody rb;
    private Collider useCollider;
    private string useTag;
    private CameraChangeController cameraManager;
    private CharacterBase characterBase;

    //移動中か
    public bool isMoving { get; private set; } = false;
    //移動を要求する方向
    //protected Vector2 MoveInput;
    //実際に移動する方向
    public Vector3 moveDirection { get; private set; }

    //アイテムを拾える状態か
    public bool isCanPickup { get; private set; }
    //インタラクトできる状態か
    public bool isCanInteruct { get; private set; }   
    //スキルを使用できるか
    public bool isCanSkill { get; private set; }

    // キャラクター選択管理
    private CharacterSelectManager characterSelectManager;

    // ガチャ管理
    private GachaSystem gachaSystem;

    private HudManager hud;



    private void Update() {
        // ローカルプレイヤー以外は処理しない
        if (!isLocalPlayer) return;

        // キャラ選択中 or ガチャ中なら全操作ブロック
        if ((characterSelectManager != null && characterSelectManager.IsCharacterSelectActive()) ||
            (gachaSystem != null && gachaSystem.IsGachaActive())
        ) {
            // HUD（レティクル）非表示
            if(hud != null) hud.SetReticleVisible(false);

            // 移動を完全停止
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            isMoving = false;
            return;
        }
        if(hud != null) hud.SetReticleVisible(true);

        JumpControl();

        //攻撃入力がある間攻撃関数を呼ぶ(間隔の制御はMainWeaponControllerに一任)
        if (input.AttackPressed) StartAttack();

        HandleSkill();
        HandleInteract();

        MoveControl();
        param.GroundCheck(core.parameter.footPoint.position);
        
        AbilityControl();
    }

    public void Initialize(CharacterBase core) {
        this.core = core;
        param = core.parameter;
        input = core.input;
        rb = core.GetComponent<Rigidbody>();

        // シーンに1つだけ存在する想定
        characterSelectManager = FindObjectOfType<CharacterSelectManager>();
        gachaSystem = FindObjectOfType<GachaSystem>();
        hud = FindObjectOfType<HudManager>();

        isCanPickup = false;
        isCanInteruct = false;
        isCanSkill = false;

        cameraManager = FindObjectOfType<CameraChangeController>();
        //一定間隔でMPを回復する
        InvokeRepeating(nameof(MPRegeneration), 0.0f,0.1f);
    }

    /// <summary>
    /// MPを回復する
    /// </summary>
    void MPRegeneration() {
        //攻撃してから短い間を置く。
        if (Time.time <= param.attackStartTime + 0.2f) return;
        //基本回復量は1
        int MPHealValue = 1;
        //移動していないときは回復量+2
        if (!isMoving) MPHealValue += 2;

        //MP回復
        param.MP += MPHealValue;
        //最大値を超えたら補正する
        if (param.MP > param.maxMP) param.MP = param.maxMP;
    }


    private void MoveControl() {
        //移動入力が行われている間は移動中フラグを立てる
        if (input.MoveInput != Vector2.zero) isMoving = true;
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
        moveDirection = forward * input.MoveInput.y + right * input.MoveInput.x;

        // カメラの向いている方向をプレイヤーの正面に
        Vector3 aimForward = forward; // 水平面だけを考慮
        if (aimForward != Vector3.zero) {
            Quaternion targetRot = Quaternion.LookRotation(aimForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, PlayerConst.TURN_SPEED * Time.deltaTime);
        }

        // 空中か地上で挙動を分ける
        Vector3 velocity = rb.velocity;
        Vector3 targetVelocity = new(moveDirection.x * param.moveSpeed, velocity.y, moveDirection.z * param.moveSpeed);

        //地面に立っていたら通常通り
        if (param.IsGrounded) {
            rb.velocity = targetVelocity;
        }
        else {
            // 空中では地上速度に向けてゆるやかに補間（慣性を残す）
            rb.velocity = Vector3.Lerp(velocity, targetVelocity, Time.deltaTime * 2f);
        }
    }    

    private void JumpControl() {
        // ジャンプ判定
        if (input.isJumpPressed && param.IsGrounded) {
            // 現在の速度をリセットしてから上方向に力を加える
            Vector3 velocity = rb.velocity;
            velocity.y = 0f;
            rb.velocity = velocity;
            rb.AddForce(Vector3.up * PlayerConst.JUMP_FORCE, ForceMode.Impulse);
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
        }       
    }

    /// <summary>
    /// 攻撃関数
    /// </summary>
    virtual public void StartAttack() {
        if (core.weaponController_main == null) return;

        if (HostUI.isVisibleUI == true) return;
        
        //最後に攻撃した時間を記録
        param.AttackStartTimeRecord();
        // 武器が攻撃可能かチェックしてサーバー命令を送る(CmdRequestAttack武器種ごとの分岐も側で)
        Vector3 shootDir = core.parameter.GetShootDirection();

        //追加：岩﨑
        //var passive = characterBase.GetComponent<Passive_Hacker>();

        // デスマッチのみ
        //if (passive != null &&
        //    RuleManager.Instance.currentRule == GameRuleType.DeathMatch &&
        //    Random.value <= 0.25f) {
        //    core.weaponController_main.CmdRequestExtraAttack(shootDir);
        //    // 弾を減らさない
        //    return;
        //}

        core.weaponController_main.CmdRequestAttack(shootDir);
    }

    private void HandleSkill()
    {
        if (!input.SkillTriggered) return;
        input.SkillTriggered = false;

        StartUseSkill();
    }

    protected void StartUseSkill() {
        if (isCanSkill) {
            param.equippedSkills[0].Activate(core);
            isCanSkill = false;
            //CT計測時間をリセット
            param.skillAfterTime = 0;
        }
    }

    private void HandleInteract() {
        if (cameraManager != null && cameraManager.IsCameraTransitioning())
            return;
        if (!input.InteractTriggered) return;
        input.InteractTriggered = false;

        if (isCanPickup) {
            ItemBase item = useCollider.GetComponent<ItemBase>();
            item.Use(gameObject);
            return;
        }
        if (isCanInteruct) {
            if (useTag == "SelectCharacterObject") {
                CharacterSelectManager select = useCollider.GetComponentInParent<CharacterSelectManager>();
                select.StartCharacterSelect(gameObject);
                core.localUI.OffLocalUIObject();
                return;
            }
            if (useTag == "Gacha") {
                GachaSystem gacha = useCollider.GetComponentInParent<GachaSystem>();
                gacha.StartGachaSelect(gameObject);
                core.localUI.OffLocalUIObject();
                return;
            }

        }
    }

    /// <summary>
    /// Abstruct : スキルとパッシブの制御用関数(死亡中は呼ばないでください。)
    /// </summary>
    private void AbilityControl() {
        //パッシブを呼ぶ(パッシブの関数内で判定、発動を制御。)
        param.equippedPassives[0].PassiveReflection(core);
        //スキル更新関数を呼ぶ(中身を未定義の場合は何もしない)
        param.equippedSkills[0].SkillEffectUpdate(core);

        //スキル使用不可中、スキルクールタイム中かつスキルがインポートされていれば時間を計測
        if (!isCanSkill && param.skillAfterTime <= param.equippedSkills[0].cooldown && param.equippedSkills[0] != null)
            param.skillAfterTime += Time.deltaTime;
        //スキルクールタイムを過ぎていたら丁度になるよう補正
        else if (param.skillAfterTime > param.equippedSkills[0].cooldown) param.skillAfterTime = param.equippedSkills[0].cooldown;
        //スキルがインポートされていて、かつ規定CTが経過していればスキルを使用可能にする
        var Skill = param.equippedSkills[0];
        if (!isCanSkill && Skill != null && param.skillAfterTime >= Skill.cooldown) {
            isCanSkill = true;
            //経過時間を固定
            param.skillAfterTime = Skill.cooldown;
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
                core.localUI.OnChangeInteractUI();
                break;
            case "SelectCharacterObject":
                // フラグを立てる
                isCanInteruct = true;
                useCollider = _collider;
                useTag = "SelectCharacterObject";
                core.localUI.OnChangeInteractUI();
                break;
            case "Gacha":
                isCanInteruct = true;
                useCollider = _collider;
                useTag = "Gacha";
                core.localUI.OnChangeInteractUI();
                break;
            case "RedTeam":
                core.CmdJoinTeam(netIdentity, TeamColor.Red);
                break;
            case "BlueTeam":
                core.CmdJoinTeam(netIdentity, TeamColor.Blue);
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
                core.localUI.OffChangeInteractUI();
                break;
            case "SelectCharacterObject":
                // フラグを下ろす
                isCanInteruct = false;
                useCollider = null;
                useTag = null;
                core.localUI.OffChangeInteractUI();
                break;
            case "Gacha":
                isCanInteruct = false;
                useCollider = null;
                useTag = null;
                core.localUI.OffChangeInteractUI();
                break;
            case "RedTeam":
                //抜けたときは処理しない。何か処理があったら追加。
                //core.CmdJoinTeam(netIdentity, TeamColor.Red);
                break;
            case "BlueTeam":
                //抜けたときは処理しない。何か処理があったら追加。
                //core.CmdJoinTeam(netIdentity, TeamColor.Blue);
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
        core.localUI.OffChangeInteractUI();
    }
}