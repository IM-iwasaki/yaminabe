using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RpcHub : NetworkBehaviour
{
    public static RpcHub instance = null;

    private void Awake() => instance = this;

    [ClientRpc]
    public void RpcShowLoadingUI() {
        LoadingUI.instance.ShowLoading(RuleManager.Instance.currentRule);
    }

    [ClientRpc]
    public void RpcHideLoadingUI() {
        StartCoroutine(LoadingUI.instance.HideLoading());
    }
}
