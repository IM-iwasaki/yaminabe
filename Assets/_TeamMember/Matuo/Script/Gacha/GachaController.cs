using UnityEngine;

/// <summary>
/// ガチャとアイテム管理を連動させる簡易サンプル
/// </summary>
public class GachaController : MonoBehaviour {
    [Header("ガチャシステム")]
    public GachaSystem gachaSystem;

    [Header("プレイヤーアイテム管理")]
    public PlayerItemManager itemManager;

    private void Start() {
        if (gachaSystem != null && itemManager != null) {
            // ガチャ結果を受け取って自動でアイテム取得済みにする
            gachaSystem.OnItemPulled += OnItemPulled;
        }
    }

    /// <summary>
    /// ガチャでアイテムが当たった時の処理
    /// </summary>
    private void OnItemPulled(GachaItem item) {
        if (item == null) return;

        // もしガチャアイテムがキャラクターなら、最初のスキンだけ解放
        if (item.isCharacter)
        {
            itemManager.UnlockCharacterFromGacha(item.itemName);
        }
        // 必要に応じて UI 表示や演出もここらで呼べ
    }

    private void OnDestroy() {
        if (gachaSystem != null)
            gachaSystem.OnItemPulled -= OnItemPulled;
    }
}