using UnityEngine;
using System.Collections.Generic;

public class AreaSpawnPoint : MonoBehaviour {

    [Header("このエリアの突破条件")]
    public float targetScore = 10f;

    [Header("突破時に実行するイベント（Prefab）")]
    public List<PVEStageEvent> eventPrefabs = new();
}