using Items;
using Manage;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private static ItemUI _currUI;
        private static BagPanel _bagPanel;
        private static EquipPanel _equipPanel;
        private static TradePanel _tradePanel;

        public GameItem Item { get; set; }
        public ItemSlot CurrSlot { get; set; }

        private void Awake()
        {
            if (_bagPanel == null)
                _bagPanel = UIManager.Instance.GetPanel<BagPanel>();
            if (_equipPanel == null)
                _equipPanel = UIManager.Instance.GetPanel<EquipPanel>();
            if (_tradePanel == null)
                _tradePanel = UIManager.Instance.GetPanel<TradePanel>();
        }

        public int AddToBagUI(int index = 0)
        {
            if (!_bagPanel.UiIndexDict.ContainsKey(Item.GetHashCode()))
            {
                CurrSlot = index > 0 ? _bagPanel.GetSlotByIndex(index) : _bagPanel.GetFirstEmptySlot();
                PoolManager.Instance.Pop(Item.ObjName, CurrSlot.Icons);
                _bagPanel.UiIndexDict.Add(Item.GetHashCode(), CurrSlot.Index);
            }
            else
            {
                CurrSlot = _bagPanel.itemSlots[_bagPanel.UiIndexDict[Item.GetHashCode()]];
                PoolManager.Instance.Pop(Item.ObjName, CurrSlot.Icons);
                CurrSlot.Count.text = CurrSlot.Icons.childCount.ToString();
            }
            return CurrSlot.Index;
        }

        public int AddToEquipUI(int index) 
        {
            CurrSlot = _equipPanel.GetSlotByIndex(index);
            PoolManager.Instance.Pop(Item.ObjName, CurrSlot.Icons);
            return index; 
        }

        public int AddToActionUI(int index = 0) { return 0; }

        public int AddToTradeUI(int index = 0) 
        {
            if (!_tradePanel.RemoteRect.UiIndexDict.ContainsKey(Item.GetHashCode()))
            {
                CurrSlot = index > 0 ? _tradePanel.RemoteRect.GetSlotByIndex(index) : _tradePanel.RemoteRect.GetFirstEmptySlot();
                PoolManager.Instance.Pop(Item.ObjName, CurrSlot.Icons);
                _tradePanel.RemoteRect.UiIndexDict.Add(Item.GetHashCode(), CurrSlot.Index);
            }
            else
            {
                CurrSlot = _tradePanel.RemoteRect.itemSlots[_tradePanel.RemoteRect.UiIndexDict[Item.GetHashCode()]];
                PoolManager.Instance.Pop(Item.ObjName, CurrSlot.Icons);
                CurrSlot.Count.text = CurrSlot.Icons.childCount.ToString();
            }
            return CurrSlot.Index;
        }

        public void RemoveFromKnap()
        {
            PoolManager.Instance.Push(Item.ObjName, gameObject);
        }

        private bool CanSwapUI(Transform parent, out ItemSlot newSlot)
        {
            newSlot = parent.GetComponent<ItemSlot>();
            if (newSlot && (newSlot.itemType == ItemType.None || newSlot.itemType == Item.itemType))
                return true;
            else
            {
                newSlot = parent.parent.GetComponent<ItemSlot>();
                if (newSlot)
                {
                    if (CurrSlot.itemType == newSlot.itemType || newSlot.itemType == Item.itemType)
                        return true;
                    if (newSlot.GetIconType() == Item.itemType)
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
                _currUI = CurrSlot.Icons.GetChild(0).GetComponent<ItemUI>();
                _currUI.transform.SetParent(UIManager.Instance.tempSlot, true);
                _currUI.GetComponent<Image>().raycastTarget = false;
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
                    int count = newSlot.Icons.childCount;
                    for (int i = 0; i < count; i++)
                    {
                        _currUI = newSlot.Icons.GetChild(0).GetComponent<ItemUI>();
                        _currUI.CurrSlot = CurrSlot;
                        _currUI.Item.UpdateUiLoc(CurrSlot.knapType, CurrSlot.Index);
                        _currUI.transform.SetParent(CurrSlot.Icons, true);
                        _currUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                    }
                    CurrSlot.Count.text = count > 1 ? count.ToString() : "";
                    count = UIManager.Instance.tempSlot.childCount;
                    for (int i = 0; i < count; i++)
                    {
                        _currUI = UIManager.Instance.tempSlot.GetChild(0).GetComponent<ItemUI>();
                        _currUI.CurrSlot = newSlot;
                        _currUI.Item.UpdateUiLoc(CurrSlot.knapType, newSlot.Index);
                        _currUI.transform.SetParent(newSlot.Icons, true);
                        _currUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                        _currUI.GetComponent<Image>().raycastTarget = true;
                    }
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
                _currUI = UIManager.Instance.tempSlot.GetChild(0).GetComponent<ItemUI>();
                _currUI.transform.SetParent(CurrSlot.Icons, true);
                _currUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                _currUI.GetComponent<Image>().raycastTarget = true;
            }
            CurrSlot.Count.text = count > 1 ? count.ToString() : "";
        }
    }
}
