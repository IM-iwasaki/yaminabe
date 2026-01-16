using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ÉKÉ`ÉÉåãâ ï\é¶êÍópÇÃÉNÉâÉX
/// </summary>
public class GachaResult : MonoBehaviour {

    private Canvas resultCanvas;
    private Camera resultCamera;

    #region Public API

    public void Clear() {
        if (resultCanvas == null) return;

        foreach (Transform child in resultCanvas.transform) {
            Destroy(child.gameObject);
        }
    }

    public void ShowSingle(GachaItem item) {
        if (item == null || item.resultPrefab == null) return;

        EnsureCanvas();
        EnsureCamera();

        StartCoroutine(CreateResultUIRoutine(resultCanvas.transform, item, 256f));
    }

    public void ShowMultiple(List<GachaItem> items) {
        if (items == null || items.Count == 0) return;

        EnsureCanvas();
        EnsureCamera();

        StartCoroutine(ShowMultipleRoutine(items));
    }

    #endregion

    #region Canvas / Camera

    private void EnsureCanvas() {
        if (resultCanvas != null) return;

        GameObject obj = new GameObject("ResultCanvas");
        resultCanvas = obj.AddComponent<Canvas>();
        resultCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        obj.AddComponent<CanvasScaler>();
        obj.AddComponent<GraphicRaycaster>();
    }

    private void EnsureCamera() {
        if (resultCamera != null) return;

        GameObject obj = new GameObject("ResultCamera");
        resultCamera = obj.AddComponent<Camera>();
        resultCamera.clearFlags = CameraClearFlags.SolidColor;
        resultCamera.backgroundColor = Color.black;
        resultCamera.enabled = false;
    }

    #endregion

    #region Multiple Flow

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
    }

    #endregion

    #region UIê∂ê¨

    private IEnumerator CreateResultUIRoutine(
        Transform parent,
        GachaItem item,
        float iconSize
    ) {
        Color rarityColor = GetRarityColor(item.rarity);

        // UI
        GameObject root = new GameObject(item.itemName + "_Root");
        root.transform.SetParent(parent, false);

        RectTransform rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(iconSize + 40, iconSize + 40);

        Image glow = new GameObject("Glow").AddComponent<Image>();
        glow.transform.SetParent(root.transform, false);
        glow.rectTransform.sizeDelta = rootRT.sizeDelta;
        glow.color = rarityColor;
        StartCoroutine(GlowEffect(glow));

        Image frame = new GameObject("Frame").AddComponent<Image>();
        frame.transform.SetParent(root.transform, false);
        frame.rectTransform.sizeDelta = new Vector2(iconSize + 10, iconSize + 10);
        frame.color = rarityColor;

        RawImage icon = new GameObject("Icon").AddComponent<RawImage>();
        icon.transform.SetParent(root.transform, false);
        icon.rectTransform.sizeDelta = new Vector2(iconSize, iconSize);

        // éBâe
        GameObject temp = Instantiate(item.resultPrefab);
        temp.SetActive(true);

        RenderTexture rtTex = new RenderTexture((int)iconSize, (int)iconSize, 16);
        resultCamera.targetTexture = rtTex;

        Vector3 offset = temp.transform.forward * 2f + Vector3.up;
        resultCamera.transform.position = temp.transform.position + offset;
        resultCamera.transform.LookAt(temp.transform.position + Vector3.up);

        yield return new WaitForEndOfFrame();

        resultCamera.Render();

        icon.texture = rtTex;
        resultCamera.targetTexture = null;

        Destroy(temp);
    }

    #endregion

    #region ââèo

    private Color GetRarityColor(Rarity rarity) {
        return rarity switch {
            Rarity.Common => Color.white,
            Rarity.Rare => new Color(0.2f, 0.6f, 1f),
            Rarity.Epic => new Color(0.8f, 0.3f, 1f),
            Rarity.Legendary => new Color(1f, 0.85f, 0.2f),
            _ => Color.white
        };
    }

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

    #endregion
}