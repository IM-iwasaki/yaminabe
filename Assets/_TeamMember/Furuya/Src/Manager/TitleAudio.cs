using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// AudioManager使い方
/// 音源をインスペクターで登録する
/// 音量、ピッチ(再生速度)が選べる
/// ※再生できない場合はピッチが変わってないことがある。0.1以上にすること
/// 呼び出したい音源をコードに書く、以下使用例
/// TitleAudio.Instance.PlaySE("ここに登録名");
/// TitleAudio.Instance.PlayBGM("名前",フェードイン時間(float))
/// TitleAudio.Instance.StopBGM(フェードアウト時間(float))
/// </summary>


public class TitleAudio : MonoBehaviour {

    public static TitleAudio Instance { get; private set; } = null;

    [System.Serializable]
    public class AudioData {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
    }

    public List<AudioData> bgmList;
    public List<AudioData> seList;

    private AudioSource bgmSource;

    private List<AudioSource> seSources = new List<AudioSource>();
    public int initialSESourceCount = 5;

    private Coroutine fadeCoroutine;

    //初期化処理
    public void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Destroy(gameObject);
            return;
        }

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;

        // SE用AudioSourceを初期化
        for (int i = 0; i < initialSESourceCount; i++) {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            seSources.Add(source);
        }

        PlayBGM();
    }

    public void PlayBGM() {
        AudioData data = bgmList.Find(b => b.name == "タイトル");
        if (data == null) {
            Debug.LogWarning("BGM not found: " + name);
            return;
        }

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeInBGM(data, 1f));
    }

    public void StopBGM(float fadeTime = 1f) {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutBGM(fadeTime));
    }


    public void PlaySE(string name) {

        Debug.Log("PlaySE called with: " + name);

        AudioData data = seList.Find(s => s.name == name);
        if (data == null) {
            Debug.LogWarning("SE not found: " + name);
            return;
        }

        AudioSource source = GetAvailableSESource();
        Debug.Log($"Using AudioSource: {source.GetInstanceID()}, clip: {data.clip.name}, volume: {data.volume}, pitch: {data.pitch}");

        source.clip = data.clip;
        source.volume = data.volume;
        source.pitch = data.pitch;
        source.Play();


    }


    private AudioSource GetAvailableSESource() {
        foreach (var source in seSources) {
            if (!source.isPlaying)
                return source;
        }

        // すべて使用中なら新しいAudioSourceを追加
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        seSources.Add(newSource);
        return newSource;
    }


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
