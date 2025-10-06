using UnityEngine;
using Mirror;

public abstract class WeaponBase : NetworkBehaviour {
    public WeaponData data;
    protected float lastAttackTime;

    public virtual bool CanAttack() => Time.time >= lastAttackTime + data.cooldown;

    [Command]
    public void CmdAttack() {
        if (!CanAttack()) return;
        lastAttackTime = Time.time;
        ServerAttack();
    }

    protected abstract void ServerAttack();
}
