using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PlayerBase : CharacterBase {

    protected override void MoveControl() {
        //カメラの向きを取得
        Transform cameraTransform = Camera.main.transform;
        //進行方向のベクトルを取得
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();
        //右方向のベクトルを取得
        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();
        //2つのベクトルを合成
        MoveDirection = forward * MoveInput.y + right * MoveInput.x;

        // カメラの向いている方向をプレイヤーの正面に
        Vector3 aimForward = forward; // 水平面だけを考慮
        if (aimForward != Vector3.zero) {
            Quaternion targetRot = Quaternion.LookRotation(aimForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, PlayerConst.TURN_SPEED * Time.deltaTime);
        }

        // 移動方向にキャラクターを向ける
        //Quaternion targetRotation = Quaternion.LookRotation(MoveDirection);
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

        rigidbody.velocity = new Vector3(MoveDirection.x * MoveSpeed, rigidbody.velocity.y, MoveDirection.z * MoveSpeed);
    }

    protected override void LookControl() {
        
    }

    protected override void StartAttack() {
       
    }

    protected override void Interact() {

    }

    // Start is called before the first frame update
    protected new void Start() {
        base.Start();        
    }

    // Update is called once per frame
    void Update() {
        MoveControl();
        LookControl();
    }

    
}
