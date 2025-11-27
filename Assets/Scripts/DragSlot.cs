using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
public class DragSlot : MonoBehaviour, IBeginDragHandler, IDragHandler ,IEndDragHandler
{
    public int index;
    public Canvas canvas;
    public CanvasGroup group;
    public Image dragIcon;

    public void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        group = GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventdata)
    {
        Item item = HotBarManager.Instance.slotItems[index];
        if (item == null)
        {
            return;
        }

        dragIcon = new GameObject("dragIcon").AddComponent<Image>();
        dragIcon.transform.SetParent(canvas.transform, false);
        dragIcon.sprite = item.itemImg;
        //dragIcon.color = new Color(1, 1, 1, 0.8f);
        dragIcon.rectTransform.sizeDelta = new Vector2(50, 50);
        dragIcon.raycastTarget = false;

        group.alpha = 0.5f;

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
        {
            dragIcon.transform.position = eventData.position;
        }

    }

    public void OnEndDrag(PointerEventData eventData) {
        group.alpha = 1f;
        if (dragIcon != null)
        {
            Destroy(dragIcon);
        }
        if (eventData.pointerEnter == null)
        {
            return;
        }
        DragSlot targetslot = eventData.pointerEnter.GetComponentInParent<DragSlot>();
        if (targetslot != null)
        {
            HotBarManager.Instance.SlotSwap(index, targetslot.index);
        }

    }


}

