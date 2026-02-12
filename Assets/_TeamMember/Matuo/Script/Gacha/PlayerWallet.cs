using UnityEngine;
using TMPro;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using Mirror;

/// <summary>
/// プレイヤーの所持金を管理するクラス
/// </summary>
public class PlayerWallet : MonoBehaviour {
    // シングルトン
    public static PlayerWallet Instance { get; private set; }

    [Header("初期設定")]
    [SerializeField] private int startMoney = 0;

    [Header("現在の所持金")]
    [SerializeField] public int currentMoney;

    /// <summary>
    /// 所持金が変化した時に通知されるイベント
    /// </summary>
    public event Action<int> OnMoneyChanged;

    [Header("所持金UI（Prefab）")]
    [SerializeField] private GameObject moneyUIPrefab; // Inspectorで設定する

    // 生成されたUIの参照
    private Canvas moneyCanvas;
    private TextMeshProUGUI moneyText;

    // 試合中に消費した総額
    public int matchSpentMoney = 0;

    /// <summary>
    /// 常時表示するかどうか（Lobbyなど）
    /// </summary>
    private bool keepMoneyUIVisible = false;

    private void Awake() {
        // シングルトン処理
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // セーブデータから所持金をロード
        LoadMoney();

        // シーンロードイベント登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() {
        // イベント解除
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// シーンロード時の処理
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "LobbyScene") {
            // Lobbyでは常時表示
            ShowMoneyUI();
            keepMoneyUIVisible = true;
        } else {
            // それ以外のシーンでは非表示
            keepMoneyUIVisible = false;
            HideMoneyUI();
        }
    }

    /// <summary>
    /// 現在の所持金を取得
    /// </summary>
    public int GetMoney() => currentMoney;

    /// <summary>
    /// 所持金を増減させる（マイナス可）
    /// </summary>
    public void AddMoney(int amount) {
        currentMoney += amount;
        if (currentMoney < 0)
            currentMoney = 0;

        OnMoneyChanged?.Invoke(currentMoney);
        SaveMoney();

        UpdateMoneyText();
        ShowFloatingMoney(amount);
    }

    /// <summary>
    /// 指定金額を支払う
    /// </summary>
    public bool SpendMoney(int amount) {
        if (amount <= 0) return false;
        if (currentMoney < amount) return false;

        currentMoney -= amount;
        if (currentMoney < 0)
            currentMoney = 0;

        OnMoneyChanged?.Invoke(currentMoney);
        SaveMoney();

        UpdateMoneyText();
        ShowFloatingMoney(-amount);

        if (GameManager.Instance.IsGameRunning())
            matchSpentMoney += amount;

        return true;
    }

    /// <summary>
    /// 所持金を初期値にリセット
    /// </summary>
    public void ResetMoney() {
        currentMoney = startMoney;
        OnMoneyChanged?.Invoke(currentMoney);
        SaveMoney();
        UpdateMoneyText();
    }

    /// <summary>
    /// 試合開始時に試合中に消費した金額をリセット
    /// </summary>
    public void ResetMatchSpentMoney() {
        matchSpentMoney = 0;
    }

    /// <summary>
    /// 勝利時に1.2倍にして返す。
    /// </summary>
    /// <param name="multiplier"></param>
    public void RefundSpentMoney(float multiplier = 1.2f) {
        int refund = Mathf.FloorToInt(matchSpentMoney * multiplier);
        AddMoney(refund);
        matchSpentMoney = 0;
    }


    /// <summary>
    /// 所持金をセーブ
    /// </summary>
    private void SaveMoney() {
        var data = PlayerSaveData.Load();
        data.currentMoney = currentMoney;
        PlayerSaveData.Save(data);
    }

    /// <summary>
    /// 所持金をロード
    /// </summary>
    private void LoadMoney() {
        var data = PlayerSaveData.Load();
        currentMoney = data.currentMoney;
        OnMoneyChanged?.Invoke(currentMoney);
    }

    /// <summary>
    /// 所持金UIを表示
    /// </summary>
    public void ShowMoneyUI() {
        CreateMoneyUI();
        keepMoneyUIVisible = true;
        UpdateMoneyText();

        if (moneyText != null)
            moneyText.gameObject.SetActive(true);
    }

    /// <summary>
    /// 所持金UIを非表示
    /// </summary>
    public void HideMoneyUI() {
        keepMoneyUIVisible = false;

        if (moneyText != null)
            moneyText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 所持金UIをプレハブから生成
    /// </summary>
    private void CreateMoneyUI() {
        // 既に生成済みなら何もしない
        if (moneyCanvas != null) return;

        if (moneyUIPrefab == null) {
            Debug.LogError("MoneyUIPrefab が Inspector に設定されていません");
            return;
        }

        // プレハブ生成
        GameObject ui = Instantiate(moneyUIPrefab);
        DontDestroyOnLoad(ui);

        // 参照取得
        moneyCanvas = ui.GetComponentInChildren<Canvas>();
        moneyText = ui.GetComponentInChildren<TextMeshProUGUI>();

        if (moneyText == null)
            Debug.LogError("MoneyText (TextMeshProUGUI) がプレハブ内に存在しません");

        // 初期状態は非表示
        if (moneyText != null)
            moneyText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 所持金テキストを更新
    /// </summary>
    private void UpdateMoneyText() {
        if (moneyText == null) return;
        moneyText.text = $"{currentMoney} G";
    }

    #region 増減UI
    /// <summary>
    /// 所持金増減時のフローティング表示
    /// </summary>
    private void ShowFloatingMoney(int amount) {
        if (moneyText == null) return;

        // フローティング用Text生成
        GameObject floatGO = new GameObject("FloatingMoneyText");
        floatGO.transform.SetParent(moneyText.transform.parent, false);

        TextMeshProUGUI floatText = floatGO.AddComponent<TextMeshProUGUI>();
        floatText.fontSize = moneyText.fontSize;
        floatText.alignment = TextAlignmentOptions.TopLeft;

        string sign = amount > 0 ? "+" : "";
        floatText.text = $"{sign}{amount}";
        floatText.color = amount > 0 ? Color.yellow : Color.red;

        RectTransform rt = floatText.rectTransform;

        // 所持金テキストの右側に表示
        float offsetX = moneyText.preferredWidth + 20f;
        rt.anchorMin = moneyText.rectTransform.anchorMin;
        rt.anchorMax = moneyText.rectTransform.anchorMax;
        rt.pivot = moneyText.rectTransform.pivot;
        rt.anchoredPosition =
            moneyText.rectTransform.anchoredPosition + new Vector2(offsetX, -20f);

        StartCoroutine(FloatingAnimation(floatText));
    }

    /// <summary>
    /// フローティングアニメーション（上昇＋フェードアウト）
    /// </summary>
    private IEnumerator FloatingAnimation(TextMeshProUGUI text) {
        float duration = 1.5f;
        float timer = 0f;

        Vector2 startPos = text.rectTransform.anchoredPosition;
        Color startColor = text.color;

        while (timer < duration) {
            timer += Time.deltaTime;
            float t = timer / duration;

            // 上方向へ移動
            text.rectTransform.anchoredPosition =
                startPos + Vector2.up * 60f * t;

            // フェードアウト
            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            text.color = c;

            yield return null;
        }

        Destroy(text.gameObject);
    }
    #endregion
}