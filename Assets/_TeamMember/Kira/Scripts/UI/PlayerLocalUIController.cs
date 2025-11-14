using Mirror;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Playerì‡ÇÃLocalUIÇÃä«óù
/// </summary>
public class PlayerLocalUIController : NetworkBehaviour {

    [SerializeField]Image[] skill_Icon;
    [SerializeField]Image skill_State;
    [SerializeField]Image[] passive_Icon;
    [SerializeField]Image passive_State;
    [SerializeField]GeneralCharacter player;
    //[SyncVar] 
    float skillStateProgress = 0.0f;
    float passiveStateProgress = 0.0f;

    void Start() {
        LocalUIChanged();
    }

    void Update() {
        if (!isLocalPlayer) return;

        if(player.isCanSkill) {
            skill_State.fillAmount = 1.0f;
            skill_State.color = Color.yellow;
        }       
        else {
            skillStateProgress = player.skillAfterTime / player.equippedSkills[0].cooldown;
            skill_State.fillAmount = skillStateProgress;
            skill_State.color = Color.white;           
        }

        if(player.equippedPassives[0].IsPassiveActive) {
            //passiveStateProgress = player.equippedPassives[0].CoolTime / player.equippedPassives[0].Cooldown;
            //passive_State.fillAmount = passiveStateProgress;
            passive_State.color = Color.yellow;
        }
        else {
            passive_State.color = Color.white;
        }
    }

    public void LocalUIChanged() {
        for (int i  = 0; i < skill_Icon.Length ; i++) {
            skill_Icon[i].sprite = player.equippedSkills[0].skillIcon;
        }
        for (int i  = 0; i < passive_Icon.Length ; i++) {
            passive_Icon[i].sprite = player.equippedPassives[0].passiveIcon;
        }  
    }
}
