using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PveBossAttackController : NetworkBehaviour
{
    //  UŒ‚’†‚©‚Ç‚¤‚©
    public bool isAttacking {  get; private set; }

    private PveBossController boss;

    private void Awake() {
        boss = GetComponent<PveBossController>();
    }

    [Server]
    public bool TryAttack(Transform target) {
        //  UŒ‚’†‚È‚ç–³‹
        if(isAttacking) return false;

        //  ƒ^[ƒQƒbƒg‚ª‚¢‚È‚¯‚ê‚Î–³Œø
        if(target == null) return false;



        //  UŒ‚ŠJn
        isAttacking = true;

        return true;
    }

}
