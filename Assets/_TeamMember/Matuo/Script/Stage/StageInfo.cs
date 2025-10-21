using UnityEngine;

[System.Serializable]
public class StageInfo {
    [Header("ステージ名")]
    public string stageName;
    [Header("ステージプレハブ（NetworkIdentity付けて）")]
    public GameObject stagePrefab;
}