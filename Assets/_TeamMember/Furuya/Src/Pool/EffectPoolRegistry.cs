using UnityEngine;

/// <summary>
/// 武器エフェクト用のプール
/// Typeでエフェクトを返す
/// </summary>
public class EffectPoolRegistry : MonoBehaviour {
    public static EffectPoolRegistry Instance;

    [System.Serializable]
    public struct EffectMapping { public EffectType type; public GameObject prefab; }
    public EffectMapping[] hitEffects;
    public EffectMapping[] muzzleFlashes;

    void Awake() { Instance = this; }

    /// <summary>
    /// ヒットエフェクトを返す
    /// </summary>
    public GameObject GetHitEffect(EffectType type) {
        foreach (var e in hitEffects) if (e.type == type) return e.prefab;
        return null;
    }

    /// <summary>
    /// マズルフラッシュを返す
    /// </summary>
    public GameObject GetMuzzleFlash(EffectType type) {
        foreach (var e in muzzleFlashes) if (e.type == type) return e.prefab;
        return null;
    }
}
