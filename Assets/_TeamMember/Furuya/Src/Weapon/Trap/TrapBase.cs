using Mirror;
using System.Collections;
using UnityEngine;

/// <summary>
/// トラップベース
/// </summary>
public struct TrapInitData {
    public int teamID;
    public string ownerName;
    public float activationDelay;
    public bool activationOnce;
    public EffectType activationEffect;
    public float duration;
}

public abstract class TrapBase : NetworkBehaviour {
    protected int ownerTeamID;
    protected string ownerName;
    protected bool isActivated;
    protected bool hasTriggered;

    [Server]
    public virtual void Init(TrapInitData data) {
        ownerTeamID = data.teamID;
        ownerName = data.ownerName;
        StartCoroutine(ActivationDelayRoutine(data.activationDelay));
    }

    [Server]
    protected IEnumerator ActivationDelayRoutine(float delay) {
        yield return new WaitForSeconds(delay);
        isActivated = true;
    }

    /// <summary>
    /// クライアントにエフェクト表示
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="effectType"></param>
    [ClientRpc(includeOwner = true)]
    protected void RpcPlayEffect(Vector3 pos, EffectType effectType) {
        var fx = EffectPoolRegistry.Instance.GetHitEffect(effectType);
        if (fx != null) {
            var instance = EffectPool.Instance.GetFromPool(fx, pos, Quaternion.identity);
            instance.SetActive(true);
            EffectPool.Instance.ReturnToPool(instance, 1.5f);
        }
    }
}