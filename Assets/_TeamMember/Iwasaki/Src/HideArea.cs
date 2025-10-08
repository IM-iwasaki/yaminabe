using UnityEngine;

public class HideArea : MonoBehaviour {
    // プレイヤーがエリアに入ったとき
    private void OnTriggerEnter(Collider other) {
        Debug.Log("隠れる");
        // Playerタグを持つオブジェクトだけ処理
        if (other.CompareTag("Player")) {
            // PlayerHide コンポーネントを取得
            var hide = other.GetComponent<PlayerHide>();
            if (hide != null) {
                hide.SetHidden(true); // 隠れる状態にする
            }
        }
    }

    // プレイヤーがエリアから出たとき
    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            var hide = other.GetComponent<PlayerHide>();
            if (hide != null) {
                hide.SetHidden(false); // 元の表示状態に戻す
            }
        }
    }
}
