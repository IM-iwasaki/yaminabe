using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ガチャシステム
/// 単発ガチャ・10連ガチャの抽選処理と結果表示を担当
/// </summary>
public class GachaSystem : MonoBehaviour {
    private readonly string SKIN_TAG = "Skin"; // プレイヤーのスキンオブジェクトを判定するタグ

    [Header("ガチャ設定")]
    public GachaData data;

    [Header("単発ガチャの価格")]
    [Min(0)]
    public int gachaCost = 100;

    [Header("カメラ制御")]
    [SerializeField] private CameraChangeController cameraManager; // 選択画面用カメラ移動
    [SerializeField] private Transform cameraTargetPoint;           // 選択画面カメラ位置

    [Header("UI")]
    [SerializeField] private GameObject gachaUI;                  // ガチャ選択画面UI

    [Header("ガチャアニメーション")]
    [SerializeField] private Animator gachaAnim;                  // ガチャ演出用アニメーター

    [Header("ガチャアニメーション設定")]
    [SerializeField]
    private float gachaAnimationWaitTime = 1.5f; // アニメーション開始から結果表示までの待機時間
    
    private bool isPulling = false;     // ガチャ実行中（結果表示含む）かどうか

    private GameObject currentPlayer; // 現在選択中のプレイヤー
    public bool isOpen;               // ガチャ画面の開閉状態およびカーソル状態

    /// <summary>
    /// ガチャ画面中かどうかのフラグ
    /// </summary>
    private bool isGacha = false;

    // 結果表示用
    private Canvas resultCanvas;      // 結果UIを配置するCanvas
    private Camera resultCamera;      // 結果を描画するためのカメラ

    private void Awake() {
        // 最初はガチャUIを非表示
        if (gachaUI != null)
            gachaUI.SetActive(false);
    }

    private void Update() {
        // ガチャ画面が閉じられた場合、結果Canvasを自動で破棄
        if (!isOpen && resultCanvas != null) {
            Destroy(resultCanvas.gameObject);
            resultCanvas = null;
        }
    }

    public event Action<GachaItem> OnItemPulled; // ガチャ抽選結果通知イベント


    #region オプション中ブロック

    private OptionMenu cachedOptionMenu;

    private OptionMenu Option {
        get {
            if (cachedOptionMenu == null)
                cachedOptionMenu = FindObjectOfType<OptionMenu>();
            return cachedOptionMenu;
        }
    }

    public bool IsBlockedByOptionMenu() {
        if (Option == null) return false;
        return Option.isOpen;
    }

    private void SetGachaState(bool open) {
        isGacha = open;
    }

    public bool IsGachaActive() {
        return isGacha;
    }

    #endregion

    #region ガチャ画面モード

    public void StartGachaSelect(GameObject player) {
        if (IsBlockedByOptionMenu()) return;
        if (currentPlayer != null) return;

        currentPlayer = player;
        SetGachaState(true);

        PlayerWallet.Instance?.ShowMoneyUI();

        if (gachaUI != null)
            gachaUI.SetActive(false);

        if (cameraManager != null && cameraTargetPoint != null)
            cameraManager.MoveCamera(player, cameraTargetPoint.position, cameraTargetPoint.rotation);

        Transform skin = FindChildWithTag(player.transform, SKIN_TAG);
        if (skin != null) skin.gameObject.SetActive(false);

        isOpen = true;
        ChangeCursorView();

        if (gachaUI != null)
            StartCoroutine(ShowUIAfterDelay(cameraManager));
    }

    public void EndGachaSelect() {
        if (currentPlayer == null) return;

        OffGachaAnim();

        if (gachaUI != null)
            gachaUI.SetActive(false);

        PlayerWallet.Instance?.HideMoneyUI();

        if (cameraManager != null)
            cameraManager.ReturnCamera();

        Transform skin = FindChildWithTag(currentPlayer.transform, SKIN_TAG);
        if (skin != null) skin.gameObject.SetActive(true);

        if (currentPlayer.GetComponent<PlayerLocalUIController>())
            currentPlayer.GetComponent<PlayerLocalUIController>().OnLocalUIObject();

        isOpen = false;
        ChangeCursorView();
        SetGachaState(false);

        currentPlayer = null;
    }

    private IEnumerator ShowUIAfterDelay(CameraChangeController camController) {
        float duration = camController != null ? camController.moveDuration : 1.5f;
        yield return new WaitForSeconds(duration);

        if (gachaUI != null)
            gachaUI.SetActive(true);
    }

    private Transform FindChildWithTag(Transform parent, string tag) {
        foreach (Transform child in parent) {
            if (child.CompareTag(tag)) return child;

            Transform found = FindChildWithTag(child, tag);
            if (found != null) return found;
        }
        return null;
    }

    #endregion

    #region カーソル制御

    private void ChangeCursorView() {
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }

    #endregion

    #region ガチャ抽選

