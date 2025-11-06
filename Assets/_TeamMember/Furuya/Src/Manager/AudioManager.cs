using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;

/// <summary>
/// AudioManager使い方
/// サーバー上で一つだけ存在
/// クライアントは RPC で再生指示を受ける
/// 
/// BGM→ AudioManager.Instance.CmdPlayBGM("MainTheme", 2f);
/// ワールド系SE→ AudioManager.Instance.CmdPlayWorldSE("GunShot", transform.position);
/// パーソナル系SE→ AudioManager.Instance.CmdPlayUISE("ButtonClick");
/// </summary>
public class AudioManager : NetworkSystemObject<AudioManager> {
    [System.Serializable]
    public class AudioData {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
    }

    public List<AudioData> bgmList;
    public List<AudioData> worldSEList; // 全員に聞こえる3D音
    public List<AudioData> uiSEList;    // 個人専用の2D音

    private AudioSource bgmSource;
    private Coroutine fadeCoroutine;

    // --- 初期化 ---
    public override void Initialize() {
        if (!NetworkServer.active) return;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f; // BGMは2D
    }

    // ======================
    // --- BGM ---
    // ======================
    [Command(requiresAuthority = false)]
    public void CmdPlayBGM(string name, float fadeTime) {
        RpcPlayBGM(name, fadeTime);
    }

    [ClientRpc]
    private void RpcPlayBGM(string name, float fadeTime) {
        var data = bgmList.Find(b => b.name == name);
        if (data == null) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeInBGM(data, fadeTime));
    }

    // ======================
    // --- ワールド系 SE ---
    // ======================
    [Command(requiresAuthority = false)]
    public void CmdPlayWorldSE(string name, Vector3 position) {
        RpcPlayWorldSE(name, position);
    }

    [ClientRpc]
    private void RpcPlayWorldSE(string name, Vector3 position) {
        var data = worldSEList.Find(s => s.name == name);
        if (data == null) return;

        GameObject go = new GameObject("WorldSE_" + name);
        go.transform.position = position;

        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = data.clip;
        source.volume = data.volume;
        source.pitch = data.pitch;
        source.spatialBlend = 1f; // 3D
        source.minDistance = 1f;
        source.maxDistance = 20f;
        source.rolloffMode = AudioRolloffMode.Linear;

        source.Play();
        Destroy(go, data.clip.length);
    }

    // ======================
    // --- パーソナル系 SE ---
    // ======================
    [Command(requiresAuthority = false)]
    public void CmdPlayUISE(string name, NetworkConnectionToClient conn = null) {
        TargetPlayUISE(conn, name);
    }

    [TargetRpc]
    private void TargetPlayUISE(NetworkConnection conn, string name) {
        var data = uiSEList.Find(s => s.name == name);
        if (data == null) return;

        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = data.clip;
        source.volume = data.volume;
        source.pitch = data.pitch;
        source.spatialBlend = 0f; // 2D
        source.Play();
        Destroy(source, data.clip.length);
    }

    // ======================
    // --- BGM フェード ---
    // ======================
    private IEnumerator FadeInBGM(AudioData data, float duration) {
        bgmSource.clip = data.clip;
        bgmSource.pitch = data.pitch;
        bgmSource.volume = 0f;
        bgmSource.Play();

        float timer = 0f;
        while (timer < duration) {
            timer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, data.volume, timer / duration);
            yield return null;
        }
        bgmSource.volume = data.volume;
    }

    private IEnumerator FadeOutBGM(float duration) {
        float startVolume = bgmSource.volume;
        float timer = 0f;
        while (timer < duration) {
            timer += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.volume = startVolume;
    }
}
