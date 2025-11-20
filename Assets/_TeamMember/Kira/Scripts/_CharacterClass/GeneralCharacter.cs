using UnityEngine;
using System.Linq;
using Mirror;

//
//  @file   Second_CharacterClass
//
public class GeneralCharacter : CharacterBase {

    #region ～キャラクターデータ管理変数～

    [Header("インポートするステータス")]
    [SerializeField]GeneralCharacterStatus inputStatus;
    //CharacterStatusをキャッシュ(ScriptableObjectを書き換えないための安全策)
    private GeneralCharacterStatus runtimeStatus;
    public SkillBase[] equippedSkills{ get; private set; }
    public PassiveBase[] equippedPassives{ get; private set; }

    #endregion

    protected new void Awake() {
        base.Awake();
        StatusInport(inputStatus);
        Initalize();
    }

    void Update() {
        if(!isLocalPlayer) return;          
        
        //TODO: MP管理系の処理がない。

        //RespawnControl();    
               
        //死んでいたら以降の処理は行わない。
        if (isDead) return;

        //攻撃入力がある間攻撃関数を呼ぶ(間隔の制御はMainWeaponControllerに一任)
        if (isAttackPressed) StartAttack();


        MoveControl();
        JumpControl();       
        AbilityControl();
        //トリガーリセット関数の呼び出し
        ResetTrigger();
    }

    public override void Initalize() {
        //HPやフラグ関連などの基礎的な初期化
        base.Initalize();
        //MaxMPが0でなければ最大値で初期化
        if (maxMP != 0) MP = maxMP; 
        //弾倉が0でなければ最大値で初期化
        if (weaponController_main.weaponData.maxAmmo != 0)
            weaponController_main.weaponData.ammo = weaponController_main.weaponData.maxAmmo;
        //Passive関連の初期化
        equippedPassives[0].CoolTime = 0;
        equippedPassives[0].IsPassiveActive = false;
        //Skill関連の初期化
        equippedSkills[0].isSkillUse = false;
    }

    public override void StatusInport(GeneralCharacterStatus _inport = null) {
        if (_inport == null) {
            DefaultStatusInport();
            return;
        }

        runtimeStatus = _inport;
        maxHP = runtimeStatus.maxHP;
        HP = maxHP;
        attack = runtimeStatus.attack;
        moveSpeed = runtimeStatus.moveSpeed;
        equippedSkills = runtimeStatus.skills;
        equippedPassives = runtimeStatus.passives;
        /* xxx.Where() <= nullでないか確認する。 xxx.Select() <= 指定した変数を取り出す。 ※using System.Linq が必要。 */        
        Debug.Log("ステータス、パッシブ、スキルのインポートを行いました。\n" +
            "インポートしたステータス... キャラクター:" + runtimeStatus.displayName + "　maxHP:" + maxHP + "　attack:" + attack + "　moveSpeed:" + moveSpeed + "\n" +
            "インポートしたパッシブ..." + string.Join(", ", equippedPassives.Where(i => i != null).Select(i => i.PassiveName)) +
            "　：　インポートしたスキル..." + string.Join(", ", equippedSkills.Where(i => i != null).Select(i => i.skillName)));
        // パッシブの初期セットアップ
        equippedPassives[0].PassiveSetting(this);
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
        weaponController_main.SetWeaponData(mainWeapon);
        weaponController_sub.SetWeaponData(subWeapon);

    }

    protected override void StartUseSkill() {
        if (isCanSkill) {
            equippedSkills[0].Activate(this);
            isCanSkill = false;
        }       
    }
    public override void Respawn() {
        base.Respawn();
        //パッシブのセットアップ
        equippedPassives[0].PassiveSetting(this);
    }

    protected override void AbilityControl() {
        //パッシブを呼ぶ(パッシブの関数内で判定、発動を制御。)
        equippedPassives[0].PassiveReflection(this);
        //スキル更新関数を呼ぶ(中身を未定義の場合は何もしない)
        equippedSkills[0].SkillEffectUpdate(this);

        //スキル使用不可中、スキルクールタイム中かつスキルがインポートされていれば時間を計測
        if (!isCanSkill && skillAfterTime <= equippedSkills[0].cooldown && equippedSkills[0] != null)
            skillAfterTime += Time.deltaTime;
        //スキルクールタイムを過ぎていたら丁度になるよう補正
        else if (skillAfterTime > equippedSkills[0].cooldown) skillAfterTime = equippedSkills[0].cooldown;
        //スキルがインポートされていて、かつ規定CTが経過していればスキルを使用可能にする
        var Skill = equippedSkills[0];
        if (!isCanSkill && Skill != null && skillAfterTime >= Skill.cooldown) {
            isCanSkill = true;
            //経過時間をリセット
            skillAfterTime = 0.0f;
            //デバッグログを出す
        }        
    }
}
