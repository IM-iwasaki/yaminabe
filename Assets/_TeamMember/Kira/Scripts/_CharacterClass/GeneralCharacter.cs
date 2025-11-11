using UnityEngine;
using System.Linq;
using Mirror;

//
//  @file   Second_CharacterClass
//
class GeneralCharacter : CharacterBase {

    #region ～キャラクターデータ管理変数～

    [Header("インポートするステータス")]
    [SerializeField]CharacterStatus inputStatus;
    //CharacterStatusをキャッシュ(ScriptableObjectを書き換えないための安全策)
    private CharacterStatus runtimeStatus;
    public SkillBase[] equippedSkills{ get; private set; }
    public PassiveBase[] equippedPassives{ get; private set; }

    #endregion

    #region ～職業限定ステータス変数～

    //魔法職のみ：攻撃時に消費。時間経過で徐々に回復(攻撃中は回復しない)。レベルアップで最大MP(もしくは回復速度？)が上昇。
    protected int MP { get; private set; }
    protected int maxMP { get; private set; }
    //間接職のみ：攻撃するたびに弾薬を消費、空になるとリロードが必要。レベルアップで最大弾容量が増加。
    protected int magazine { get; private set; }
    protected int maxMagazine { get; private set; }

    #endregion  

    protected new void Awake() {
        base.Awake();
        StatusInport(inputStatus);
        Initalize();
    }

    void Update() {
        if(!isLocalPlayer) return;  
        //トリガーリセット関数の呼び出し
        ResetTrigger();
        
        //TODO: MP管理系の処理がない。
        //TODO: リロード処理を呼ぶところがないかも。(キーバインドは作った。)

        RespawnControl();           
        //死んでいたら以降の処理は行わない。
        if (isDead) return;

        MoveControl();
        JumpControl();       
        AbilityControl();
    }

    public override void Initalize() {
        //HPやフラグ関連などの基礎的な初期化
        base.Initalize();
        //MaxMPが0でなければ最大値で初期化
        if (maxMP != 0) MP = maxMP; 
        //MaxMagazineが0でなければ最大値で初期化
        if (maxMagazine != 0) magazine = maxMagazine;
        //Passive関連の初期化
        equippedPassives[0].CoolTime = 0;
        equippedPassives[0].IsPassiveActive = false;
        //Skill関連の初期化
        equippedSkills[0].isSkillUse = false;
    }

    public override void StatusInport(CharacterStatus _inport = null) {
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
        //  デフォルトステータスを代入
        InDefaultStatus();
    }

    protected override void StartUseSkill() {
        if (isCanSkill) {
            equippedSkills[0].Activate(this);
            isCanSkill = false;
        }       
    }

    public void Reload() {
        magazine = maxMagazine;
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
