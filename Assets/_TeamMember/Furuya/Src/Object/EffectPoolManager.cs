using System.Collections.Generic;
using UnityEngine;

public class EffectPoolManager : MonoBehaviour {
    public static EffectPoolManager Instance;
    private Dictionary<GameObject, Queue<GameObject>> pools = new();

    void Awake() {
        Instance = this;
    }

    public GameObject GetFromPool(GameObject prefab, Vector3 pos, Quaternion rot) {
        if (!pools.ContainsKey(prefab)) pools[prefab] = new Queue<GameObject>();

        GameObject obj;
        if (pools[prefab].Count > 0) {
            obj = pools[prefab].Dequeue();
            obj.SetActive(true);
        }
        else {
            obj = Instantiate(prefab);
        }

        obj.transform.SetPositionAndRotation(pos, rot);
        return obj;
    }

    public void ReturnToPool(GameObject obj, float delay = 0f) {
        StartCoroutine(ReturnAfterDelay(obj, delay));
    }

    private System.Collections.IEnumerator ReturnAfterDelay(GameObject obj, float delay) {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);

        foreach (var kvp in pools) {
            if (kvp.Key.name == obj.name.Replace("(Clone)", "").Trim()) {
                pools[kvp.Key].Enqueue(obj);
                yield break;
            }
        }
    }
}