    /// <summary>
    /// 単発ガチャを引く
    /// </summary>
    public void PullSingle() {
        // ガチャ結果表示中は引けない
        if (isPulling) return;

        if (PlayerWallet.Instance == null) return;

        // 所持金チェックと支払い
        if (!PlayerWallet.Instance.SpendMoney(gachaCost)) return;

        // ガチャの一連の流れをCoroutineで制御
        StartCoroutine(PullSingleFlow());
    }

    /// <summary>
    /// 複数回（10連など）ガチャを引く
    /// </summary>
    public void PullMultiple(int count) {
        // ガチャ結果表示中は引けない
        if (isPulling) return;

        if (PlayerWallet.Instance == null || count <= 0) return;

        int totalCost = gachaCost * count;

        if (!PlayerWallet.Instance.SpendMoney(totalCost))
            return;

        // ガチャの一連の流れをCoroutineで制御
        StartCoroutine(PullMultipleFlow(count));
    }

    /// <summary>
    /// ガチャ抽選の内部処理
    /// </summary>
    private GachaItem PullSingleInternal() {
        if (data == null) return null;

        int roll = UnityEngine.Random.Range(0, 100);
        int current = 0;
        Rarity selectedRarity = Rarity.Common;

        foreach (var r in data.rarityRates) {
            current += r.rate;
            if (roll < current) {
                selectedRarity = r.rarity;
                break;
            }
        }

        var pool = data.GetItemsByRarity(selectedRarity);
        if (pool == null || pool.Count == 0) return null;

        int totalRate = 0;
        foreach (var item in pool) totalRate += item.rate;
        if (totalRate <= 0) return null;

        int randomValue = UnityEngine.Random.Range(0, totalRate);
        int currentWeight = 0;

        foreach (var item in pool) {
            currentWeight += item.rate;
            if (randomValue < currentWeight)
                return item;
        }

        return null;
    }

    #endregion

    #region ガチャ実行フロー

    /// <summary>
    /// 単発ガチャの一連の処理フロー
    /// </summary>
    private IEnumerator PullSingleFlow() {
        // ガチャ実行中ロック
        isPulling = true;

        // 前回のガチャ結果を削除
        ClearPreviousResults();

        // ガチャ演出（一定時間待機）
        yield return StartCoroutine(PlayGachaAnimationAndWait());

        // 抽選処理
        var item = PullSingleInternal();
        if (item == null) {
            isPulling = false;
            yield break;
        }

        PlayerItemManager.Instance.UnlockGachaItem(item);

        // 抽選結果通知
        OnItemPulled?.Invoke(item);

        // 結果表示
        yield return StartCoroutine(ShowSingleResult(item));

        // ロック解除
        isPulling = false;
    }

    /// <summary>
    /// 複数回ガチャ（10連など）の一連の処理フロー
    /// </summary>
    private IEnumerator PullMultipleFlow(int count) {
        // ガチャ実行中ロック
        isPulling = true;

        // 前回のガチャ結果を削除
        ClearPreviousResults();

        // ガチャ演出（一定時間待機）
        yield return StartCoroutine(PlayGachaAnimationAndWait());

        List<GachaItem> results = new();

        // 抽選処理
        for (int i = 0; i < count; i++) {
            var item = PullSingleInternal();
            if (item == null) continue;

            results.Add(item);
            PlayerItemManager.Instance.UnlockGachaItem(item);

            // 抽選結果通知
            OnItemPulled?.Invoke(item);
        }

        // 結果表示
        yield return StartCoroutine(ShowMultipleResults(results));

        // ロック解除
        isPulling = false;
    }

    #endregion

    #region ガチャアニメーション

    private void OnGachaAnim() => gachaAnim.SetBool("Open", true);
    private void OffGachaAnim() => gachaAnim.SetBool("Open", false);

    private IEnumerator PlayGachaAnimation() {
        OffGachaAnim();
        yield return null;
        OnGachaAnim();
    }

    /// <summary>
    /// ガチャアニメーションを再生し、一定時間待機する
    /// </summary>
    private IEnumerator PlayGachaAnimationAndWait() {
        OffGachaAnim();
        yield return null;
        OnGachaAnim();

        // アニメーション開始後、指定時間待機
        yield return new WaitForSeconds(gachaAnimationWaitTime);
    }

    #endregion

    #region レアリティ演出

    /// <summary>
    /// レアリティに応じた色を返す
    /// </summary>
    private Color GetRarityColor(Rarity rarity) {
        return rarity switch {
            Rarity.Common => Color.white,
            Rarity.Rare => new Color(0.2f, 0.6f, 1f),        // ド派手青
            Rarity.Epic => new Color(0.8f, 0.3f, 1f),        // ド紫
            Rarity.Legendary => new Color(1f, 0.85f, 0.2f),  // 金ピカ
            _ => Color.white
        };
    }

