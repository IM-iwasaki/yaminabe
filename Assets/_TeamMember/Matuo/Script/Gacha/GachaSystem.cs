using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガチャシステム本体
/// </summary>
public class GachaSystem : MonoBehaviour {
    private readonly string SKIN_TAG = "Skin";

    [Header("ガチャ設定")]
    public GachaData data;

    [Header("単発ガチャの価格")]
    [Min(0)]
    public int gachaCost = 100;

    [Header("カメラ制御")]
    [SerializeField] private CameraChangeController cameraManager;
    [SerializeField] private Transform cameraTargetPoint;

    [Header("UI")]
    [SerializeField] private GameObject gachaUI;

    [Header("ガチャアニメーション")]
    [SerializeField] private Animator gachaAnim;

    [Header("アニメーション待機時間")]
    [SerializeField] private float gachaAnimationWaitTime = 1.5f;

    [Header("結果演出")]
    [SerializeField] private GachaResult gachaResult;

    [Header("ガチャ集中線・キラキラ演出")]
    [SerializeField] private GachaEffect gachaEffect;

    private bool isPulling = false;     // ガチャ実行中ロック
    private bool isGacha = false;       // ガチャ画面中か
    public bool isOpen = false;         // UI・カーソル状態

    private GameObject currentPlayer;

    public event Action<GachaItem> OnItemPulled;

    private void Awake() {
        // 最初はガチャUIを非表示
        if (gachaUI != null)
            gachaUI.SetActive(false);
    }

    private void Update() {
        // ガチャ画面を閉じたら結果表示を破棄
        if (!isOpen && gachaResult != null)
            gachaResult.Clear();
    }

    #region オプション中ブロック

    private OptionMenu cachedOptionMenu;

    private OptionMenu Option {
        get {
            if (cachedOptionMenu == null)
                cachedOptionMenu = FindObjectOfType<OptionMenu>();
            return cachedOptionMenu;
        }
    }

    /// <summary>
    /// オプションメニューが開いている間はガチャを禁止
    /// </summary>
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

    /// <summary>
    /// ガチャ選択画面開始
    /// </summary>
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

    /// <summary>
    /// ガチャ画面終了
    /// </summary>
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

        if (currentPlayer.TryGetComponent(out PlayerLocalUIController ui))
            ui.OnLocalUIObject();

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

    #region ガチャ実行API

    /// <summary>
    /// 単発ガチャを引く
    /// </summary>
    public void PullSingle() {
        if (isPulling) return;
        if (!PlayerWallet.Instance.SpendMoney(gachaCost)) return;

        // ガチャの一連の流れをCoroutineで制御
        StartCoroutine(PullSingleFlow());
    }

    /// <summary>
    /// 10連ガチャを引く
    /// </summary>
    public void PullMultiple(int count) {
        if (isPulling || count <= 0) return;

        int totalCost = gachaCost * count;
        if (!PlayerWallet.Instance.SpendMoney(totalCost)) return;

        // ガチャの一連の流れをCoroutineで制御
        StartCoroutine(PullMultipleFlow(count));
    }

    #endregion

    #region ガチャフロー

    /// <summary>
    /// 単発ガチャの一連の処理
    /// </summary>
    private IEnumerator PullSingleFlow() {
        // ガチャ実行中ロック
        isPulling = true;
        gachaResult.Clear();

        // 先に抽選
        GachaItem item = PullSingleInternal();
        if (item != null) {
            PlayerItemManager.Instance.UnlockGachaItem(item);
            // 抽選結果通知
            OnItemPulled?.Invoke(item);
        }

        Rarity highestRarity = item != null ? item.rarity : Rarity.Common;

        // 演出付きアニメーション
        yield return PlayGachaAnimationAndWait(highestRarity);

        if (item != null)
            gachaResult.ShowSingle(item);

        isPulling = false;
    }

    /// <summary>
    /// 10連の一連の処理
    /// </summary>
    private IEnumerator PullMultipleFlow(int count) {
        // ガチャ実行中ロック
        isPulling = true;
        gachaResult.Clear();

        List<GachaItem> results = new();

        // 抽選処理
        for (int i = 0; i < count; i++) {
            GachaItem item = PullSingleInternal();
            if (item == null) continue;

            results.Add(item);
            PlayerItemManager.Instance.UnlockGachaItem(item);
            // 抽選結果通知
            OnItemPulled?.Invoke(item);
        }

        Rarity highestRarity = GetHighestRarity(results);

        yield return PlayGachaAnimationAndWait(highestRarity);

        gachaResult.ShowMultiple(results);
        isPulling = false;
    }

    #endregion

    #region 抽選処理

    private GachaItem PullSingleInternal() {
        int roll = UnityEngine.Random.Range(0, 100);
        int current = 0;
        Rarity rarity = Rarity.Common;

        foreach (var r in data.rarityRates) {
            current += r.rate;
            if (roll < current) {
                rarity = r.rarity;
                break;
            }
        }

        List<GachaItem> pool = data.GetItemsByRarity(rarity);
        if (pool == null || pool.Count == 0) return null;

        int total = 0;
        foreach (var item in pool) total += item.rate;

        int value = UnityEngine.Random.Range(0, total);
        int weight = 0;

        foreach (var item in pool) {
            weight += item.rate;
            if (value < weight) return item;
        }

        return null;
    }

    private Rarity GetHighestRarity(List<GachaItem> items) {
        Rarity highest = Rarity.Common;
        foreach (var item in items) {
            if (item.rarity > highest)
                highest = item.rarity;
        }
        return highest;
    }

    #endregion

    #region ガチャアニメーション

    private void OnGachaAnim() => gachaAnim.SetBool("Open", true);
    private void OffGachaAnim() => gachaAnim.SetBool("Open", false);

    private IEnumerator PlayGachaAnimationAndWait(Rarity rarity) {
        OffGachaAnim();
        yield return null;

        if (gachaEffect != null)
            gachaEffect.Play(rarity);

        OnGachaAnim();

        yield return new WaitForSeconds(gachaAnimationWaitTime);

        if (gachaEffect != null)
            gachaEffect.Stop();
    }

    #endregion
}