

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
        switch (eventWallType) {
            //  中間地点解放で通知するメッセージ
            case EventWallType.Halfway:
                break;
            //  ゴール地点解放で通知するメッセージ
            case EventWallType.Goal:
                break;
        }
    }
}