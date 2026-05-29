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
        if(eventSystem == null)
        {
            eventSystem = FindAnyObjectByType<EventSystem>();
        }
        pointerData = new PointerEventData(eventSystem);

        pointerData.position = cursor.position;

        //Raycast
        currentHits.Clear();
        eventSystem.RaycastAll(pointerData, currentHits);

        List<GameObject> currentObjects = new List<GameObject>();
        foreach (var hit in currentHits)
            currentObjects.Add(hit.gameObject);

        foreach (var hit in currentHits)
        {
            Debug.Log("Hit: " + hit.gameObject.name);
        }

        //PointerEnter
        foreach (var obj in currentObjects){
            if (!lastHits.Contains(obj))
            ExecuteEvents.Execute(obj.gameObject, pointerData, ExecuteEvents.pointerEnterHandler);
        }
        //PointerExit
        foreach(var lastObj in lastHits)
        {
            if (!currentObjects.Contains(lastObj)){
                ExecuteEvents.Execute(lastObj.gameObject, pointerData, ExecuteEvents.pointerExitHandler);
            }
        }

        //PointerDown
        if(Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            if(currentObjects.Count > 0)
            {
                var hit = currentHits[0];

                // Button を探す（Text や Image の親に Button がある場合）
                var button = hit.gameObject.GetComponent<Button>()
                             ?? hit.gameObject.GetComponentInParent<Button>();

                if (button != null)
                {
                    pointerData.pointerPressRaycast = hit;
                    pressedObject = button.gameObject;

                    ExecuteEvents.Execute(pressedObject, pointerData, ExecuteEvents.pointerDownHandler);
                }
            }
        }

        // PointerUp + Click
        if (Gamepad.current != null && Gamepad.current.buttonWest.wasReleasedThisFrame)
        {
            if (pressedObject != null)
            {
                // PointerUp
                ExecuteEvents.Execute(pressedObject, pointerData, ExecuteEvents.pointerUpHandler);

                // Click（Down と Up が同じオブジェクトならクリック扱い）
                if (currentObjects.Contains(pressedObject))
                {
                    ExecuteEvents.Execute(pressedObject, pointerData, ExecuteEvents.pointerClickHandler);
                }

                pressedObject = null;
            }
        }
        lastHits = currentObjects;
    }
}
