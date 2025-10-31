using UnityEngine;

public class EffectPoolRegistry : MonoBehaviour {
    public static EffectPoolRegistry Instance;

    [System.Serializable]
    public struct EffectMapping { public EffectType type; public GameObject prefab; }
    public EffectMapping[] hitEffects;
    public EffectMapping[] muzzleFlashes;

    void Awake() { Instance = this; }

    public GameObject GetHitEffect(EffectType type) {
        foreach (var e in hitEffects) if (e.type == type) return e.prefab;
        return null;
    }

    public GameObject GetMuzzleFlash(EffectType type) {
        foreach (var e in muzzleFlashes) if (e.type == type) return e.prefab;
        return null;
    }
}
