public class RemoveObjectEvent : PVEStageEvent {

    protected override void Execute() {
        gameObject.SetActive(false);
    }
}