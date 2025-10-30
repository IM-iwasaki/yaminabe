using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ガチャシステム
/// </summary>
public class GachaSystem : MonoBehaviour {
    [Header("ガチャ設定")]
    public GachaData data;

    [Header("単発ガチャの価格")]
    [Min(0)]
    public int gachaCost = 100;

    [Header("カメラ制御")]
    [SerializeField] private CameraChangeController cameraManager;  // カメラ移動用Controller
    [SerializeField] private Transform cameraTargetPoint;           // 選択画面カメラ位置

    [Header("UI")]
    [SerializeField] private GameObject gachaUI;                  // 選択画面UI

    [Header("ガチャアニメーション")]
    [SerializeField] private Animator gachaAnim;

    private GameObject currentPlayer; // 現在選択中のプレイヤー

    private void Awake() {
        gachaUI.SetActive(false);
    }

    /// <summary>
    /// ガチャ結果通知イベント
    /// </summary>
    public event Action<GachaItem> OnItemPulled;

    /// <summary>
    /// 単発ガチャを引く
    /// </summary>
    /// <returns>貧乏ならnull</returns>
    public GachaItem PullSingle() {
        // 所持金チェック
        if (PlayerWallet.Instance == null) return null;
        // 支払い処理
        if (!PlayerWallet.Instance.SpendMoney(gachaCost)) {
            Debug.Log("貧乏過ぎて引けないよん");
            return null;
        }

        //  ガチャアニメーション再生
        StartCoroutine(PlayGachaAnimation());

        // 抽選処理
        var item = PullSingleInternal();
        if (item != null)
            PlayerItemManager.Instance.UnlockGachaItem(item);

        OnItemPulled?.Invoke(item);

        return item;
    }

    /// <summary>
    /// 複数回ガチャを引く
    /// </summary>
    /// <param name="count">引く回数</param>
    /// <returns>貧乏なら空</returns>
    public List<GachaItem> PullMultiple(int count) {

        List<GachaItem> results = new();
        if (PlayerWallet.Instance == null || count <= 0) return results;

        int totalCost = gachaCost * count;
        // 支払い処理
        if (!PlayerWallet.Instance.SpendMoney(totalCost)) {
            Debug.Log("貧乏過ぎて引けないよん");
            return results;
        }

        //  ガチャアニメーション再生
        StartCoroutine(PlayGachaAnimation());

        // 指定回数分抽選
        for (int i = 0; i < count; i++) {
            var item = PullSingleInternal();
            if (item != null) {
                results.Add(item);
                PlayerItemManager.Instance.UnlockGachaItem(item);
                OnItemPulled?.Invoke(item);
            }
        }
        return results;
    }

    /// <summary>
    /// ガチャ抽選処理
    /// </summary>
    private GachaItem PullSingleInternal() {
        if (data == null) return null;

        // レアリティ抽選
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

        // アイテム抽選
        var pool = data.GetItemsByRarity(selectedRarity);
        if (pool == null || pool.Count == 0) return null;

        // 各アイテムの rate に応じた抽選
        int totalRate = 0;
        foreach (var item in pool) totalRate += item.rate;
        if (totalRate <= 0) return null;

        int randomValue = UnityEngine.Random.Range(0, totalRate);
        int currentWeight = 0;
        foreach (var item in pool) {
            currentWeight += item.rate;
            if (randomValue < currentWeight) {
#if UNITY_EDITOR
                Debug.Log($"ガチャ結果: {item.itemName} ({selectedRarity})");
#endif
                return item;
            }
        }

        // 万一の保険
        return null;
    }

    #region キャラクター選択時のUI表示非表示、カメラの挙動
    /// <summary>
    /// キャラクター選択モードを開始
    /// </summary>
    /// <param name="player">操作中のプレイヤー</param>
    public void StartGachaSelect(GameObject player) {

        if (currentPlayer != null) return;
        currentPlayer = player;

        // プレイヤー操作停止
        //  後で入れ込む

        // UIを非表示（移動開始前）
        if (gachaUI != null)
            gachaUI.SetActive(false);

        // カメラ移動開始
        if (cameraManager != null && cameraTargetPoint != null) {
            cameraManager.MoveCamera(
                player,
                cameraTargetPoint.position,
                cameraTargetPoint.rotation
            );
        }

        // 移動完了後にUIを表示する場合は、
        // CameraChangeControllerのコルーチンが終わったタイミングで呼ぶか
        // ここで遅延コルーチンを追加しても良い
        if (gachaUI != null)
            StartCoroutine(ShowUIAfterDelay(cameraManager));
    }

    /// <summary>
    /// キャラクター選択モードを終了
    /// </summary>
    public void EndGachaSelect() {

        if (currentPlayer == null) return;

        //  アニメーションをOffにする
        OffGachaAnim();

        // UIを非表示（戻る操作開始時）
        if (gachaUI != null)
            gachaUI.SetActive(false);

        // カメラを戻す
        if (cameraManager != null)
            cameraManager.ReturnCamera();

        currentPlayer = null;
    }

    /// <summary>
    /// 遅延してUIを表示（カメラ移動完了後にUIを表示する補助）
    /// </summary>
    private System.Collections.IEnumerator ShowUIAfterDelay(CameraChangeController camController) {
        // CameraChangeController の移動時間と同じだけ待つ
        float duration = camController != null ? camController.moveDuration : 1.5f;
        yield return new WaitForSeconds(duration);

        if (gachaUI != null)
            gachaUI.SetActive(true);
    }
    #endregion

    private void OnGachaAnim() {
        //  アニメーション追加
        gachaAnim.SetBool("Open", true);
    }

    private void OffGachaAnim() {
        //  アニメーション追加
        gachaAnim.SetBool("Open", false);
    }

    private IEnumerator PlayGachaAnimation() {
        OffGachaAnim();
        yield return null;
        OnGachaAnim();
    }

}