using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class CollectPoint : NetworkBehaviour {
    [Header("集めるホコの必要数")]
    public int requiredHokoCount = 3;

    [Header("成功時に発動するイベント")]
    public PVEStageEvent onCollectedEvent;

    private HashSet<CaptureHokoPVE> collectedHokos = new();

    private void OnTriggerEnter(Collider other) {
        var hoko = other.GetComponent<CaptureHokoPVE>();
        if (hoko != null && !collectedHokos.Contains(hoko)) {
            collectedHokos.Add(hoko);

            // プレイヤーが持っている場合はホコを置く
            if (hoko.holder != null)
                hoko.Drop();

            if (collectedHokos.Count >= requiredHokoCount) {
                ExecuteEvent();
            }
        }
    }

    [Server]
    private void ExecuteEvent() {
        onCollectedEvent?.Execute();

        foreach (var h in collectedHokos) {
            if (h != null)
                NetworkServer.Destroy(h.gameObject);
        }

        collectedHokos.Clear();
    }
}