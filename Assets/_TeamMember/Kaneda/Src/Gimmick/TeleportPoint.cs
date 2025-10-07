using UnityEngine;
using System.Collections;
using System.Linq;

/// <summary>
/// テレポートタイプ（Enum）
/// Random: ランダムな他のTPポイントへ移動
/// Pair: ペア指定のTPポイントへ移動
/// </summary>
public enum TPType {
    Random,
    Pair
}

/// <summary>
/// テレポートポイント制御クラス
/// - プレイヤーが3秒間トリガー内に留まるとワープ
/// - ワープ前後に待機時間を設ける
/// - 全体クールタイムで連続ワープを防止
/// </summary>
[RequireComponent(typeof(Collider))]
public class TeleportPoint : MonoBehaviour {
    [Header("＝＝＝テレポート設定＝＝＝")]
    [Header("TPタイプ（ランダム or ペア）")]
    public TPType tpType = TPType.Random;          // ランダム or ペア
    [Header("ペア指定時のTP先（ランダム時はnull）")]
    public TeleportPoint linkedPoint = null;       // ペア先
    [Header("TPポイント全体のタグ名（ランダムTPで使用）")]
    public string tpTag = "Teleport";              // 全TP共通タグ
    [Header("TPできるまでクールタイム（秒）")]
    public float cooldown = 2f;                    // 全TP共通のクールタイム
    [Header("TP前滞在時間（秒）")]
    public float preDelay = 3f;                    // ワープまでの滞在時間
    [Header("TP後硬直時間（秒）")]
    public float postDelay = 0.5f;                 // ワープ後の硬直時間

    // 内部状態管理
    private float lastTPTime = -Mathf.Infinity; // 全体クールタイム管理（全インスタンス共通）
    private bool isTeleporting = false;                // ワープ中フラグ
    private Coroutine stayCoroutine = null;            // 滞在監視コルーチン
    private GameObject currentPlayer = null;           // 現在トリガー内のプレイヤー参照

    // --- プレイヤーがトリガー内に入った時 ---
    private void OnTriggerEnter(Collider other) {
        // Playerタグで判定
        if (!other.CompareTag("Player")) return;

        // 既に監視中またはワープ中なら無視
        if (stayCoroutine != null || isTeleporting) return;

        // 現在のプレイヤーを記録
        currentPlayer = other.gameObject;

        // 一定時間留まったかを監視開始
        stayCoroutine = StartCoroutine(StayCheckCoroutine(currentPlayer));
    }

    // --- プレイヤーがトリガー外に出た時 ---
    private void OnTriggerExit(Collider other) {
        // Playerタグでない場合は無視
        if (!other.CompareTag("Player")) return;

        // 離脱したプレイヤーが現在監視中のプレイヤーであればキャンセル
        if (other.gameObject == currentPlayer) {
            // 滞在監視を停止
            if (stayCoroutine != null) {
                StopCoroutine(stayCoroutine);
                stayCoroutine = null;
            }
            currentPlayer = null;
        }
    }

    /// <summary>
    /// 一定時間留まっていたか監視するコルーチン
    /// </summary>
    private IEnumerator StayCheckCoroutine(GameObject player) {
        // 滞在時間計測
        float timer = 0f;
        while (timer < preDelay) {
            // プレイヤーが離れたら中断
            if (currentPlayer == null) yield break;

            // 経過時間を加算
            timer += Time.deltaTime;
            yield return null;
        }

        // 一定時間滞在した場合のみワープを実行
        TryTeleport(player);

        // 監視コルーチン終了
        stayCoroutine = null;
    }

    /// <summary>
    /// ワープ開始処理（クールタイム・状態チェック含む）
    /// </summary>
    private void TryTeleport(GameObject player) {
        if (player == null) return;

        // 全体クールタイムまたは現在ワープ中なら無視
        if (Time.time - lastTPTime < cooldown || isTeleporting) return;

        // ワープ処理開始
        StartCoroutine(TeleportCoroutine(player));
    }

    /// <summary>
    /// 実際のワープ処理を行うコルーチン
    /// </summary>
    private IEnumerator TeleportCoroutine(GameObject player) {
        isTeleporting = true; // フラグON

        TeleportPoint target = null; // ワープ先

        switch (tpType) {
            case TPType.Random:
                // 指定タグを持つ全オブジェクトを取得
                var objs = GameObject.FindGameObjectsWithTag(tpTag)
                                     .Select(go => go.GetComponent<TeleportPoint>())
                                     .Where(tp => tp != null && tp != this)
                                     .ToArray();

                // 対象が存在しない場合は警告を出して中断
                if (objs.Length == 0) {
                    Debug.LogWarning($"[{name}] ランダムTP対象が見つかりません！");
                    isTeleporting = false;
                    yield break;
                }

                // ランダムに1つ選択
                target = objs[Random.Range(0, objs.Length)];
                break;

            case TPType.Pair:
                // ペア指定がない場合は中断
                if (linkedPoint == null) {
                    Debug.LogWarning($"[{name}] ペアTP先が設定されていません！");
                    isTeleporting = false;
                    yield break;
                }
                target = linkedPoint;
                break;
        }

        // 実際にTPを行う
        if (target != null) {
            // 少し浮かせて配置
            player.transform.position = target.transform.position + Vector3.up;

            // 全体クールタイムを更新
            lastTPTime = Time.time;

            // ワープ後硬直
            yield return new WaitForSeconds(postDelay);
        }

        // 状態リセット
        isTeleporting = false;
    }
}
