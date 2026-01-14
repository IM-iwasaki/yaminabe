using UnityEngine;

/// <summary>
/// エフェクトの前方移動用 古谷
/// </summary>

public class EffectForwardMove : MonoBehaviour {
    [SerializeField] private float speed = 3f;
    [SerializeField] private float lifeTime = 5f;

    private void Start() {
        Destroy(gameObject, lifeTime);
    }

    private void Update() {
        transform.position += transform.forward * speed * Time.deltaTime;
    }
}
