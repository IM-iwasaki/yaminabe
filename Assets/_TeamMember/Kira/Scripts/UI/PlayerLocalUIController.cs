using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Playerì‡ÇÃLocalUIÇÃä«óù
/// </summary>
public class PlayerLocalUIController : MonoBehaviour {

    [SerializeField]Image[] skill_Icon;
    [SerializeField]Image skill_State;
    [SerializeField]Image[] passive_Icon;
    [SerializeField]Image passive_State;
    [SerializeField]GeneralCharacter player;

    void Start() {
        LocalUIChanged();
    }

    void Update() {
        if(player.isCanSkill) {
            skill_State.fillAmount = 1.0f;
            skill_State.color = Color.yellow;
        }       
        else {
            skill_State.fillAmount = player.SkillAfterTime / player.equippedSkills[0].Cooldown;
            skill_State.color = Color.white;           
        }

        if(player.equippedPassives[0].IsPassiveActive) {
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
