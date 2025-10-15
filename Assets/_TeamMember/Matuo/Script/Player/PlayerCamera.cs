using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TPSカメラ制御スクリプト
/// プレイヤーが見えにくくなる障害物を自動で半透明する
/// </summary>
public class PlayerCamera : MonoBehaviour {
    [Header("プレイヤー参照")]
    public Transform player;
    [Header("カメラ設定")]
    public Vector3 offset = new Vector3(0, 0f, -4f);
    public float smoothSpeed = 10f;
    [Header("透明化設定")]
    public LayerMask obstacleMask;
    public float transparentAlpha = 0.1f;

    // 元のマテリアルを記録しておく
    private Dictionary<Renderer, Material[]> originalMaterials = new();
    // 現在透明化中のRenderer
    private HashSet<Renderer> transparentObjects = new();

    void LateUpdate() {
        if (player == null) return;

        Vector3 desiredPos = player.position + player.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * smoothSpeed);
        transform.LookAt(player.position + Vector3.up * 1.5f);

        HandleTransparency(player.position, transform.position);
    }

    /// <summary>
    /// プレイヤーとカメラの間にあるオブジェクトを透明化し、離れたら元に戻す
    /// </summary>
    private void HandleTransparency(Vector3 playerPos, Vector3 cameraPos) {
        Ray ray = new Ray(playerPos + Vector3.up * 1.5f, cameraPos - (playerPos + Vector3.up * 1.5f));
        float distance = Vector3.Distance(playerPos + Vector3.up * 1.5f, cameraPos);

        RaycastHit[] hits = Physics.RaycastAll(ray, distance, obstacleMask);
        HashSet<Renderer> hitRenderers = new();

        foreach (var hit in hits) {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend == null) continue;
            hitRenderers.Add(rend);

            // まだ透明化していないなら元のマテリアルを記録して透明化
            if (!transparentObjects.Contains(rend)) {
                originalMaterials[rend] = rend.materials; // 現在のマテリアルを保存
                MakeTransparent(rend);
                transparentObjects.Add(rend);
            }
        }

        // カメラとの間から外れたオブジェクトは元に戻す
        List<Renderer> toRestore = new();
        foreach (var rend in transparentObjects) {
            if (!hitRenderers.Contains(rend)) {
                RestoreMaterial(rend);
                toRestore.Add(rend);
            }
        }

        // コレクション変更はループ後に
        foreach (var rend in toRestore) {
            transparentObjects.Remove(rend);
            originalMaterials.Remove(rend);
        }
    }

    /// <summary>
    /// 透明用マテリアルに切り替える
    /// </summary>
    private void MakeTransparent(Renderer rend) {
        foreach (var mat in rend.materials) {
            if (mat.HasProperty("_Color")) {
                Color c = mat.color;
                c.a = transparentAlpha;
                mat.color = c;
            }
        }
    }

    /// <summary>
    /// 元のマテリアルに戻す
    /// </summary>
    private void RestoreMaterial(Renderer rend) {
        if (originalMaterials.ContainsKey(rend)) {
            rend.materials = originalMaterials[rend];
        }
    }
}