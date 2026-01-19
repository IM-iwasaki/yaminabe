using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD変更用のマネージャー
/// </summary>
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

    /// <summary>
    /// HUD にスプライトを即時反映する（呼び出し元がローカルチェックを行うこと）
    /// </summary>
    /// <param name="s"></param>
    public void SetReticleSprite(Sprite s) {
        if (reticleImage == null) return;
        reticleImage.sprite = s;
    }

    /// <summary>
    /// HUD の現在スプライトを取得（必要なら）
    /// </summary>
    /// <returns></returns>
    public Sprite GetReticleSprite() {
        return reticleImage != null ? reticleImage.sprite : null;
    }

    /// <summary>
    /// レティクルの表示・非表示を切り替える
    /// </summary>
    public void SetReticleVisible(bool visible) {
        if (reticleImage == null) return;
        reticleImage.enabled = visible;
    }


}
