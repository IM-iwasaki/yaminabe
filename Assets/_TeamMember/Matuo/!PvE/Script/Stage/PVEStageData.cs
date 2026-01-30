using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PVEStageData",menuName = "ScriptableObject/Stage/PVEStageData")]
public class PVEStageData : ScriptableObject {

    [Header("ステージ名")]
    public string stageName;

    [Header("ステージプレハブ（NetworkIdentity必須）")]
    public GameObject stagePrefab;

    [Header("このステージで生成するルールオブジェクト")]
    public List<GameRuleType> rulesToSpawn = new();

    [Header("制限時間（0なら無制限）")]
    public float timeLimit = 180f;
}