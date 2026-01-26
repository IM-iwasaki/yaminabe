using Mirror;
using UnityEngine;

public class AssetIdFinder : MonoBehaviour {
    // エラーログに出ている値を入れる
    public uint targetAssetId = 3465884604;

    void Start() {
        foreach (var ni in Resources.FindObjectsOfTypeAll<NetworkIdentity>()) {
            if (ni.assetId == targetAssetId) {
                Debug.Log($"FOUND: {ni.name}  assetId={ni.assetId}", ni.gameObject);
            }
        }
    }
}
