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
    public EffectMapping[] deathEffects;
    public EffectMapping[] chargeEffects;

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

    /// <summary>
    /// 死亡エフェクトを返す
    /// </summary>
    public GameObject GetDeathEffect(EffectType type) {
        foreach (var e in deathEffects) if (e.type == type) return e.prefab;
        return null;
    }

    /// <summary>
    /// チャージエフェクトを返す
    /// </summary>
    public GameObject GetChargeEffect(EffectType type) {
        foreach (var e in chargeEffects) if (e.type == type) return e.prefab;
        return null;
    }
}
