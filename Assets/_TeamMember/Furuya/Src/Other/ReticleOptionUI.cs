using UnityEngine;
using UnityEngine.UI;

public class ReticleOptionUI : MonoBehaviour {
    [Header("Local preview (assign in Inspector)")]
    [SerializeField] private Image previewImage;   // Option 内のプレビューImage
    [SerializeField] private Sprite[] reticles;    // レティクル候補

    private int currentIndex = 0;
    private Sprite cachedHudSprite;
    private bool isLocal = false;
    private bool initialized = false;

    // 外部から初期化してもらう（生成直後にプレイヤー側から呼ぶ）
    public void Initialize(bool isLocalPlayer) {
        isLocal = isLocalPlayer;
        initialized = true;

        // HudManager と同期する（存在すれば）
        if (HudManager.Instance != null) {
            cachedHudSprite = HudManager.Instance.GetReticleSprite();
            int idx = GetIndexFromSprite(cachedHudSprite);
            //currentIndex = idx >= 0 ? idx : 0;
            LoadIndex();
            ApplyPreviewToBoth();
        }
    }

    /// <summary>
    /// Next / Prev / Apply / Cancel はすべてローカルのみ有効
    /// それぞれ名前の通りの処理をする
    /// </summary>
    public void OnNextButton() {
        if (!CheckLocal()) return;
        if (reticles == null || reticles.Length == 0) return;
        AudioManager.Instance.CmdPlayUISE("選択");
        currentIndex = (currentIndex + 1) % reticles.Length;
        ApplyPreviewToBoth();
    }

    public void OnPreviousButton() {
        if (!CheckLocal()) return;
        if (reticles == null || reticles.Length == 0) return;
        AudioManager.Instance.CmdPlayUISE("選択");
        currentIndex--;
        if (currentIndex < 0) currentIndex = reticles.Length - 1;
        ApplyPreviewToBoth();
    }

    public void OnApplyButton() {
        if (!CheckLocal()) return;
        if (reticles == null || reticles.Length == 0) return;
        Sprite s = reticles[currentIndex];
        HudManager.Instance?.SetReticleSprite(s);
        cachedHudSprite = s;
        // 必要なら PlayerPrefs やサーバ同期をここで行う
    }

    public void OnCancelButton() {
        if (!CheckLocal()) return;
        HudManager.Instance?.SetReticleSprite(cachedHudSprite);
        if (previewImage != null) previewImage.sprite = cachedHudSprite;
        int idx = GetIndexFromSprite(cachedHudSprite);
        currentIndex = idx >= 0 ? idx : 0;
    }

    /// <summary>
    /// プレビューを適用
    /// </summary>
    private void ApplyPreviewToBoth() {
        if (reticles == null || reticles.Length == 0) return;
        Sprite s = reticles[currentIndex];
        if (previewImage != null) previewImage.sprite = s;
        HudManager.Instance?.SetReticleSprite(s);

        //セーブ
        SaveIndex();
    }


    /// <summary>
    /// スプライト名から番号取得
    /// </summary>
    private int GetIndexFromSprite(Sprite s) {
        if (s == null || reticles == null) return -1;
        for (int i = 0; i < reticles.Length; i++)
            if (reticles[i] == s) return i;
        return -1;
    }

    /// <summary>
    /// 初期化済みかつローカルかチェック。false なら何もしない。
    /// </summary>
    private bool CheckLocal() {
        if (!initialized) {
            Debug.LogWarning("ReticleOptionUI: not initialized. Call Initialize(isLocal) after instantiation.");
            return false;
        }
        if (!isLocal) return false;
        return true;
    }

    /// <summary>
    /// レティクル番号のセーブ
    /// </summary>
    private void SaveIndex() {
        // 既存データをロードして所持金だけ更新
        var data = PlayerSaveData.Load();
        data.currentReticle = currentIndex;
        PlayerSaveData.Save(data);
    }

    /// <summary>
    /// セーブされている所持金のロード
    /// </summary>
    private void LoadIndex() {
        var data = PlayerSaveData.Load();
        currentIndex = data.currentReticle;
    }
}
