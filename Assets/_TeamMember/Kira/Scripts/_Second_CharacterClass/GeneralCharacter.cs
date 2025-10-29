using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

//
//  @file   Second_CharacterClass
//
class GeneralCharacter : CharacterBase {

    #region ～キャラクターデータ管理変数～

    [Tooltip("インポートするステータスのScriptableObject")]
    [SerializeField]CharacterStatus InputStatus;
    //CharacterStatusをキャッシュ(ScriptableObjectを書き換えないための安全策)
    private CharacterStatus RunTimeStatus;
    [Tooltip("使用するスキル")]
    private SkillBase[] EquippedSkills;
    [Tooltip("使用するパッシブ")]
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
        MaxHP = RunTimeStatus.MaxHP;
        HP = MaxHP;
        Attack = RunTimeStatus.Attack;
        MoveSpeed = RunTimeStatus.MoveSpeed;
        Debug.Log("MeleeCharacter.cs : StatusInportを実行しました。\nMaxHP:" + MaxHP + " Attack:" + Attack + " MoveSpeed:" + MoveSpeed);
        EquippedSkills = RunTimeStatus.Skills;
        /* xxx.Where() <= nullでないか確認する。 xxx.Select() <= 指定した変数を取り出す。 ※using System.Linq が必要。 */
        Debug.Log("MeleeCharacter.cs : スキルのインポートを行いました。\nインポートしたスキル: " + string.Join(", ", EquippedSkills.Where(i => i != null).Select(i => i.SkillName)));
        EquippedPassives = RunTimeStatus.Passives;
        Debug.Log("MeleeCharacter.cs : パッシブのインポートを行いました。\nインポートしたパッシブ: " + string.Join(", ", EquippedPassives.Where(i => i != null).Select(i => i.PassiveName)));
        // パッシブの初期セットアップ
        EquippedPassives[0].PassiveSetting(this);
    }

    protected override void StartUseSkill() {
        if (IsCanSkill) {
            EquippedSkills[0].Activate(this);
            IsCanSkill = false;
        }
        
    }

    public void Reload() {
        Magazine = MaxMagazine;
    }

    protected new void Awake() {
        base.Awake();
        StatusInport(InputStatus);
    }

    void Update() {
        if(!isLocalPlayer) return;   
        
        //TODO: MP管理系の処理がない。
        //TODO: リロード処理を呼ぶところがないかも。

        MoveControl();
        JumpControl();
        RespownControl();
        AbilityControl();
    }

    protected override void RespownControl() {
        base.RespownControl();
        EquippedPassives[0].PassiveSetting(this);
    }

    protected override void AbilityControl() {
        //パッシブを呼ぶ(パッシブの関数内で判定、発動を制御。)
        EquippedPassives[0].PassiveReflection(this);

        //スキル使用不可中、かつスキルがインポートされていれば時間を計測
        if (!IsCanSkill && EquippedSkills[0] != null)
            SkillAfterTime += Time.deltaTime;
        //スキルがインポートされていて、かつ規定CTが経過していればスキルを使用可能にする
        var Skill = EquippedSkills[0];
        if (!IsCanSkill && Skill != null && SkillAfterTime >= Skill.Cooldown) {
            IsCanSkill = true;
            //経過時間をリセット
            SkillAfterTime = 0.0f;
            //デバッグログを出す
            Debug.Log("スキルが使用可能になりました。");
        }        
    }
}
