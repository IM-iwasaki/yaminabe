using UnityEngine;

[CreateAssetMenu(menuName = "Game/Weapon")]
public class WeaponData : ScriptableObject {
    public string weaponName;
    public Sprite icon;
    public WeaponType type;
    public int damage = 10;
    public float cooldown = 0.5f;
    public float range = 2f; // ‹ßÚ‚Íg‚¤‚µA‰“Šu‚ÍË’ö
    public GameObject projectilePrefab; // ‰“Šu—p
    public float projectileSpeed = 20f;
    public AudioClip swingSfx;
    public GameObject hitEffectPrefab;
}