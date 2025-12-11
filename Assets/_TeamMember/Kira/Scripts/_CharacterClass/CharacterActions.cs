using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;

public class CharacterActions : NetworkBehaviour
{
    private CharacterBase core;
    private CharacterParameter param;
    private CharacterInput input;
    private Rigidbody rb;

    //移動中か
    public bool isMoving { get; private set; } = false;
    //移動を要求する方向
    //protected Vector2 MoveInput;
    //実際に移動する方向
    public Vector3 moveDirection { get; private set; }

    public void Initialize(CharacterBase core)
    {
        this.core = core;
        param = core.parameter;
        input = core.input;
        rb = core.GetComponent<Rigidbody>();
    }

    public void TickUpdate() {
        JumpControl();
        HandleAttack();
        HandleSkill();
        HandleInteract();
    }

    public void TickFixedUpdate() {
        MoveControl();
        param.GroundCheck(core.transform.position);
    }

    private void MoveControl() {
        //移動入力が行われている間は移動中フラグを立てる
        if (input.MoveInput != Vector2.zero) isMoving = true;
        else isMoving = false;

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
        moveDirection = forward * input.MoveInput.y + right * input.MoveInput.x;

        // カメラの向いている方向をプレイヤーの正面に
        Vector3 aimForward = forward; // 水平面だけを考慮
        if (aimForward != Vector3.zero) {
            Quaternion targetRot = Quaternion.LookRotation(aimForward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, PlayerConst.TURN_SPEED * Time.deltaTime);
        }

        // 空中か地上で挙動を分ける
        Vector3 velocity = rb.velocity;
        Vector3 targetVelocity = new(moveDirection.x * param.moveSpeed, velocity.y, moveDirection.z * param.moveSpeed);

        //地面に立っていたら通常通り
        if (param.IsGrounded) {
            rb.velocity = targetVelocity;
        }
        else {
            // 空中では地上速度に向けてゆるやかに補間（慣性を残す）
            rb.velocity = Vector3.Lerp(velocity, targetVelocity, Time.deltaTime * 2f);
        }
    }    

    private void JumpControl() {
        if (!input.JumpPressed || !param.IsGrounded) return;

        Vector3 vel = rb.velocity;
        vel.y = 0;
        rb.velocity = vel;

        rb.AddForce(Vector3.up * 12f, ForceMode.Impulse);
    }

    private void HandleAttack()
    {
        if (input.AttackPressed)
        {
            Cmd_DoAttack();
        }
    }

    [Command]
    private void Cmd_DoAttack()
    {
        if (param.IsDead) return;
        Debug.Log("Attack performed");
    }

    private void HandleSkill()
    {
        if (!input.SkillTriggered) return;
        input.SkillTriggered = false;

        Cmd_DoSkill();
    }

    [Command]
    private void Cmd_DoSkill()
    {
        Debug.Log("Skill activated");
    }

    private void HandleInteract()
    {
        if (!input.InteractTriggered) return;
        input.InteractTriggered = false;

        Debug.Log("Interact event");
    }
}