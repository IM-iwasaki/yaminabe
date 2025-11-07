using UnityEngine;
using UnityEngine.UI;

public class ReticleOptionUI : MonoBehaviour {
    [Header("UI References")]
    [SerializeField] private Image previewImage;   // Option 内プレビュー用（必須）
    [SerializeField] private Image hudImage;       // HUD の実際のレティクルImage（未割当ならタグで検索）
    [Header("Reticles")]
    [SerializeField] private Sprite[] reticles;    // 登録するレティクル（配列チェックあり）

    private int currentIndex = 0;
    private Sprite cachedHudSprite;

    private void Start() {
        if (previewImage == null) {
            Debug.LogError("ReticleOptionUI: previewImage is not assigned.");
            enabled = false;
            return;
        }

        // hudImage が未割当ならタグ "Reticle" で探す
        if (hudImage == null) {
            var go = GameObject.FindGameObjectWithTag("Reticle");
            if (go != null) {
                hudImage = go.GetComponent<Image>();
                if (hudImage == null)
                    Debug.LogError("ReticleOptionUI: GameObject with tag 'Reticle' has no Image component.");
            }
            else {
                Debug.LogError("ReticleOptionUI: No GameObject found with tag 'Reticle'. Assign hudImage or add an Image with tag 'Reticle'.");
            }
        }

        // HUD の現在スプライトをキャッシュ（キャンセル時に戻すため）
        if (hudImage != null) cachedHudSprite = hudImage.sprite;

        // プレビューは HUD と同期（HUD のスプライトが配列内にあればその index を利用）
        int idx = GetHudReticleIndex();
        currentIndex = idx >= 0 ? idx : 0;
        UpdatePreviewOnly();
    }

    // 次へボタン
    public void OnNextButton() {
        if (reticles == null || reticles.Length == 0) return;
        currentIndex = (currentIndex + 1) % reticles.Length;
        UpdatePreviewOnly();
    }

    // 前へボタン
    public void OnPreviousButton() {
        if (reticles == null || reticles.Length == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = reticles.Length - 1;
        UpdatePreviewOnly();
    }

    // プレビュー更新
    private void UpdatePreviewOnly() {
        if (reticles == null || reticles.Length == 0) {
            previewImage.sprite = null;
            return;
        }
        previewImage.sprite = reticles[currentIndex];

        //反映
        hudImage.sprite = reticles[currentIndex];
        cachedHudSprite = hudImage.sprite;
    }

    // 適用ボタン（HUD に反映）
    public void OnApplyButton() {
        if (hudImage == null || reticles == null || reticles.Length == 0) return;
        hudImage.sprite = reticles[currentIndex];
        cachedHudSprite = hudImage.sprite;
    }

    // キャンセルボタン（プレビュー破棄して HUD を元に戻す）
    public void OnCancelButton() {
        if (hudImage != null) hudImage.sprite = cachedHudSprite;
        int idx = GetHudReticleIndex();
        currentIndex = idx >= 0 ? idx : 0;
        UpdatePreviewOnly();
    }

    // HUD の現在スプライトが配列の何番目か。見つからなければ -1。
    private int GetHudReticleIndex() {
        if (hudImage == null || reticles == null || reticles.Length == 0) return -1;
        Sprite current = hudImage.sprite;
        if (current == null) return -1;
        for (int i = 0; i < reticles.Length; i++) {
            if (reticles[i] == current) return i;
        }
        return -1;
    }
}
