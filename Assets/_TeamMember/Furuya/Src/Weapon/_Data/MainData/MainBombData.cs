using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Weapons/Bomb")]
public class MainBombData : WeaponData {
    [Header("Bomb Settings")]
    public GameObject bombPrefab;
    public float explodeTime = 3f;

    public bool damageSelf = false;
    public bool damageAlly = false;
    public bool chainReaction = true;

    public ExplosionType explosionType = ExplosionType.Center;

    [Header("Center Explosion")]
    public float explosionRange = 3f;

    [Header("Cross Explosion")]
    public float maxDistance = 5f;
    public float interval = 1f;
    public float delayBetween = 0.05f;
    public LayerMask wallLayer;

    [Header("Visual")]
    public Color safeColor = Color.white;
    public Color dangerColor = Color.red;
}
