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

        //バフデバフの処理
        for(int paramIndex = (int)ParamaterType.max -1 ; 0 < paramIndex ; paramIndex--) {
            Debug.Log(temporaryBuffs.Count);
            Debug.Log(temporaryBuffs[paramIndex].Count);

            //nullだったり要素がなかったりしたらとばす
            if(temporaryBuffs[paramIndex].Count == 0) continue;

            //倍率の累計
            float influxValue = 1.0f;
            //各バフの反映と時間経過処理
            for (int buffIndex = 0 ; buffIndex < temporaryBuffs[paramIndex].Count ; buffIndex++) {
                //倍率の累計に掛けていく
                influxValue *= temporaryBuffs[paramIndex][buffIndex].amount;
                //反映したら効果時間を減らす
                temporaryBuffs[paramIndex][buffIndex].duration -= Time.deltaTime;

                //効果時間が切れたら
                if(temporaryBuffs[paramIndex][buffIndex].duration <= 0.0f) {
                    //該当する要素を削除。
                    temporaryBuffs[paramIndex].RemoveAt(buffIndex);
                }                   
            }

            //値を反映(switchで分岐)
            switch((ParamaterType)paramIndex) {
                case ParamaterType.Attack:
                    parameter.attack *= influxValue;
                    break;
                case ParamaterType.moveSpeed:
                    parameter.moveSpeed *= (int)influxValue;
                    break;
                case ParamaterType.damageRate:
                    parameter.DamageRatio *= (int)influxValue;
                    break;
                default:
#if UNITY_EDITOR
                    Debug.LogWarning("実行されないべきであるコードが実行されました。\n発生：GeneralCharacter.Update");
#endif
                    break;
            }
        }
    }

    [ClientRpc]
    public override void Initalize() {
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
}
