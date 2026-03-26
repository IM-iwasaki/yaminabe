using UnityEngine;
using Mirror;
using Mirror.BouncyCastle.Security;

[CreateAssetMenu(menuName = "Character/Passive/Hacker_RuleBreaker")]
public class Passive_Hacker : PassiveBase {

    public override void PassiveReflection(CharacterBase user) {
        if (!user.isLocalPlayer) return;
        // ƒQ[ƒ€’†‚Å‚È‚¯‚ê‚Î‰½‚à‚µ‚È‚¢
        if (!GameManager.Instance.IsGameRunning()) return;



    }
}