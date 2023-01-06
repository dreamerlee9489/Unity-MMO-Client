using Items;
using Manage;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private static RectTransform _currUI;
        private static KnapPanel _knapPanel;

        public GameItem Item { get; set; }
        public ItemSlot CurrSlot { get; set; }

        private void Awake()
        {
            if (_knapPanel == null)
                _knapPanel = UIManager.Instance.FindPanel<KnapPanel>();
        }

        public int AddToKnap(int index = 0)
        {
            if (!_knapPanel.UiIndexDict.ContainsKey(Item.GetKnapId()))
            {
                CurrSlot = index > 0 ? _knapPanel.GetSlotByIndex(index) : _knapPanel.GetFirstEmptySlot();
                PoolManager.Instance.Pop(Item.ObjName, CurrSlot.Icons);
                _knapPanel.UiIndexDict.Add(Item.GetKnapId(), CurrSlot.index);
            }
            else
            {
                CurrSlot = _knapPanel.ItemSlots[_knapPanel.UiIndexDict[Item.GetKnapId()]];
                PoolManager.Instance.Pop(Item.ObjName, CurrSlot.Icons);
                CurrSlot.Count.text = CurrSlot.Icons.childCount.ToString();
            }
            return CurrSlot.index;
        }

        public void RemoveFromKnap()
        {
            PoolManager.Instance.Push(Item.ObjName, gameObject);
        }

        private bool CanSwapUI(Transform parent, out ItemSlot newSlot)
        {
            newSlot = parent.GetComponent<ItemSlot>();
            if (newSlot && (newSlot.slotType == ItemType.None || newSlot.slotType == Item.itemType))
                return true;
            else
            {
                newSlot = parent.parent.GetComponent<ItemSlot>();
                if(newSlot)
                {
                    if(CurrSlot.slotType == newSlot.slotType || newSlot.slotType == Item.itemType)
                        return true;
                    if(newSlot.GetIconType() == Item.itemType) 
                        return true;
                }
                newSlot = null;
                return false;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (CurrSlot.Icons.childCount > 1)
                CurrSlot.Count.text = "";
            int count = CurrSlot.Icons.childCount;
            for (int i = 0; i < count; i++)
            {
                _currUI = CurrSlot.Icons.GetChild(0) as RectTransform;
                _currUI.GetComponent<Image>().raycastTarget = false;
                _currUI.SetParent(UIManager.Instance.tempSlot, true);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            for (int i = 0; i < UIManager.Instance.tempSlot.childCount; i++)
                UIManager.Instance.tempSlot.GetChild(i).position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            GameObject target = eventData.pointerCurrentRaycast.gameObject;
            if (!target || !target.transform.parent || !target.transform.parent.parent)
                PutBackTempIcons();
            else
            {
                if (!CanSwapUI(target.transform.parent, out ItemSlot newSlot))
                    PutBackTempIcons();
                else
                {
                    var knapPanle = UIManager.Instance.FindPanel<KnapPanel>();
                    int count = newSlot.Icons.childCount;
                    for (int i = 0; i < count; i++)
                    {
                        _currUI = newSlot.Icons.GetChild(0) as RectTransform;
                        _currUI.GetComponent<ItemUI>().CurrSlot = CurrSlot;
                        _currUI.SetParent(CurrSlot.Icons, true);
                        _currUI.anchoredPosition = new Vector2(0, 0);
                    }
                    if(count > 0)
                        knapPanle.UpdateUiIndex(_currUI.GetComponent<ItemUI>().Item, CurrSlot);
                    CurrSlot.Count.text = count > 1 ? count.ToString() : "";
                    count = UIManager.Instance.tempSlot.childCount;
                    for (int i = 0; i < count; i++)
                    {
                        _currUI = UIManager.Instance.tempSlot.GetChild(0) as RectTransform;
                        _currUI.GetComponent<Image>().raycastTarget = true;
                        _currUI.GetComponent<ItemUI>().CurrSlot = newSlot;
                        _currUI.SetParent(newSlot.Icons, true);
                        _currUI.anchoredPosition = new Vector2(0, 0);
                    }
                    if (count > 0)
                        knapPanle.UpdateUiIndex(_currUI.GetComponent<ItemUI>().Item, CurrSlot);
                    newSlot.Count.text = count > 1 ? count.ToString() : "";
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
        }

        public void OnPointerExit(PointerEventData eventData)
        {
        }

        public void PutBackTempIcons()
        {
            int count = UIManager.Instance.tempSlot.childCount;
            for (int i = 0; i < count; i++)
            {
                _currUI = UIManager.Instance.tempSlot.GetChild(0) as RectTransform;
                _currUI.GetComponent<Image>().raycastTarget = true;
                _currUI.SetParent(CurrSlot.Icons, true);
                _currUI.anchoredPosition = new Vector2(0, 0);
            }
            CurrSlot.Count.text = count > 1 ? count.ToString() : "";
        }
    }
}
