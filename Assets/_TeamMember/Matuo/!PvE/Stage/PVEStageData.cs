using UnityEngine;

[CreateAssetMenu(fileName = "PVEStageData",menuName = "ScriptableObject/Stage/PVEStageData")]
public class PVEStageData : ScriptableObject {

    [Header("ステージ名")]
    public string stageName;

    [Header("ステージプレハブ（NetworkIdentity必須）")]
    public GameObject stagePrefab;

    [Header("使用するルール")]
    public GameRuleType rule;

    [Header("勝利条件スコア")]
    public float targetScore = 50f;

    [Header("制限時間（0なら無制限）")]
    public float timeLimit = 180f;
}