    /// <summary>
    /// グロー
    /// </summary>
    private IEnumerator GlowEffect(Image glow) {
        if (glow == null) yield break;

        float t = 0f;
        Color baseColor = glow.color;

        while (glow != null) {
            t += Time.deltaTime * 4f;

            // かなり強めの点滅
            float alpha = (Mathf.Sin(t) + 1f) * 0.35f + 0.3f;

            glow.color = new Color(
                baseColor.r,
                baseColor.g,
                baseColor.b,
                alpha
            );

            yield return null;
        }
    }

    /// <summary>
    /// 枠の演出
    /// </summary>
    private IEnumerator FramePunchEffect(RectTransform frame) {
        if (frame == null) yield break;

        Vector3 baseScale = Vector3.one;
        float t = 0f;

        while (frame != null) {
            t += Time.deltaTime * 16f;

            // 激しさMAX
            float scale =
                1f
                + Mathf.Abs(Mathf.Sin(t)) * 0.55f
                + Mathf.Abs(Mathf.Sin(t * 2f)) * 0.25f;

            frame.localScale = baseScale * scale;

            yield return null;
        }
    }

    #endregion

    #region 結果表示共通処理

    private void EnsureResultCanvas() {
        if (resultCanvas != null) return;

        GameObject canvasObj = new GameObject("ResultCanvas");
        resultCanvas = canvasObj.AddComponent<Canvas>();
        resultCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
    }

    private void EnsureResultCamera() {
        if (resultCamera != null) return;

        GameObject camObj = new GameObject("ResultCamera");
        resultCamera = camObj.AddComponent<Camera>();
        resultCamera.clearFlags = CameraClearFlags.SolidColor;
        resultCamera.backgroundColor = Color.black;
        resultCamera.enabled = false;
    }

    private void CreateResultUI(Transform parent, GachaItem item, float iconSize) {
        Color rarityColor = GetRarityColor(item.rarity);

        GameObject root = new GameObject(item.itemName + "_Root");
        root.transform.SetParent(parent, false);

        RectTransform rootRT = root.AddComponent<RectTransform>();
        rootRT.sizeDelta = new Vector2(iconSize + 40, iconSize + 40);

        GameObject glowObj = new GameObject("Glow");
        glowObj.transform.SetParent(root.transform, false);
        Image glow = glowObj.AddComponent<Image>();
        glow.color = rarityColor;
        glow.rectTransform.sizeDelta = rootRT.sizeDelta;
        StartCoroutine(GlowEffect(glow));

        GameObject frameObj = new GameObject("Frame");
        frameObj.transform.SetParent(root.transform, false);
        Image frame = frameObj.AddComponent<Image>();
        frame.color = rarityColor;
        frame.rectTransform.sizeDelta = new Vector2(iconSize + 10, iconSize + 10);

        GameObject iconObj = new GameObject(item.itemName);
        iconObj.transform.SetParent(root.transform, false);
        RawImage img = iconObj.AddComponent<RawImage>();

        RectTransform iconRT = iconObj.GetComponent<RectTransform>();
        iconRT.sizeDelta = new Vector2(iconSize, iconSize);

        GameObject temp = Instantiate(item.resultPrefab, Vector3.zero, Quaternion.identity);
        temp.SetActive(true);

        RenderTexture rtTex = new RenderTexture((int)iconSize, (int)iconSize, 16);
        resultCamera.targetTexture = rtTex;

        Vector3 offset = temp.transform.forward * 2f + Vector3.up;
        resultCamera.transform.position = temp.transform.position + offset;
        resultCamera.transform.LookAt(temp.transform.position + Vector3.up);

        resultCamera.Render();

        img.texture = rtTex;
        resultCamera.targetTexture = null;

        Destroy(temp);
    }

    #endregion

    #region 結果表示

    private void ClearPreviousResults() {
        if (resultCanvas == null) return;

        foreach (Transform child in resultCanvas.transform)
            Destroy(child.gameObject);
    }

    private IEnumerator ShowSingleResult(GachaItem item) {
        if (item == null || item.resultPrefab == null) yield break;

        EnsureResultCanvas();
        EnsureResultCamera();

        CreateResultUI(resultCanvas.transform, item, 256f);
        yield return null;
    }

    private IEnumerator ShowMultipleResults(List<GachaItem> items) {
        if (items == null || items.Count == 0) yield break;

        EnsureResultCanvas();
        EnsureResultCamera();

        float iconSize = 256f;
        float spacing = 10f;

        GameObject gridParent = new GameObject("GachaResultGrid");
        gridParent.transform.SetParent(resultCanvas.transform, false);

        RectTransform rt = gridParent.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(5 * (iconSize + 40) + 4 * spacing, 2 * (iconSize + 40) + spacing);
        rt.anchoredPosition = Vector2.zero;

        GridLayoutGroup grid = gridParent.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(iconSize + 40, iconSize + 40);
        grid.spacing = new Vector2(spacing, spacing);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 5;

        foreach (var item in items) {
            if (item.resultPrefab == null) continue;

            CreateResultUI(gridParent.transform, item, iconSize);
            yield return new WaitForEndOfFrame();
        }
    }

    #endregion
}