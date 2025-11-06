using UnityEngine;
using System.Linq;
using Mirror;

//
//  @file   Second_CharacterClass
//
class GeneralCharacter : CharacterBase {

    #region ～キャラクターデータ管理変数～

    [Header("インポートするステータス")]
    [SerializeField]CharacterStatus InputStatus;
    //CharacterStatusをキャッシュ(ScriptableObjectを書き換えないための安全策)
    private CharacterStatus RunTimeStatus;
    [Header("使用するスキル")]
    private SkillBase[] EquippedSkills;
    [Header("使用するパッシブ")]
    private PassiveBase[] EquippedPassives;

    #endregion

    #region ～職業限定ステータス変数～

    //魔法職のみ：攻撃時に消費。時間経過で徐々に回復(攻撃中は回復しない)。レベルアップで最大MP(もしくは回復速度？)が上昇。
    protected int MP { get; private set; }
    protected int MaxMP { get; private set; }
    //間接職のみ：攻撃するたびに弾薬を消費、空になるとリロードが必要。レベルアップで最大弾容量が増加。
    protected int Magazine { get; private set; }
    protected int MaxMagazine { get; private set; }

    #endregion

    public override void StatusInport(CharacterStatus _inport = null) {
        if (_inport == null) {
            DefaultStatusInport();
            return;
        }

        RunTimeStatus = _inport;
        maxHP = RunTimeStatus.MaxHP;
        HP = maxHP;
        attack = RunTimeStatus.Attack;
        moveSpeed = RunTimeStatus.MoveSpeed;
        Debug.Log("MeleeCharacter.cs : StatusInportを実行しました。\nMaxHP:" + maxHP + " attack:" + attack + " moveSpeed:" + moveSpeed);
        EquippedSkills = RunTimeStatus.Skills;
        /* xxx.Where() <= nullでないか確認する。 xxx.Select() <= 指定した変数を取り出す。 ※using System.Linq が必要。 */
        Debug.Log("MeleeCharacter.cs : スキルのインポートを行いました。\nインポートしたスキル: " + string.Join(", ", EquippedSkills.Where(i => i != null).Select(i => i.SkillName)));
        EquippedPassives = RunTimeStatus.Passives;
        Debug.Log("MeleeCharacter.cs : パッシブのインポートを行いました。\nインポートしたパッシブ: " + string.Join(", ", EquippedPassives.Where(i => i != null).Select(i => i.PassiveName)));
        // パッシブの初期セットアップ
        EquippedPassives[0].PassiveSetting(this);
        //  デフォルトステータスを代入
        InDefaultStatus();
    }

    protected override void StartUseSkill() {
        if (isCanSkill) {
            EquippedSkills[0].Activate(this);
            isCanSkill = false;
        }       
    }

    public void Reload() {
        Magazine = MaxMagazine;
    }

    protected new void Awake() {
        base.Awake();
        StatusInport(InputStatus);
        Initalize();
    }

    void Update() {
        if(!isLocalPlayer) return;  
        //攻撃トリガーが立っていたら下す
        isAttackTrigger = false;
        
        //TODO: MP管理系の処理がない。
        //TODO: リロード処理を呼ぶところがないかも。(キーバインドは作った。)
        //TODO: 死亡中でもアクションが起こせてしまう。

        RespawnControl();
        //HPが0以下になったとき死亡していなかったら死亡処理を行う
        if (HP <= 0 && !IsDead) Dead();               
        //死んでいたら以降の処理は行わない。
        if (IsDead) return;

        MoveControl();
        JumpControl();       
        AbilityControl();
    }

    public override void Respawn() {
        base.Respawn();
        //パッシブのセットアップ
        EquippedPassives[0].PassiveSetting(this);
    }

    protected override void AbilityControl() {
        //パッシブを呼ぶ(パッシブの関数内で判定、発動を制御。)
        EquippedPassives[0].PassiveReflection(this);
        //スキル更新関数を呼ぶ(中身を未定義の場合は何もしない)
        EquippedSkills[0].SkillEffectUpdate(this);

        //スキル使用不可中、かつスキルがインポートされていれば時間を計測
        if (!isCanSkill && EquippedSkills[0] != null)
            SkillAfterTime += Time.deltaTime;
        //スキルがインポートされていて、かつ規定CTが経過していればスキルを使用可能にする
        var Skill = EquippedSkills[0];
        if (!isCanSkill && Skill != null && SkillAfterTime >= Skill.Cooldown) {
            isCanSkill = true;
            //経過時間をリセット
            SkillAfterTime = 0.0f;
            //デバッグログを出す
        }        
    }

    public override void Initalize() {
        //HPやフラグ関連などの基礎的な初期化
        base.Initalize();
        //MaxMPが0でなければ最大値で初期化
        if (MaxMP != 0) MP = MaxMP; 
        //MaxMagazineが0でなければ最大値で初期化
        if (MaxMagazine != 0) Magazine = MaxMagazine;
        //Passive関連の初期化
        EquippedPassives[0].CoolTime = 0;
        EquippedPassives[0].IsPassiveActive = false;
        //Skill関連の初期化
        EquippedSkills[0].IsSkillUse = false;
    }
}
