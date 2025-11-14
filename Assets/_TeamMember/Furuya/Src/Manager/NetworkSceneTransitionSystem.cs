using UnityEngine;
using Mirror;
using System.Collections;

//これを使いたいところで呼ぶだけ
//NetworkSceneTransitionSystem.Instance.ChangeScene("NextSceneName");

/// <summary>
/// シーン遷移用システム
/// </summary>
public class NetworkSceneTransitionSystem : NetworkSystemObject<NetworkSceneTransitionSystem> {
    //フェード時間
    [SerializeField] private float fadeDuration = 1f;

    // サーバー側がシーンを切り替える（全クライアントにフェード命令）
    [Server]
    public void ChangeScene(string sceneName) {
        RpcStartFadeOut(sceneName);

        if(sceneName == GameSceneManager.Instance.lobbySceneName) {
            AudioManager.Instance.CmdPlayBGM("ロビー", 2f);
        }
        else if (sceneName == GameSceneManager.Instance.gameSceneName) {
            AudioManager.Instance.CmdPlayBGM("ゲーム1", 2f);
        }
    }

    /// <summary>
    /// クライアントのシーン遷移
    /// </summary>
    /// <param name="sceneName">遷移先の名前</param>
    [ClientRpc]
    private void RpcStartFadeOut(string sceneName) {
        FadeManager.Instance.StartFadeOut(fadeDuration, () => {
            if (isServer) {
                // フェードが終わったらサーバーがシーンを変更（全員追従）
                StartCoroutine(ServerChangeSceneDelayed(sceneName));
            }
        });
    }

    /// <summary>
    /// サーバー上での遷移
    /// </summary>
    /// <param name="sceneName">シーン名</param>
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

    /// <summary>
    /// シーン遷移後のフェード
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeInAfterSceneLoad() {
        yield return new WaitForSeconds(0.2f);
        if (FadeManager.Instance != null)
            FadeManager.Instance.StartFadeIn(fadeDuration);
    }
}
