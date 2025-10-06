using UnityEngine;
using System.Linq;
using System.Collections;

/// <summary>
/// TPの種類をEnumで管理
/// Random: シーン内の他のTPオブジェクトにランダムでワープ
/// Pair: Inspectorで設定したリンク先にワープ
/// </summary>
public enum TPType {
    Random,
    Pair
}

/// <summary>
/// TPS用テレポートポイント
/// - 前滞在(preDelay)
/// - ワープ(TP)
/// - 後硬直(postDelay)
/// - 全体クールタイム管理(cooldown)
/// </summary>
public class TeleportPoint : MonoBehaviour {
    [Header("＝＝＝テレポート設定＝＝＝")]
    [Header("TPタイプ（ランダムか二点間か）")]
    public TPType tpType = TPType.Random;       // TPタイプ（Random or Pair）
    [Header("二点間の際のTP先(Randomの場合は何も入れない)")]
    public TeleportPoint linkedPoint = null;    // ペアTPのリンク先
    [Header("クールタイム")]
    public float cooldown = 2f;                 // 全体クールタイム（秒）
    [Header("TP前の滞在時間")]
    public float preDelay = 3f;                 // TP前滞在時間（秒）
    [Header("TP後の硬直時間")]
    public float postDelay = 0.5f;              // TP後硬直時間（秒）

    private float lastTPTime = -Mathf.Infinity; // 最後にTPした時間を記録（全体管理）
    private bool isTeleporting = false;         // ワープ中フラグ（同時ワープ防止）

    /// <summary>
    /// ワープ開始トリガー
    /// - 全体クールタイム中またはワープ中は何もしない
    /// </summary>
    /// <param name="player">ワープ対象のプレイヤーオブジェクト</param>
    public void TryTeleport(GameObject player) {
        if (player == null) return; // nullチェック

        // 全体クールタイムまたは現在ワープ中なら処理しない
        if (Time.time - lastTPTime < cooldown || isTeleporting) return;

        // Coroutineで前滞在 → TP → 後硬直を順番に処理
        StartCoroutine(TeleportCoroutine(player));
    }

    /// <summary>
    /// ワープ処理コルーチン
    /// </summary>
    /// <param name="player">ワープ対象のプレイヤーオブジェクト</param>
    private IEnumerator TeleportCoroutine(GameObject player) {
        isTeleporting = true; // ワープ中フラグON

        // TP前滞在時間待機
        yield return new WaitForSeconds(preDelay);

        TeleportPoint target = null; // ワープ先を格納する変数

        switch (tpType) {
            case TPType.Random:
                // 自分自身以外の全TPオブジェクトを取得
                var points = FindObjectsOfType<TeleportPoint>()
                                .Where(tp => tp != this)
                                .ToArray();

                // 対象がいなければ警告を出して終了
                if (points.Length == 0) {
                    Debug.LogWarning("Random TP対象が存在しません！");
                    isTeleporting = false;
                    yield break;
                }

                // ランダムで1つ選択
                target = points[Random.Range(0, points.Length)];
                break;

            case TPType.Pair:
                // リンク先が設定されていなければ警告を出して終了
                if (linkedPoint == null) {
                    Debug.LogWarning($"Pair TPのリンク先が未設定: {gameObject.name}");
                    isTeleporting = false;
                    yield break;
                }

                // リンク先をターゲットに設定
                target = linkedPoint;
                break;
        }

        if (target != null) {
            // ワープ実行（少し浮かせて着地）
            player.transform.position = target.transform.position + Vector3.up;

            // TP後硬直時間待機
            yield return new WaitForSeconds(postDelay);

            // 全体クールタイムを更新
            lastTPTime = Time.time;

            // ここでパーティクルやSEなどワープ演出を追加可能

        }

        // ワープ処理完了、フラグOFF
        isTeleporting = false;
    }
}
