using UnityEngine;

public class ExplosiveObject : MonoBehaviour {
    [Header("爆発設定")]
    //public GameObject explosionEffectPrefab; // 爆発エフェクトのプレハブ
    public float explosionRadius = 5f;       // 爆発範囲
    public float explosionForce = 700f;      // 爆風の力
   

    private bool hasExploded = false;        // 二重爆発防止

    private void OnCollisionEnter(Collision collision) {
        // 一度だけ爆発
        if (hasExploded) return;
        hasExploded = true;

        Explode();
    }

    void Explode() {
        

        // 爆発範囲内のすべてのコライダーを取得
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearby in colliders) {
            Rigidbody rb = nearby.attachedRigidbody;
            if (rb != null) {
                // 爆風の力を与える
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            // Playerタグを持つオブジェクトだけ処理
            if (nearby.CompareTag("Player")) {
                // ダメージが処理などをかける
                Debug.Log("爆発");

            }
        }

        // 自身を削除
        Destroy(gameObject);
    }

    // 爆発範囲をシーン上で可視化
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
