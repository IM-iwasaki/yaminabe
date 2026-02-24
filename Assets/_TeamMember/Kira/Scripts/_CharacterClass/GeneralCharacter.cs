//
//  @file   Second_CharacterClass
//
using Mirror;
using UnityEngine;

public class GeneralCharacter : CharacterBase {

    protected new void Awake() {
        base.Awake();
        //Initalize();        
    }

    public override void OnStartLocalPlayer() {
        base.OnStartLocalPlayer();
        if (!isLocalPlayer) return; // 自分だけ表示

        localUI.Initialize();
        localUI.LocalUIChanged();

        SkillBase skill = parameter.equippedSkills[0];
        PassiveBase passive = parameter.equippedPassives[0];

        SkillDisplayer.Instance.SetSkillUI(
        skill.skillName, skill.skillDescription,
        passive.passiveName, passive.passiveDescription
        );
    }

    public override void OnStartClient() {
        base.OnStartClient();

    }

    void Update() {
        if (!isLocalPlayer) return;  //自分だけ処理する         

        parameter.UpdateNearbyAlly(allyCheckRadius, allyLayer);
        //RespawnControl();
        
        //BuffUpdate();
    }

    [ClientRpc]
    public void Initalize() {
        //HPやフラグ関連などの基礎的な初期化
        //base.Initalize();
        //MaxMPが0でなければ最大値で初期化
        if (parameter.maxMP != 0) parameter.MP = parameter.maxMP;
        //弾倉が0でなければ最大値で初期化
        if (weaponController_main.weaponData.maxAmmo != 0)
            weaponController_main.weaponData.ammo = weaponController_main.weaponData.maxAmmo;
    }

    public override void Respawn() {
        base.Respawn();
        if (!isLocalPlayer) return;
        //パッシブのセットアップ
        parameter.equippedPassives[0].PassiveSetting();
    }

    // 追加 マツオ : クライアントステータスリセット用
    [TargetRpc]
    public void TargetResetStatus(NetworkConnection target) {
        parameter.StatusInport(parameter.runtimeStatus);
    }
}
