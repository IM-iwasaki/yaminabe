using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// TPSカメラ制御スクリプト
/// プレイヤーが見えにくくなる障害物を自動で半透明する
/// </summary>
public class PlayerCamera : MonoBehaviour {
    [Header("プレイヤー参照")]
    public Transform player;

    public Vector3 normalOffset = new Vector3(0f, 0f, -4f);

    [Header("カメラ設定")]
    public float rotationSpeed = 120f;
    public float minPitch = -20f;
    public float maxPitch = 60f;
    public float moveSpeed = 10f;
    public float upOffsetAmount = 0f;
    public float leftOffsetAmount = -2f;

    [Header("透明化設定")]
    public LayerMask transparentMask;  // 障害物レイヤー
    public float fadeAlpha = 0.1f;     // 透明化したときのアルファ
    public float fadeSpeed = 5f;       // フェード速度

    private float yaw;
    private float pitch;
    private Vector2 lookInput;
    private Vector3 currentOffset;
    private Vector3 targetOffset;

    // 現在透明化しているオブジェクト
    private Dictionary<Renderer, float> fadingObjects = new Dictionary<Renderer, float>();

    private void Start() {
        currentOffset = normalOffset;
        targetOffset = normalOffset;
    }

    public void OnLook(InputAction.CallbackContext context) {
        lookInput = context.ReadValue<Vector2>();
    }

    private void LateUpdate() {
        if (!player) return;

        // 回転入力処理
        yaw += lookInput.x * rotationSpeed * Time.deltaTime;
        pitch -= lookInput.y * rotationSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        targetOffset = rotation * normalOffset;
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, moveSpeed * Time.deltaTime);

        // プレイヤー位置
        Vector3 playerPos = player.position + Vector3.up * 1.5f;

        // 左寄せ
        Vector3 leftScreenOffset = player.right * -leftOffsetAmount + player.up * upOffsetAmount;
        Vector3 lookTarget = playerPos + leftScreenOffset;

        // カメラ位置
        Vector3 desiredPos = playerPos + currentOffset;
        transform.position = desiredPos;
        transform.LookAt(lookTarget);

        // 透明化処理
        HandleTransparency(playerPos, desiredPos);
    }

    /// <summary>
    /// プレイヤーとカメラの間にある複数のオブジェクトを透明化する
    /// </summary>
    private void HandleTransparency(Vector3 playerPos, Vector3 cameraPos) {
        // プレイヤーとの間を全Raycast
        Vector3 dir = playerPos - cameraPos;
        float distance = Vector3.Distance(playerPos, cameraPos);
        RaycastHit[] hits = Physics.RaycastAll(cameraPos, dir.normalized, distance, transparentMask);

        // 現在ヒットしたオブジェクト
        HashSet<Renderer> currentHits = new HashSet<Renderer>();

        foreach (var hit in hits) {
            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend) {
                currentHits.Add(rend);
                if (!fadingObjects.ContainsKey(rend)) {
                    fadingObjects[rend] = 1f;
                }

                float currentAlpha = fadingObjects[rend];
                currentAlpha = Mathf.MoveTowards(currentAlpha, fadeAlpha, Time.deltaTime * fadeSpeed);
                SetRendererAlpha(rend, currentAlpha);
                fadingObjects[rend] = currentAlpha;
            }
        }

        // 削除リストを用意
        List<Renderer> toRemove = new List<Renderer>();

        // 視界から外れたオブジェクトをフェードイン
        foreach (var kvp in new List<KeyValuePair<Renderer, float>>(fadingObjects)) {
            Renderer rend = kvp.Key;
            if (!rend) {
                toRemove.Add(rend);
                continue;
            }

            if (!currentHits.Contains(rend)) {
                float currentAlpha = kvp.Value;
                currentAlpha = Mathf.MoveTowards(currentAlpha, 1f, Time.deltaTime * fadeSpeed);
                SetRendererAlpha(rend, currentAlpha);
                fadingObjects[rend] = currentAlpha;

                if (Mathf.Approximately(currentAlpha, 1f)) {
                    toRemove.Add(rend);
                }
            }
        }

        // ループ終了後に削除
        foreach (var rend in toRemove) {
            fadingObjects.Remove(rend);
        }
    }

    #region マテリアル設定
    private void SetRendererAlpha(Renderer rend, float alpha) {
        foreach (var mat in rend.materials) {
            if (mat.HasProperty("_Color")) {
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;

                // Transparent設定
                mat.SetFloat("_Mode", 2);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.renderQueue = 3000;
            }
        }
    }
    #endregion
}