using UnityEngine;
using System.Collections.Generic;

public enum AreaClearCondition {
    AnyPlayer,   // 1人いればOK
    AllPlayers   // 全員必要
}

public class AreaSpawnPoint : MonoBehaviour {

    [Header("このエリアの突破条件")]
    public float targetScore = 10f;

    [Header("カウントを進めるのに必要な人数")]
    public AreaClearCondition clearCondition = AreaClearCondition.AnyPlayer;

    [Header("突破時に実行するイベント")]
    public List<PVEStageEvent> events = new();
}