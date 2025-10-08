using UnityEngine;

public class ItemRotation : MonoBehaviour {
    [Header("跳ねるスピード"), SerializeField] private float bounceSpeed = 8;
    [Header("跳ねる幅"), SerializeField] private float bounceAmplitude = 0.05f;
    [Header("回るスピード"), SerializeField] private float rotationSpeed = 90;

    private float startHeight;
    private float timeOffset;

    void Start() {
        startHeight = transform.localPosition.y;
        timeOffset = Random.value * Mathf.PI * 2;
    }

    void Update() {
        //バウンドしながら回転
        float finalheight = startHeight + Mathf.Sin(Time.time * bounceSpeed + timeOffset) * bounceAmplitude;
        var position = transform.localPosition;
        position.y = finalheight;
        transform.localPosition = position;

        Vector3 rotation = transform.localRotation.eulerAngles;
        rotation.y += rotationSpeed * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
    }
}