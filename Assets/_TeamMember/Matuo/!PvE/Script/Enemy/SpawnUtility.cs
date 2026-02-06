using UnityEngine;
using Mirror;

/// <summary>
/// スポーン判定用の静的ユーティリティ
/// </summary>
public static class SpawnUtility {

    /// <summary>
    /// プレイヤーが指定距離内に存在するか
    /// </summary>
    public static bool IsAnyPlayerInRange(Vector3 pos, float radius) {

        foreach (var conn in NetworkServer.connections.Values) {
            if (conn.identity == null) continue;

            var player = conn.identity.GetComponent<CharacterBase>();
            if (player == null) continue;

            float dist = Vector3.Distance(player.transform.position, pos);
            if (dist <= radius)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 全プレイヤーの視界外なら true
    /// </summary>
    public static bool CanSpawnOutOfPlayerView(Vector3 spawnPos) {

        foreach (var conn in NetworkServer.connections.Values) {
            if (conn.identity == null) continue;

            var player = conn.identity.GetComponent<CharacterBase>();
            if (player == null) continue;

            if (IsInView(player, spawnPos) &&
                HasLineOfSight(player, spawnPos)) {
                // 誰か1人にでも見られていたらスポーンできない
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// プレイヤーの画面内に入っているか
    /// </summary>
    private static bool IsInView(CharacterBase player, Vector3 pos) {

        Camera cam = player.GetComponentInChildren<Camera>();
        if (cam == null) return false;

        Vector3 viewport = cam.WorldToViewportPoint(pos);

        return viewport.z > 0 &&
               viewport.x > 0 && viewport.x < 1 &&
               viewport.y > 0 && viewport.y < 1;
    }

    /// <summary>
    /// プレイヤーとスポーン地点が直線で見通せるか
    /// </summary>
    private static bool HasLineOfSight(CharacterBase player, Vector3 pos) {

        Vector3 eyePos = player.transform.position + Vector3.up * 1.6f;
        Vector3 dir = (pos - eyePos).normalized;

        if (Physics.Raycast(eyePos, dir, out RaycastHit hit, 50f)) {
            // 最初に当たった場所がスポーン地点付近なら見えている判定
            return Vector3.Distance(hit.point, pos) < 1.0f;
        }
        return false;
    }
}