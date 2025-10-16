using UnityEngine;
using Mirror;
using System.Collections;

public class NetworkSceneTransitionSystem : NetworkSystemObject<NetworkSceneTransitionSystem> {
    [SerializeField] private float fadeDuration = 1f;

    // サーバー側がシーンを切り替える（全クライアントにフェード命令）
    [Server]
    public void ChangeScene(string sceneName) {
        RpcStartFadeOut(sceneName);
    }

    [ClientRpc]
    private void RpcStartFadeOut(string sceneName) {
        FadeManager.Instance.StartFadeOut(fadeDuration, () => {
            if (isServer) {
                // フェードが終わったらサーバーがシーンを変更（全員追従）
                StartCoroutine(ServerChangeSceneDelayed(sceneName));
            }
        });
    }

    [Server]
    private IEnumerator ServerChangeSceneDelayed(string sceneName) {
        yield return new WaitForSeconds(fadeDuration);
        NetworkManager.singleton.ServerChangeScene(sceneName);
    }

    public override void OnStartClient() {
        base.OnStartClient();
        // シーン切り替え後にフェードイン
        StartCoroutine(FadeInAfterSceneLoad());
    }

    private IEnumerator FadeInAfterSceneLoad() {
        yield return new WaitForSeconds(0.2f);
        if (FadeManager.Instance != null)
            FadeManager.Instance.StartFadeIn(fadeDuration);
    }
}
