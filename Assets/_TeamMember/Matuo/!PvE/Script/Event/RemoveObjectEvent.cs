

//  追加（気に食わなかったら好きに書き換えて）
using UnityEngine;

public enum EventWallType {
    Halfway,    //  中間
    Goal        //  ゴール
}

public class RemoveObjectEvent : PVEStageEvent {

    [Header("カウントが完了した際に通知する\n" +
        "壁のタイプを選んで通知を分ける\n" +
        "Halfway->中間、Goal->ゴール")]
    public EventWallType eventWallType;

    protected override void Execute() {
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
        SetNotification();
    }

    /// <summary>
    /// 通知を送る
    /// </summary>
    private void SetNotification() {

        PVENotificationManager text = PVENotificationManager.Instance;
        if(text == null) return;

        switch (eventWallType) {
            //  中間地点解放で通知するメッセージ
            case EventWallType.Halfway:
                text.SendNotificationMessage("次の道が解放された！");
                break;
            //  ゴール地点解放で通知するメッセージ
            case EventWallType.Goal:
                text.SendNotificationMessage("ゴールが解放された！");
                break;
        }
    }

}