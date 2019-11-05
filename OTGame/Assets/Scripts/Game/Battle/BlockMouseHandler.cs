using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockMouseHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public Cell parent;

    public void OnPointerClick(PointerEventData eventData)
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        FindObjectOfType<Board>().SelectBlock(parent);
        FindObjectOfType<Board>().StartDragging(parent);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        FindObjectOfType<Board>().StopDragging(parent);
    }
}
