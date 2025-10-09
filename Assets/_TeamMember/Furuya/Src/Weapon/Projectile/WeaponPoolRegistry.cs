using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPoolRegistry : MonoBehaviour {
    public static WeaponPoolRegistry Instance;
    public GameObject hitEffect;
    public GameObject muzzleEffect;

    private void Awake() {
        Instance = this;
    }
}
