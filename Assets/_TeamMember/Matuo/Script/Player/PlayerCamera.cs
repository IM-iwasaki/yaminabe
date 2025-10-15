using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// TPSカメラ制御スクリプト
/// プレイヤーとカメラの間の障害物を自動的に半透明化
/// </summary>
public class PlayerCamera : MonoBehaviour {
    [Header("プレイヤー参照")]
    public Transform player; // プレイヤー

    [Header("カメラ設定")]
    public Vector3 normalOffset = new Vector3(0f, 0f, -4f); // 通常時のカメラオフセット
    public float rotationSpeed = 120f;                      // カメラ回転速度
    public float minPitch = -20f;                           // カメラの下方向回転制限
    public float maxPitch = 60f;                            // カメラの上方向回転制限
    public float moveSpeed = 10f;                           // カメラ位置補間速度

    [Header("画面位置調整")]
    public float upOffsetAmount = 0f;    // 視点の上下オフセット
    public float leftOffsetAmount = 2f; // 視点の左右オフセット

    [Header("透明化設定")]
    public LayerMask transparentMask; // 障害物検出に使用するレイヤーマスク
    public float fadeAlpha = 0.2f;    // 半透明化時の目標アルファ値
    public float fadeSpeed = 5f;      // 透明化/復元の速度

    private float yaw;                // カメラの水平回転角
    private float pitch;              // カメラの垂直回転角
    private Vector2 lookInput;        // 入力値
    private Vector3 currentOffset;    // 現在のカメラオフセット
    private Vector3 targetOffset;     // 目標オフセット

    /// <summary>
    /// 現在フェード処理中のオブジェクトを管理
    /// </summary>
    private readonly Dictionary<Renderer, (float current, float original)> fadingObjects = new();

    private void Start() {
        // 初期化
        currentOffset = normalOffset;
        targetOffset = normalOffset;
    }

    /// <summary>
    /// 入力アクションシステム
    /// </summary>
    public void OnLook(InputAction.CallbackContext context) {
        lookInput = context.ReadValue<Vector2>();
    }

    private void LateUpdate() {
        if (!player) return;

        // カメラ回転制御
        yaw += lookInput.x * rotationSpeed * Time.deltaTime; // 水平方向回転
        pitch -= lookInput.y * rotationSpeed * Time.deltaTime; // 垂直方向回転
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch); // ピッチ角制限

        // 回転情報からオフセットを計算
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        targetOffset = rotation * normalOffset;

        // スムーズにオフセットを補間
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, moveSpeed * Time.deltaTime);

        // プレイヤーの視点位置
        Vector3 playerPos = player.position + Vector3.up * 1.5f;

        // カメラの最終目標位置
        Vector3 desiredPos = playerPos + currentOffset;
        transform.position = desiredPos;

        // 見るべきターゲット位置
        Vector3 lookTarget = playerPos + (player.right * leftOffsetAmount) + (Vector3.up * upOffsetAmount);
        transform.LookAt(lookTarget);

        // カメラとプレイヤーの間の障害物を透明化
        HandleTransparency(playerPos, desiredPos);
    }

    /// <summary>
    /// プレイヤーとカメラの間にある障害物を半透明化
    /// </summary>
    private void HandleTransparency(Vector3 playerPos, Vector3 cameraPos) {
        // カメラ方向のベクトル
        Vector3 dir = playerPos - cameraPos;
        float dist = Vector3.Distance(playerPos, cameraPos);

        // レイキャストで間にある全障害物を取得
        RaycastHit[] hits = Physics.RaycastAll(cameraPos, dir.normalized, dist, transparentMask);

        // 今フレームでヒットしたRendererを記録
        HashSet<Renderer> hitRenderers = new();
        foreach (var hit in hits) {
            Renderer r = hit.collider.GetComponent<Renderer>();
            if (!r) continue;
            hitRenderers.Add(r);

            // 初回ヒットなら登録
            if (!fadingObjects.ContainsKey(r)) {
                float baseAlpha = GetRendererAlpha(r);
                fadingObjects[r] = (baseAlpha, baseAlpha);
            }
        }

        // 登録済み全オブジェクトのアルファ値を更新
        List<Renderer> keys = new(fadingObjects.Keys); // 列挙中の変更を避けるためコピー
        foreach (Renderer r in keys) {
            if (!r) { fadingObjects.Remove(r); continue; }

            // 現在と元のアルファ値を取得
            (float current, float original) data = fadingObjects[r];

            // 現在ヒット中ならfadeAlphaへ、ヒットしていなければ元に戻す
            float targetAlpha = hitRenderers.Contains(r) ? fadeAlpha : data.original;

            // スムーズに目標アルファへ移行
            data.current = Mathf.MoveTowards(data.current, targetAlpha, Time.deltaTime * fadeSpeed);
            SetRendererAlpha(r, data.current); // 実際に透明度を反映
            fadingObjects[r] = data;

            // 元のアルファに戻ったら削除
            if (!hitRenderers.Contains(r) && Mathf.Approximately(data.current, data.original))
                fadingObjects.Remove(r);
        }
    }
    #region マテリアル設定
    /// <summary>
    /// Rendererの現在アルファ値を取得
    /// </summary>
    private float GetRendererAlpha(Renderer rend) {
        if (!rend || rend.sharedMaterial == null) return 1f;

        Material mat = rend.sharedMaterial; // 共有マテリアル参照
        if (mat.HasProperty("_BaseColor"))
            return mat.GetColor("_BaseColor").a;
        if (mat.HasProperty("_Color"))
            return mat.GetColor("_Color").a;
        return 1f;
    }
    
    /// <summary>
    /// Rendererの透明度を設定
    /// </summary>
    private void SetRendererAlpha(Renderer rend, float alpha) {
        if (!rend) return;

        Material mat = rend.material; // 個別インスタンスを取得

        // マテリアルのアルファ値を変更
        if (mat.HasProperty("_BaseColor")) {
            Color c = mat.GetColor("_BaseColor");
            c.a = alpha;
            mat.SetColor("_BaseColor", c);
        } else if (mat.HasProperty("_Color")) {
            Color c = mat.GetColor("_Color");
            c.a = alpha;
            mat.SetColor("_Color", c);
        }

        // α値に応じて描画モードを自動的に切り替え
        if (alpha < 0.99f)
            SetMaterialToFade(mat);
        else
            SetMaterialToOpaque(mat);
    }

    /// <summary>
    /// マテリアルを半透明描画モードに設定
    /// </summary>
    private void SetMaterialToFade(Material mat) {
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    /// <summary>
    /// マテリアルを不透明描画モードに戻す
    /// </summary>
    private void SetMaterialToOpaque(Material mat) {
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        mat.SetInt("_ZWrite", 1);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = -1;
    }
    #endregion
}