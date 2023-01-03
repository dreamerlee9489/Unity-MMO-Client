using Control;
using Manage;
using UI;
using UnityEngine;

namespace Item
{
    public enum ItemType { None, Potion, Weapon }
    public enum ItemState { World, Knap, Equip }

    public abstract class GameItem : MonoBehaviour
    {
        protected ItemState _itemState = ItemState.World;
        protected ItemNameBar _nameBar;

        public int itemId;
        public string objName;
        public ItemType itemType = ItemType.None;

        public ItemUI ItemUI { get; set; }

        private void Awake()
        {
            _nameBar = transform.Find("ItemNameBar").GetComponent<ItemNameBar>();
        }

        private void Update()
        {
            if (_itemState != ItemState.World && gameObject.activeSelf)
                gameObject.SetActive(false);
            else if (_itemState == ItemState.World && !gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        protected void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                Proto.AddItemToKnap addItem = new() { Item = new() };
                switch (itemType)
                {
                    case ItemType.Potion:
                        addItem.Item.Id = itemId;
                        addItem.Item.Num = 1;
                        addItem.Item.Type = Proto.ItemData.Types.ItemType.Potion;
                        break;
                    case ItemType.Weapon:
                        addItem.Item.Id = itemId;
                        addItem.Item.Num = 1;
                        addItem.Item.Type = Proto.ItemData.Types.ItemType.Weapon;
                        break;
                    default:
                        break;
                }
                _itemState = ItemState.Knap;
                other.GetComponent<PlayerController>().AddItemToKnap(this);
                NetManager.Instance.SendPacket(Proto.MsgId.C2SAddItemToKnap, addItem);
            }
        }

        public void SetNameBar(string name) => _nameBar.Name.text = name;

        public string GetKey() => $"{(int)itemType}@{itemId}";

        public string GetName() => objName;
    }
}
