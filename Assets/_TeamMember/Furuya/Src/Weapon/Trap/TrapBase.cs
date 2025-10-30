using Mirror;
using System.Collections;
using UnityEngine;

public struct TrapInitData {
    public int teamID;
    public float activationDelay;
    public bool activationOnce;
    public EffectType activationEffect;
    public float duration;
}

public abstract class TrapBase : NetworkBehaviour {
    protected int ownerTeamID;
    protected bool isActivated;
    protected bool hasTriggered;

    [Server]
    public virtual void Init(TrapInitData data) {
        ownerTeamID = data.teamID;
        StartCoroutine(ActivationDelayRoutine(data.activationDelay));
    }

    [Server]
    protected IEnumerator ActivationDelayRoutine(float delay) {
        yield return new WaitForSeconds(delay);
        isActivated = true;
    }

    [ClientRpc(includeOwner = true)]
    protected void RpcPlayEffect(Vector3 pos, EffectType effectType) {
        var fx = WeaponPoolRegistry.Instance.GetHitEffect(effectType);
        if (fx != null) {
            var instance = WeaponEffectPool.Instance.GetFromPool(fx, pos, Quaternion.identity);
            instance.SetActive(true);
            WeaponEffectPool.Instance.ReturnToPool(instance, 1.5f);
        }
    }
}