using Mirror;
using UnityEngine;
using System.Collections;

public class CountdownManager : NetworkSystemObject<CountdownManager> {
    [ClientRpc]
    public void RpcStartCountdown(int seconds) {
        if (CountdownUI.Instance != null) {
            // UI だけ表示、ゲーム開始処理は触らない
            StartCoroutine(CountdownUI.Instance.CountdownCoroutine(seconds));
            Debug.Log("コルーチン呼ばれた");
        }
    }

    [Server]
    public void StartCountdownForAll(int seconds) {
        Debug.Log("[StartCountdownForAll] isServer=" + isServer);
        RpcStartCountdown(seconds);
    }
}
