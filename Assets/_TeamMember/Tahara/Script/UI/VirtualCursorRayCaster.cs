using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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
        pointerData = new PointerEventData(eventSystem);
        pointerData.position = cursor.position;

        //Raycast
        currentHits.Clear();
        eventSystem.RaycastAll(pointerData, currentHits);

        List<GameObject> currentObjects = new List<GameObject>();
        foreach (var hit in currentHits)
            currentObjects.Add(hit.gameObject);

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
        if(Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            if(currentObjects.Count > 0)
            {
                pressedObject = currentObjects[0];
                ExecuteEvents.Execute(pressedObject, pointerData, ExecuteEvents.pointerDownHandler);
            }
        }

        lastHits = currentObjects;
    }
}
