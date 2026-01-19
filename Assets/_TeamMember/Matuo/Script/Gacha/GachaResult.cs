using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ガチャ結果表示専用クラス
/// </summary>
public class GachaResult : MonoBehaviour {

    private Canvas resultCanvas;
    private Camera resultCamera;
    private int gachaRenderLayer;

    /// <summary>
    /// すべての結果表示演出が完了したときに通知されるイベント
    /// </summary>
    public event Action OnResultAnimationFinished;

    [Header("結果表示SE（AudioManager）")]
    [SerializeField] private string resultSEName = "GachaResult";

    /// <summary>
    /// 結果表示UIと進行中の演出をすべてクリアする
    /// ガチャ画面を閉じたときなどに呼ばれる
    /// </summary>
    public void Clear() {
        StopAllCoroutines();

        if (resultCanvas == null) return;

        foreach (Transform child in resultCanvas.transform) {
            if (child != null)
                Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 単発ガチャの結果を表示する
    /// </summary>
    public void ShowSingle(GachaItem item) {
        if (item == null || item.resultPrefab == null) return;

        EnsureCanvas();
        EnsureCamera();

        StartCoroutine(ShowSingleRoutine(item));
    }

    /// <summary>
    /// 複数（10連）ガチャの結果を表示する
    /// </summary>
    public void ShowMultiple(List<GachaItem> items) {
        if (items == null || items.Count == 0) return;

        EnsureCanvas();
        EnsureCamera();

        StartCoroutine(ShowMultipleRoutine(items));
    }

    #region Canvas / Camera

    /// <summary>
    /// 結果表示用 Canvas を生成・確保する
    /// </summary>
    private void EnsureCanvas() {
        if (resultCanvas != null) return;

        GameObject obj = new GameObject("ResultCanvas");
        resultCanvas = obj.AddComponent<Canvas>();
        resultCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        obj.AddComponent<CanvasScaler>();
        obj.AddComponent<GraphicRaycaster>();
    }

    /// <summary>
    /// 結果撮影用 Camera を生成・確保する
    /// </summary>
    private void EnsureCamera() {
        if (resultCamera != null) return;

        gachaRenderLayer = LayerMask.NameToLayer("GachaRender");

        GameObject obj = new GameObject("ResultCamera");
        resultCamera = obj.AddComponent<Camera>();
        resultCamera.clearFlags = CameraClearFlags.SolidColor;
        resultCamera.backgroundColor = Color.clear;
        resultCamera.cullingMask = 1 << gachaRenderLayer;
        resultCamera.enabled = false;
    }
    #endregion

    #region ガチャ演出
    /// <summary>
    /// 単発ガチャ結果表示の一連の演出
    /// </summary>
    private IEnumerator ShowSingleRoutine(GachaItem item) {
        yield return CreateResultUIRoutine(resultCanvas.transform, item, 256f);
        OnResultAnimationFinished?.Invoke();
    }

    /// <summary>
    /// 複数ガチャ（10連）結果表示の一連の演出
    /// </summary>
    private IEnumerator ShowMultipleRoutine(List<GachaItem> items) {
        float iconSize = 256f;
        float spacing = 10f;

        GameObject gridParent = new GameObject("GachaResultGrid");
        gridParent.transform.SetParent(resultCanvas.transform, false);

        RectTransform rt = gridParent.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(
            5 * (iconSize + 40) + 4 * spacing,
            2 * (iconSize + 40) + spacing
        );
        rt.anchoredPosition = Vector2.zero;

        GridLayoutGroup grid = gridParent.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(iconSize + 40, iconSize + 40);
        grid.spacing = new Vector2(spacing, spacing);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;

        foreach (var item in items) {
            if (item == null || item.resultPrefab == null) continue;
            yield return CreateResultUIRoutine(gridParent.transform, item, iconSize);
        }

        OnResultAnimationFinished?.Invoke();
    }
    #endregion

    #region UI生成
    /// <summary>
    /// ガチャ結果1個分のUIを生成し、表示演出を行う
    /// </summary>
    private IEnumerator CreateResultUIRoutine(
        Transform parent,
        GachaItem item,
        float iconSize
    ) {
        if (parent == null) yield break;

        Color rarityColor = GetRarityColor(item.rarity);

        // UIルート生成
        GameObject root = new GameObject(item.itemName + "_Root");
        root.transform.SetParent(parent, false);

        RectTransform rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(iconSize + 40, iconSize + 40);
        rootRT.localScale = Vector3.zero;

        // 光演出
        Image glow = new GameObject("Glow").AddComponent<Image>();
        glow.transform.SetParent(root.transform, false);
        glow.rectTransform.sizeDelta = rootRT.sizeDelta;
        glow.color = rarityColor;
        StartCoroutine(GlowEffect(glow));

        // 枠
        Image frame = new GameObject("Frame").AddComponent<Image>();
        frame.transform.SetParent(root.transform, false);
        frame.rectTransform.sizeDelta = new Vector2(iconSize + 10, iconSize + 10);
        frame.color = rarityColor;

        // アイコン
        RawImage icon = new GameObject("Icon").AddComponent<RawImage>();
        icon.transform.SetParent(root.transform, false);
        icon.rectTransform.sizeDelta = new Vector2(iconSize, iconSize);

        GameObject temp = Instantiate(item.resultPrefab);
        temp.SetActive(true);
        SetLayerRecursively(temp, gachaRenderLayer);

        RenderTexture rtTex = new RenderTexture((int)iconSize, (int)iconSize, 16);
        rtTex.Create();

        resultCamera.targetTexture = rtTex;

        Vector3 offset = temp.transform.forward * 2f + Vector3.up;
        resultCamera.transform.position = temp.transform.position + offset;
        resultCamera.transform.LookAt(temp.transform.position + Vector3.up);

        yield return new WaitForEndOfFrame();

        resultCamera.Render();

        icon.texture = rtTex;
        resultCamera.targetTexture = null;

        Destroy(temp);

        // ポップ演出
        StartCoroutine(PopBounce(rootRT));

        // UI専用SE（クライアントのみ）
        if (AudioManager.Instance != null) {
            AudioManager.Instance.CmdPlayUISE(resultSEName);
        }

        yield return new WaitForSeconds(0.15f);
    }
    #endregion

    #region Utility

    /// <summary>
    /// 指定した GameObject とその子をすべて指定レイヤーに設定する
    /// </summary>
    private void SetLayerRecursively(GameObject obj, int layer) {
        obj.layer = layer;
        foreach (Transform child in obj.transform) {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    /// <summary>
    /// レアリティに応じた表示カラーを取得する
    /// </summary>
    private Color GetRarityColor(Rarity rarity) {
        return rarity switch {
            Rarity.Common => Color.white,
            Rarity.Rare => new Color(0.2f, 0.6f, 1f),
            Rarity.Epic => new Color(0.8f, 0.3f, 1f),
            Rarity.Legendary => new Color(1f, 0.85f, 0.2f),
            _ => Color.white
        };
    }

    /// <summary>
    /// 枠の発光エフェクト
    /// </summary>
    private IEnumerator GlowEffect(Image glow) {
        float t = 0f;
        Color baseColor = glow.color;

        while (glow != null) {
            t += Time.deltaTime * 4f;
            float alpha = (Mathf.Sin(t) + 1f) * 0.35f + 0.3f;
            glow.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }
    }

    /// <summary>
    /// UI出現時に少し跳ねるポップ演出
    /// </summary>
    private IEnumerator PopBounce(RectTransform rt) {
        if (rt == null) yield break;

        float t = 0f;
        Vector3 start = Vector3.zero;
        Vector3 overshoot = Vector3.one * 1.15f;
        Vector3 end = Vector3.one;

        while (t < 1f) {
            if (rt == null) yield break;
            t += Time.deltaTime * 10f;
            rt.localScale = Vector3.Lerp(start, overshoot, t);
            yield return null;
        }

        t = 0f;

        while (t < 1f) {
            if (rt == null) yield break;
            t += Time.deltaTime * 12f;
            rt.localScale = Vector3.Lerp(overshoot, end, t);
            yield return null;
        }

        if (rt != null)
            rt.localScale = Vector3.one;
    }
    #endregion
}