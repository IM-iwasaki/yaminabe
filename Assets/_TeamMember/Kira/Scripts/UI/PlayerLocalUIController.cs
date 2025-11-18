using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player内のLocalUIの管理
/// </summary>
public class PlayerLocalUIController : NetworkBehaviour {

    enum TextIndex {
        Current = 0,
        Max,
        Partition,
        WeaponName,
    }

    [SerializeField]TextMeshProUGUI[] mainWeaponText;
    [SerializeField]TextMeshProUGUI[] subWeaponText;

    [SerializeField]Image[] skill_Icon;
    [SerializeField]Image skill_State;
    [SerializeField]Image[] passive_Icon;
    [SerializeField]Image passive_State;
    [SerializeField]GeneralCharacter player;
    [SyncVar]float skillStateProgress = 0.0f;
    [SyncVar]float passiveStateProgress = 0.0f;

    void Start() {
        LocalUIChanged();
    }

    void Update() {
        //スキルの表示状態管理
        if(player.isCanSkill) {
            skill_State.fillAmount = 1.0f;
            skill_State.color = Color.yellow;
        }       
        else {
            skillStateProgress = player.skillAfterTime / player.equippedSkills[0].cooldown;
            skill_State.fillAmount = skillStateProgress;
            skill_State.color = Color.white;           
        }
        //パッシブの表示状態管理
        if(player.equippedPassives[0].IsPassiveActive) {
            //passiveStateProgress = player.equippedPassives[0].CoolTime / player.equippedPassives[0].Cooldown;
            //passive_State.fillAmount = passiveStateProgress;
            passive_State.color = Color.yellow;
        }
        else {
            passive_State.color = Color.white;
        }

        //メインウェポンの現在弾倉数を更新
        mainWeaponText[(int)TextIndex.Current].text = player.magazine.ToString();
        //サブウェポンの現在所持数を更新
        subWeaponText[(int)TextIndex.Current].text = player.weaponController_sub.currentUses.ToString();
    }

    public void LocalUIChanged() {
        for (int i  = 0; i < skill_Icon.Length ; i++) {
            skill_Icon[i].sprite = player.equippedSkills[0].skillIcon;
        }
        for (int i  = 0; i < passive_Icon.Length ; i++) {
            passive_Icon[i].sprite = player.equippedPassives[0].passiveIcon;
        } 
        
        //プレイヤーの弾倉が存在すればメインウェポンの弾倉UIを有効化する
        if( player.magazine >= 1) {
            for(int i = 0 ; i< mainWeaponText.Length ; i++) {
                mainWeaponText[i].enabled = true;
            }
            mainWeaponText[(int)TextIndex.Current].text = player.magazine.ToString();
            mainWeaponText[(int)TextIndex.Max].text = player.weaponController_main.weaponData.maxAmmo.ToString();
            mainWeaponText[(int)TextIndex.WeaponName].text = player.weaponController_main.weaponData.weaponName;
        }
        //プレイヤーのサブウェポンUIを反映
        subWeaponText[(int)TextIndex.Current].text = player.weaponController_sub.currentUses.ToString();
        subWeaponText[(int)TextIndex.Max].text = player.weaponController_sub.subWeaponData.maxUses.ToString();
        subWeaponText[(int)TextIndex.WeaponName].text = player.weaponController_sub.subWeaponData.WeaponName;
    }
}
