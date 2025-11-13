using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ロビーの落下判定用スクリプト
/// </summary>
public class LobbyFallArea : MonoBehaviour
{
    private readonly string FALL_TAG = "Player";

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag(FALL_TAG)) {
            Vector3 teleportPos = new Vector3(0, 10, 0);

            other.transform.position = teleportPos;
        }
    }
}
