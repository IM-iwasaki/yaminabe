using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

//
//  @file   Second_CharacterClass
//
class MeleeCharacter : CharacterBase {
    [Tooltip("インポートするステータスのScriptableObject")]
    [SerializeField]CharacterStatus InputStatus;
    //CharacterStatusをキャッシュ(ScriptableObjectを書き換えないための安全策)
    private CharacterStatus RunTimeStatus;
    [Tooltip("使用するスキル")]
    private SkillBase[] EquippedSkills;
    [Tooltip("使用するパッシブ")]
    private PassiveBase[] EquippedPassives;

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

    protected override void StartUseSkill() {
        if (IsCanSkill) {
            EquippedSkills[0].Activate(this);
            IsCanSkill = false;
        }
        
    }

    // Start is called before the first frame update
    protected new void Awake() {
        base.Awake();
        StatusInport(InputStatus);
    }

    // Update is called once per frame
    void Update() {
        if(!isLocalPlayer) return;

        //スキル使用不可中、かつスキルがインポートされていれば時間を計測
        if (!IsCanSkill && EquippedSkills[0] != null) SkillAfterTime += Time.deltaTime;
        //スキルがインポートされていて、かつ規定CTが経過していればスキルを使用可能にする
        if (!IsCanSkill && SkillAfterTime >= EquippedSkills[0]?.Cooldown) {
            IsCanSkill = true;
            //経過時間をリセット
            SkillAfterTime = 0.0f;
            //デバッグログを出す
            Debug.Log("スキルが使用可能になりました。");
        }

        MoveControl();
        JumpControl();
        EquippedPassives[0].PassiveReflection(this);
    }   

    
}
