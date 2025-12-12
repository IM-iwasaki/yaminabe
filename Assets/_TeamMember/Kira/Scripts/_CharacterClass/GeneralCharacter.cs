using UnityEngine;
using System.Linq;
using Mirror;

//
//  @file   Second_CharacterClass
//
public class GeneralCharacter : CharacterBase {

    protected new void Awake() {
        base.Awake();
        Initalize();
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
        if(MoveInput == Vector2.zero)InvokeRepeating(nameof(MPExtraRegeneration), 0.5f,0.4f);
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

    public override void OnStartClient() {
        base.OnStartClient();
        localUI.Initialize();


        if (!isLocalPlayer) return; // 自分だけ表示
        SkillBase skill = equippedSkills[0];
        PassiveBase passive = equippedPassives[0];

        SkillDisplayer.Instance.SetSkillUI(
        skill.skillName, skill.skillDescription,
        passive.passiveName, passive.passiveDescription
        );
    }

    void Update() {
        if(!isLocalPlayer) return;  //自分だけ処理する         

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
        //if (maxMP != 0) MP = maxMP; 
        //弾倉が0でなければ最大値で初期化
        if (weaponController_main.weaponData.maxAmmo != 0)
            weaponController_main.weaponData.ammo = weaponController_main.weaponData.maxAmmo;
        //Passive関連の初期化
        equippedPassives[0].coolTime = 0;
        equippedPassives[0].isPassiveActive = false;
        //Skill関連の初期化
        equippedSkills[0].isSkillUse = false;
    }

    

    protected override void StartUseSkill() {
        if (isCanSkill) {
            equippedSkills[0].Activate(this);
            isCanSkill = false;
            //CT計測時間をリセット
            skillAfterTime = 0;
        }       
    }
    public override void Respawn() {
        base.Respawn();
        //パッシブのセットアップ
        equippedPassives[0].PassiveSetting(this);
    }
}
