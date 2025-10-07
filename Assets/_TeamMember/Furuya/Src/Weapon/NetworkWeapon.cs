using UnityEngine;
using Mirror;
using Mirror.Examples.Tanks;

public class NetworkWeapon : NetworkBehaviour {
    public WeaponData weaponData;
    public Transform firePoint;

    [Command] // クライアントからサーバーへ「攻撃して！」を送る
    public void CmdRequestAttack() {
        if (!CanAttack()) return;
        lastAttackTime = Time.time;
        if (weaponData.type == WeaponType.Melee)
            ServerMeleeAttack();
        else if (weaponData.type == WeaponType.Gun)
            ServerRangedAttack();
    }

    float lastAttackTime;
    bool CanAttack() => Time.time >= lastAttackTime + weaponData.cooldown;

    void ServerMeleeAttack() {
#if UNITY_EDITOR
        // 攻撃範囲の可視化（1秒表示）
        DrawDebugSphere(firePoint.position, weaponData.range, Color.red, 1f);
#endif
        Collider[] hitBuffer = new Collider[16]; // 同時に当たる最大数を想定
        int hitCount = Physics.OverlapSphereNonAlloc(firePoint.position, weaponData.range, hitBuffer);

        for (int i = 0; i < hitCount; i++) {
            var c = hitBuffer[i];
            var hp = c.GetComponent<CharacterBase>();
            if (hp != null && IsValidTarget(hp.gameObject)) {
                hp.TakeDamage(weaponData.damage);
                RpcSpawnHitEffect(c.transform.position);
            }
        }
    }
    void DrawDebugSphere(Vector3 center, float radius, Color color, float duration = 0.5f) {
        int segments = 24;
        Vector3 prevPoint = center + new Vector3(0, 0, radius);
        for (int i = 1; i <= segments; i++) {
            float angle = i * Mathf.PI * 2 / segments;
            Vector3 nextPoint = center + new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius);
            Debug.DrawLine(prevPoint, nextPoint, color, duration);
            prevPoint = nextPoint;
        }
    }

    void ServerRangedAttack() {
        var proj = Instantiate(weaponData.projectilePrefab, firePoint.position, firePoint.rotation);
        var netObj = proj.GetComponent<NetworkIdentity>();
        NetworkServer.Spawn(proj); // 全クライアントにプロジェクトル生成を通知
        var pb = proj.GetComponent<Projectile>();
        if (pb != null) pb.Init(weaponData.damage, weaponData.projectileSpeed);
    }

    bool IsValidTarget(GameObject target) {
        // チーム判定や味方判定をここで行う
        return true;
    }

    [ClientRpc]
    void RpcSpawnHitEffect(Vector3 pos) {
        if (weaponData.hitEffectPrefab) Instantiate(weaponData.hitEffectPrefab, pos, Quaternion.identity);
    }
}
