using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//
//  @file   Second_CharacterClass
//
class GunnerCharacter : CharacterBase {
    [Tooltip("インポートするステータスのScriptableObject")]
    [SerializeField] CharacterStatus InputStatus;
    //CharacterStatusをキャッシュ(ScriptableObjectを書き換えないための安全策)
    private CharacterStatus RunTimeStatus;
    [Tooltip("使用するスキル")]
    private SkillBase[] EquippedSkills;
    [Tooltip("使用するパッシブ")]
    private PassiveBase[] EquippedPassives;

    //間接職のみ：攻撃するたびに弾薬を消費、空になるとリロードが必要。レベルアップで最大弾容量が増加。
    protected int Magazine { get; private set; }
    protected int MaxMagazine { get; private set; }

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
    }

    protected override void StartAttack(PlayerConst.AttackType _type = PlayerConst.AttackType.Main) {
        if (Magazine <= 0) {
            Debug.Log("弾切れ。リロードが必要です。");
            return;
        }

        if (weaponController != null) {
            Magazine--;
        }
    }
    public void Reload() {
        Magazine = MaxMagazine;
    }

    protected override void StartUseSkill() {
        EquippedSkills[0].Activate(this);
    }

    // Start is called before the first frame update
    protected new void Awake() {
        base.Awake();
        MaxMagazine = 30;
        Magazine = MaxMagazine;
    }

    // Update is called once per frame
    void Update() {
        MoveControl();
    }

}
