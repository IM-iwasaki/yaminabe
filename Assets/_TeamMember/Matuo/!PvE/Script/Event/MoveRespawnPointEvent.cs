using UnityEngine;
using Mirror;

public class MoveRespawnPointEvent : PVEStageEvent {
    [Header("‚±‚ÌƒGƒŠƒA“Ë”j‚ÌˆÚ“®æ")]
    [SerializeField] private Transform moveDestination;

    protected override void Execute() {
        if (!isServer) return;
        if (moveDestination == null) return;

        if (RespawnPoint.Instance != null) {
            RespawnPoint.Instance.MoveTo(moveDestination);
        }
    }
}