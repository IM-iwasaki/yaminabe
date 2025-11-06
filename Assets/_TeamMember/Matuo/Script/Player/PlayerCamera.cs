using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Mirror;

/// <summary>
/// TPSカメラ制御スクリプト
/// プレイヤーとカメラの間の障害物を自動的に半透明化
/// </summary>
public class PlayerCamera : MonoBehaviour {
    [Header("プレイヤー参照")]
    public Transform player; // プレイヤー

    [Header("カメラ設定")]
    public Vector3 normalOffset = new Vector3(0f, 0f, -4f); // 通常時のカメラオフセット
    public float rotationSpeed = 90f;                      // カメラ回転速度
    public float minPitch = -60f;                           // カメラの下方向回転制限
    public float maxPitch = 40f;                            // カメラの上方向回転制限
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

    // 死亡中のカメラ処理用
    private bool isDeathView = false;   // 死亡視点中か
    private bool isTransitioningToDeathView = false; // 死亡カメラへ移行中か
    private Vector3 deathCamTargetPos;               // 死亡時カメラ目標位置
    private Quaternion deathCamTargetRot;            // 死亡時カメラ目標回転
    private float transitionProgress = 0f;           // 補間進行度
    public float deathCamTransitionTime = 2f;        // 死亡カメラ移行にかける時間

    private GameObject vignetteOverlay; // 画面暗転用オーバーレイ
    private Vector3 savedOffset;        // 復帰用のオフセット
    private float savedYaw;
    private float savedPitch;

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

    /// <summary>
    /// 死亡時カメラ視点へ移行
    /// </summary>
    public void EnterDeathView() {
        // ローカルプレイヤーでなければ処理しない
        var netIdentity = player.GetComponent<NetworkIdentity>();
        if (netIdentity && !netIdentity.isLocalPlayer)
            return;

        if (isDeathView || isTransitioningToDeathView) return;

        // 状態フラグ更新
        isTransitioningToDeathView = true;
        transitionProgress = 0f;

        // 現在のカメラ状態を保存
        savedOffset = currentOffset;
        savedYaw = yaw;
        savedPitch = pitch;

        // 外周暗転エフェクトを生成
        CreateVignetteOverlay();

        // カメラの目標位置と回転を設定
        if (player) {
            deathCamTargetPos = player.position + Vector3.up * 10f;
            deathCamTargetRot = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    /// <summary>
    /// リスポーン時に通常視点へ戻す
    /// </summary>
    public void ExitDeathView() {
        // ローカルプレイヤーでなければ処理しない
        var netIdentity = player.GetComponent<NetworkIdentity>();
        if (netIdentity && !netIdentity.isLocalPlayer)
            return;

        if (!isDeathView && !isTransitioningToDeathView) return;

        // 死亡ビューまたは遷移中を強制終了
        isDeathView = false;
        isTransitioningToDeathView = false;
        transitionProgress = 0f;

        // 暗転エフェクトを削除
        if (vignetteOverlay) Destroy(vignetteOverlay);

        // 保存しておいたカメラ角度・位置に一瞬で戻す
        currentOffset = savedOffset;
        yaw = savedYaw;
        pitch = savedPitch;

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPos = player.position + rot * currentOffset;

        transform.position = targetPos;
        transform.rotation = rot;
    }

    private void LateUpdate() {
        // ローカルプレイヤーでなければ処理しない
        var netIdentity = player.GetComponent<NetworkIdentity>();
        if (netIdentity && !netIdentity.isLocalPlayer)
            return;

        if (!player) return;

        // 死亡カメラ
        if (isTransitioningToDeathView) {
            transitionProgress += Time.deltaTime / deathCamTransitionTime;
            float t = Mathf.SmoothStep(0f, 1f, transitionProgress); // イージング

            transform.position = Vector3.Lerp(transform.position, deathCamTargetPos, t);
            transform.rotation = Quaternion.Slerp(transform.rotation, deathCamTargetRot, t);

            if (transitionProgress >= 1f) {
                isTransitioningToDeathView = false;
                isDeathView = true;
            }
            return;
        }

        // 死亡中は通常TPS制御を停止（固定視点のまま）
        if (isDeathView)
            return;

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

        // カメラとプレイヤーの間の障害物を透明化（ローカルプレイヤーのみ）
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
        }
    }

    #region マテリアル設定
    /// <summary>
    /// Rendererの現在アルファ値を取得
    /// </summary>
    private float GetRendererAlpha(Renderer rend) {
        if (!rend) return 1f;

        // 共有マテリアルではなく個別マテリアルを参照（他プレイヤーへの影響防止）
        Material mat = rend.material;
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

    #region デスカメラ用周りを暗くするUI
    /// <summary>
    /// 外周を暗くするUI
    /// </summary>
    private void CreateVignetteOverlay() {
        // すでに存在している場合はスキップ
        if (vignetteOverlay) return;

        // 新しいCanvasとイメージを生成
        Canvas canvas = new GameObject("VignetteCanvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // とりま最前面に出す

        GameObject imageObj = new GameObject("VignetteOverlay");
        imageObj.transform.SetParent(canvas.transform, false);

        UnityEngine.UI.Image img = imageObj.AddComponent<UnityEngine.UI.Image>();

        // テクスチャ作成
        Texture2D tex = new Texture2D(256, 256);
        for (int y = 0; y < 256; y++) {
            for (int x = 0; x < 256; x++) {
                float dx = (x - 128f) / 128f;
                float dy = (y - 128f) / 128f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01((dist - 0.5f) * 2f); // 中央0→外1
                tex.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
            }
        }
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));
        img.sprite = sprite;
        img.color = Color.black;
        img.rectTransform.anchorMin = Vector2.zero;
        img.rectTransform.anchorMax = Vector2.one;
        img.rectTransform.offsetMin = Vector2.zero;
        img.rectTransform.offsetMax = Vector2.zero;

        vignetteOverlay = canvas.gameObject;
    }
    #endregion
}