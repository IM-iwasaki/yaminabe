public class RemoveObjectEvent : PVEStageEvent {
    protected override void Execute() {
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
    }
}