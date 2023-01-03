using Item;
using Manage;
using UnityEngine;

namespace UI
{
    public class ItemUI : MonoBehaviour
	{
        private static KnapPanel _knapPanel;
        public GameItem Item { get; set; }

        private void Awake()
        {
            if(_knapPanel == null)
                _knapPanel = UIManager.Instance.FindPanel<KnapPanel>();
        }

        public void AddToKnap()
        {
            ItemSlot itemSlot;
            if (!_knapPanel.UiDict.ContainsKey(Item.objName))
            {
                itemSlot = _knapPanel.GetFirstEmptySlot();
                PoolManager.Instance.Pop(Item.objName, itemSlot.Icons);
                _knapPanel.UiDict.Add(Item.objName, itemSlot.Index);
            }
            else
            {
                switch (Item.itemType)
                {
                    case ItemType.None:
                        break;
                    case ItemType.Potion:
                        itemSlot = _knapPanel.ItemSlots[_knapPanel.UiDict[Item.objName]]; 
                        PoolManager.Instance.Pop(Item.objName, itemSlot.Icons);
                        itemSlot.Count.text = itemSlot.Icons.childCount.ToString();
                        break;
                    case ItemType.Weapon:
                        itemSlot = _knapPanel.GetFirstEmptySlot();
                        transform.SetParent(itemSlot.Icons, false);
                        gameObject.SetActive(true);
                        _knapPanel.UiDict[Item.objName] = itemSlot.Index;
                        break;
                    default:
                        break;
                }
            }
        }

        public void RemoveFromKnap()
        {
            PoolManager.Instance.Push(Item.objName, gameObject);
        }
    }
}
