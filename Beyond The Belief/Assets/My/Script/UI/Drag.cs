using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Drag : MonoBehaviour,IDragHandler
{
   
    public void OnDrag(PointerEventData data)
    {
        var rect = transform.GetComponent<RectTransform>(); 
        rect.anchoredPosition += data.delta;
    }
    // Update is called once per frame
    
}
