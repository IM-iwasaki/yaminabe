using Mirror;
using UnityEngine;

/// <summary>
/// プレイヤーのアニメーション管理クラス
/// </summary>
public class CharacterAnimationController : NetworkBehaviour {
    //扱うアニメーター
    public Animator anim = null;
    public enum BaseAnimState {
        IdleBattle01_AR,
        RunFWD_AR,
        RunBWD_AR,
        RunLeft_AR,
        RunRight_AR,
        ShootSingleshot_HG01,
        Dead,
    }

    public enum UpperAnimStateGunner {
        BaseLayer,
        HundGun,
        AR,
        RPG,
        Sniper,
        Minigun,
    }

    [SyncVar(hook = nameof(OnBaseStateChanged))]
    public BaseAnimState baseState = BaseAnimState.IdleBattle01_AR;

    [SyncVar(hook = nameof(OnLayerChanged))]
    public int activeLayer;

    void OnBaseStateChanged(BaseAnimState _,BaseAnimState _new) {
        if (!isClient || anim == null) return;
        anim.Play(_new.ToString(), 0);
    }

    void OnLayerChanged(int _, int _newLayer) {
        for(int i = 0,max = anim.layerCount;i < max; i++) {
            anim.SetLayerWeight(i, i == _newLayer ? 1.0f : 0.0f);
        }
    }

    [TargetRpc]
    public void TargetRpcPlayTrigger(NetworkConnection _,string triggerName) {
        if (!isLocalPlayer) return;
        anim.SetTrigger(triggerName);
    }

    [Command]
    public void CmdMove(float _x,float _z) {
        if (_z > 0) baseState = BaseAnimState.RunFWD_AR;
        else if (_z < 0) baseState = BaseAnimState.RunBWD_AR;
        else if (_x > 0) baseState = BaseAnimState.RunLeft_AR;
        else if (_x < 0) baseState = BaseAnimState.RunRight_AR;
        //else baseState = BaseAnimState.IdleBattle01_AR;
    }

}
