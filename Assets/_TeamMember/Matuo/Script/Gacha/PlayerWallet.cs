using UnityEngine;
using System;

/// <summary>
/// プレイヤーのお金を管理するクラス
/// シングルトン構造でゲーム全体から参照可能
/// </summary>
public class PlayerWallet : MonoBehaviour {
    // シングルトン
    public static PlayerWallet Instance { get; private set; }

    [Header("初期設定")]
    [SerializeField] private int startMoney = 0;

    [Header("現在の所持金")]
    [SerializeField] private int currentMoney;

    /// <summary>
    /// 所持金が変化したときに呼ばれるイベント
    /// </summary>
    public event Action<int> OnMoneyChanged;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadMoney();
    }

    /// <summary>
    /// 現在の所持金を取得
    /// </summary>
    public int GetMoney() => currentMoney;

    /// <summary>
    /// お金を追加する（マイナスも可）
    /// </summary>
    /// <param name="amount">追加する金額（負数で減算）</param>
    public void AddMoney(int amount) {
        currentMoney += amount;
        if (currentMoney < 0) currentMoney = 0;
        OnMoneyChanged?.Invoke(currentMoney);
        SaveMoney();
    }

    /// <summary>
    /// 指定した金額を支払う
    /// </summary>
    /// <param name="amount">支払う金額</param>
    /// <returns>成功したらtrue</returns>
    public bool SpendMoney(int amount) {
        // 金額が無効な場合(マイナスなど)
        if (amount <= 0) return false;
        // お金が足りない場合
        if (currentMoney < amount) return false;

        currentMoney -= amount;
        OnMoneyChanged?.Invoke(currentMoney);
        SaveMoney();
        Debug.Log($"{amount} 円支払った, 残高: {currentMoney}");
        return true;
    }

    /// <summary>
    /// 所持金のリセット
    /// </summary>
    public void ResetMoney() {
        currentMoney = startMoney;
        OnMoneyChanged?.Invoke(currentMoney);
        SaveMoney();
    }

    /// <summary>
    /// 所持金のセーブ
    /// </summary>
    private void SaveMoney() {
        // 既存データをロードして所持金だけ更新
        var data = PlayerSaveData.Load();
        data.currentMoney = currentMoney;
        PlayerSaveData.Save(data);
    }

    /// <summary>
    /// セーブされている所持金のロード
    /// </summary>
    private void LoadMoney() {
        var data = PlayerSaveData.Load();
        currentMoney = data.currentMoney > 0 ? data.currentMoney : startMoney;
        OnMoneyChanged?.Invoke(currentMoney);
    }
}