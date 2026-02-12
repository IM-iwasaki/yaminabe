using UnityEngine;

public class RemoveObjectEvent : PVEStageEvent {
    public override void Execute() {
        gameObject.SetActive(false);
    }
}