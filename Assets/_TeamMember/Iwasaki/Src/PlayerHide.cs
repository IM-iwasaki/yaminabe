using UnityEngine;
using Mirror;

public class PlayerHide : NetworkBehaviour {
    private Renderer[] renderers;           // プレイヤーの全Rendererを保持
    private Material[] originalMaterials;   // 元のマテリアルを保持して色を復元できるようにする

    [Header("透明度")]
    [Range(0f, 1f)]
    public float localAlpha = 0.3f; // 自分から見たときの半透明度

    void Start() {
        // プレイヤーの子オブジェクトも含めてRendererを取得
        renderers = GetComponentsInChildren<Renderer>();

        // 元のマテリアルを保存
        originalMaterials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) {
            originalMaterials[i] = renderers[i].material;
        }
    }

    /// <summary>
    /// プレイヤーを隠す/表示するコマンド
    /// クライアント → サーバーに送信
    /// </summary>
    /// <param name="isHidden">隠すかどうか</param>
    [Command]
    public void SetHidden(bool isHidden) {
        // サーバーから全クライアントに反映
        RpcSetHidden(isHidden);
    }

    /// <summary>
    /// 全クライアントで表示状態を更新
    /// サーバー → 全クライアント
    /// </summary>
    /// <param name="isHidden">隠すかどうか</param>
    [ClientRpc]
    private void RpcSetHidden(bool isHidden) {
        for (int i = 0; i < renderers.Length; i++) {
            if (isLocalPlayer) {
                // 自分のプレイヤーは半透明で表示
                Color color = originalMaterials[i].color;
                color.a = isHidden ? localAlpha : 1f; // 範囲内なら半透明、範囲外なら元に戻す
                renderers[i].material.color = color;

                // 半透明でも必ず表示させる
                renderers[i].enabled = true;
            }
            else {
                // 他のプレイヤーからは完全に消す
                renderers[i].enabled = !isHidden;
            }
        }
    }
}
