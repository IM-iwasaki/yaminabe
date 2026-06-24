using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VirtualCursorRayCaster : MonoBehaviour
{
    public RectTransform cursor;
    public EventSystem eventSystem;

    PointerEventData pointerData;
    List<RaycastResult> currentHits = new List<RaycastResult>();
    List<GameObject> lastHits = new List<GameObject>();

    GameObject pressedObject;
    // Update is called once per frame
    void Update()
    {
        EnsureEventSystem();
        UpdateCursorPosition();
        UpdatePointerData();
        RaycastUI();

        HandlePointerEnterExit();
        HandlePointerPress();
        HandlePointerRelease();

    }

    /// <summary>
    /// イベントシステムの存在チェック
    /// </summary>
    private void EnsureEventSystem()
    {
        if (eventSystem == null)
        {
            eventSystem = FindAnyObjectByType<EventSystem>();
        }
    }

    /// <summary>
    /// カーソルの更新
    /// </summary>
    private void UpdateCursorPosition()
    {
        RectTransform rt = cursor;
        Vector2 pos = rt.anchoredPosition;

        // カーソルのサイズ
        float halfW = rt.sizeDelta.x * 0.5f;
        float halfH = rt.sizeDelta.y * 0.5f;

        // Canvas のサイズ
        Canvas canvas = GetComponentInParent<Canvas>();
        float maxX = canvas.pixelRect.width - halfW;
        float maxY = canvas.pixelRect.height - halfH;

        pos.x = Mathf.Clamp(pos.x, halfW, maxX);
        pos.y = Mathf.Clamp(pos.y, halfH, maxY);

        rt.anchoredPosition = pos;
    }

    /// <summary>
    /// ポインターデータの更新
    /// </summary>
    private void UpdatePointerData()
    {
        pointerData = new PointerEventData(eventSystem);

        pointerData.position = cursor.position;

    }

    /// <summary>
    /// UIへレイキャスト
    /// </summary>
    private void RaycastUI()
    {
        //Raycast
        currentHits.Clear();
        eventSystem.RaycastAll(pointerData, currentHits);

    }

    /// <summary>
    /// PointerEnter / PointerExit
    /// </summary>
    private void HandlePointerEnterExit()
    {
        List<GameObject> currentObjects = new List<GameObject>();
        foreach (var hit in currentHits)
            currentObjects.Add(hit.gameObject);

        foreach (var hit in currentHits)
        {
            Debug.Log("Hit: " + hit.gameObject.name);
        }

        //PointerEnter
        foreach (var obj in currentObjects)
        {
            if (!lastHits.Contains(obj))
                ExecuteEvents.Execute(obj.gameObject, pointerData, ExecuteEvents.pointerEnterHandler);
        }
        //PointerExit
        foreach (var lastObj in lastHits)
        {
            if (!currentObjects.Contains(lastObj))
            {
                ExecuteEvents.Execute(lastObj.gameObject, pointerData, ExecuteEvents.pointerExitHandler);
            }
        }

        lastHits = currentObjects;
    }

    /// <summary>
    /// PointerPress
    /// </summary>
    private void HandlePointerPress()
    {
        //PointerDown
        if (Gamepad.current == null) return;
        if (!Gamepad.current.buttonWest.wasPressedThisFrame) return;

        if (currentHits.Count == 0) return;

        var hit = currentHits[0];

        // Button を探す（Text や Image の親に Button がある場合にも対応）
        var button = hit.gameObject.GetComponent<Button>()
                     ?? hit.gameObject.GetComponentInParent<Button>();

        if (button != null)
        {
            pointerData.pointerPressRaycast = hit;
            pressedObject = button.gameObject;

            ExecuteEvents.Execute(pressedObject, pointerData, ExecuteEvents.pointerDownHandler);
        }
    }

    /// <summary>
    /// PointerRelease
    /// </summary>
    private void HandlePointerRelease()
    {
        // PointerUp + Click
        if (Gamepad.current == null) return;
        if (!Gamepad.current.buttonWest.wasReleasedThisFrame) return;

        if (pressedObject == null) return;

        // PointerUp
        ExecuteEvents.Execute(pressedObject, pointerData, ExecuteEvents.pointerUpHandler);

        // Click（Down と Up が同じオブジェクトならクリック扱い）
        foreach(var hit in currentHits)
        {
            if (hit.gameObject == pressedObject)
            {
                ExecuteEvents.Execute(pressedObject, pointerData, ExecuteEvents.pointerClickHandler);
            }
        }

        pressedObject = null;

    }
    

}
