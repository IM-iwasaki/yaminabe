using UnityEngine;

/// <summary>
/// お金増やす用(仮)
/// </summary>
public class MoneyTrigger : MonoBehaviour {
    [Header("増える金額")]
    [SerializeField] private int addAmount = 100;

    [Header("反応するタグ")]
    [SerializeField] private string targetTag = "Player";

    private void OnTriggerEnter(Collider other) {
        // プレイヤーだけ反応
        if (!other.CompareTag(targetTag)) return;

        // PlayerWallet が存在すればお金を追加
        if (PlayerWallet.Instance != null) {
            PlayerWallet.Instance.AddMoney(addAmount);
        }
    }
}
