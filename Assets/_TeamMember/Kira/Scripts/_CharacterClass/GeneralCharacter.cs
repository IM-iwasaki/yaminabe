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
    }   

    public override void OnStartClient() {
        base.OnStartClient();
        localUI.Initialize();


        if (!isLocalPlayer) return; // 自分だけ表示
        SkillBase skill = parameter.equippedSkills[0];
        PassiveBase passive = parameter.equippedPassives[0];

        SkillDisplayer.Instance.SetSkillUI(
        skill.skillName, skill.skillDescription,
        passive.passiveName, passive.passiveDescription
        );
    }

    void Update() {
        if(!isLocalPlayer) return;  //自分だけ処理する         

        //RespawnControl();    
               
        //死んでいたら以降の処理は行わない。
        //if (isDead) return;

        //MoveControl();
        //JumpControl();       
        //AbilityControl();
    }

    public override void Initalize() {
        //HPやフラグ関連などの基礎的な初期化
        base.Initalize();
        //MaxMPが0でなければ最大値で初期化
        //if (maxMP != 0) MP = maxMP; 
        //弾倉が0でなければ最大値で初期化
        if (weaponController_main.weaponData.maxAmmo != 0)
            weaponController_main.weaponData.ammo = weaponController_main.weaponData.maxAmmo;
    }   

    public override void Respawn() {
        base.Respawn();
        //パッシブのセットアップ
        parameter.equippedPassives[0].PassiveSetting();
    }
}
