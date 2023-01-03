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
            if (!_knapPanel.uiDict.ContainsKey(Item.objName))
            {
                itemSlot = _knapPanel.GetFirstEmptySlot();
                PoolManager.Instance.Pop(Item.objName, itemSlot.icons);
                _knapPanel.uiDict.Add(Item.objName, itemSlot.index);
            }
            else
            {
                switch (Item.itemType)
                {
                    case ItemType.None:
                        break;
                    case ItemType.Potion:
                        itemSlot = _knapPanel.itemSlots[_knapPanel.uiDict[Item.objName]]; 
                        PoolManager.Instance.Pop(Item.objName, itemSlot.icons);
                        itemSlot.count.text = itemSlot.icons.childCount.ToString();
                        break;
                    case ItemType.Weapon:
                        itemSlot = _knapPanel.GetFirstEmptySlot();
                        transform.SetParent(itemSlot.icons, false);
                        gameObject.SetActive(true);
                        _knapPanel.uiDict[Item.objName] = itemSlot.index;
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
