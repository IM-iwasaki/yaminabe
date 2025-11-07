using UnityEngine;
using UnityEngine.UI;

public class HudManager : SystemObject<HudManager> {

    [Tooltip("HUD 上の Reticle Image をここに割り当ててください")]
    public Image reticleImage;

    public override void Initialize() {
        // 安全チェック
        if (reticleImage == null) {
            // タグで探すフォールバック（任意）
            var go = GameObject.FindGameObjectWithTag("Reticle");
            if (go != null) reticleImage = go.GetComponent<Image>();
        }

        if (reticleImage == null) {
            Debug.LogWarning("HudManager: reticleImage not assigned or found. Assign in Inspector.");
        }
    }

    // HUD にスプライトを即時反映する（呼び出し元がローカルチェックを行うこと）
    public void SetReticleSprite(Sprite s) {
        if (reticleImage == null) return;
        reticleImage.sprite = s;
    }

    // HUD の現在スプライトを取得（必要なら）
    public Sprite GetReticleSprite() {
        return reticleImage != null ? reticleImage.sprite : null;
    }
}
