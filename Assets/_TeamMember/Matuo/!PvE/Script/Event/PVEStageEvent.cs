using Mirror;
using UnityEngine;

public abstract class PVEStageEvent : NetworkBehaviour {

    [ClientRpc]
    public void RpcExecute() {
        Execute();
    }

    protected abstract void Execute();
}