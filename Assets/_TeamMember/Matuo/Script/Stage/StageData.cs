using UnityEngine;

/// <summary>
/// ステージ情報を保持する ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "StageData", menuName = "ScriptableObject/Stage/StageData")]
public class StageData : ScriptableObject {
    [Header("ステージ名")]
    public string stageName;

    [Header("ステージプレハブ（NetworkIdentity付けて）")]
    public GameObject stagePrefab;
}