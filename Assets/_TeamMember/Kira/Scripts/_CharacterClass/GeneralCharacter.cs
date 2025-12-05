using UnityEngine;

//
//  @file   Second_CharacterClass
//
public class GeneralCharacter : CharacterBase {

    protected new void Awake() {
        base.Awake();      
    }    

    public override void OnStartClient() {
        base.OnStartClient();
        localUI.Initialize();


        if (!isLocalPlayer) return; // ©•ª‚¾‚¯•\¦
        SkillBase skill = paramater.equippedSkills[0];
        PassiveBase passive = paramater.equippedPassives[0];

        SkillDisplayer.Instance.SetSkillUI(
        skill.skillName, skill.skillDescription,
        passive.passiveName, passive.passiveDescription
        );
    }

    void Update() {
        if(!isLocalPlayer) return;  //©•ª‚¾‚¯ˆ—‚·‚é         

        //RespawnControl();    
               
        //€‚ñ‚Å‚¢‚½‚çˆÈ~‚Ìˆ—‚Ís‚í‚È‚¢B
        if (paramater.isDead) return;

        //UŒ‚“ü—Í‚ª‚ ‚éŠÔUŒ‚ŠÖ”‚ğŒÄ‚Ô(ŠÔŠu‚Ì§Œä‚ÍMainWeaponController‚Éˆê”C)
        if (paramater.isAttackPressed) StartAttack();


        MoveControl();
        JumpControl();       
        paramater.AbilityControl();
        //ƒgƒŠƒK[ƒŠƒZƒbƒgŠÖ”‚ÌŒÄ‚Ño‚µ
        paramater.ResetTrigger();
    }
}
