using Control;
using Manage;
using UI;
using UnityEngine;

namespace Items
{
    public enum ItemType { None, Potion, Weapon, Shield, Helmet, Chest, Boots, Neck, Gloves, Pants, Portal }
    public enum ItemState { World, Knap, Equip }

    public abstract class GameItem : MonoBehaviour
    {
        protected ItemState _itemState = ItemState.World;
        protected ItemNameBar _nameBar;

        public ulong Sn { get; set; }
        public string ObjName { get; set; }
        public ItemUI ItemUI { get; set; }

        public int id = 0;
        public ItemType itemType = ItemType.None;

        public override int GetHashCode()
        {
            return itemType switch
            {
                ItemType.Potion => $"{itemType}@{id}".GetHashCode(),
                ItemType.Weapon or ItemType.Shield or ItemType.Helmet or ItemType.Chest or ItemType.Boots or ItemType.Neck or ItemType.Gloves or ItemType.Pants or ItemType.Portal => Sn.GetHashCode(),
                _ => base.GetHashCode(),
            };
        }

        public override bool Equals(object other)
        {
            if(other is not GameItem) return false;
            return GetHashCode().Equals(other.GetHashCode());
        }

        protected virtual void Awake()
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

        public void SetNameBar(string name) => _nameBar.Name.text = name;

        public virtual void RequestPickup(PlayerController player)
        {
            _itemState = ItemState.Knap;
            Proto.UpdateKnapItem proto = new() { Item = new() };
            switch (itemType)
            {
                case ItemType.Potion:
                    proto.Item.Type = Proto.ItemData.Types.ItemType.Potion;
                    break;
                case ItemType.Weapon:
                    proto.Item.Type = Proto.ItemData.Types.ItemType.Weapon;
                    break;
                default:
                    break;
            }
            proto.Item.Sn = Sn;
            proto.Item.Id = id;
            proto.Item.Index = player.AddItemToBag(this);
            NetManager.Instance.SendPacket(Proto.MsgId.C2SUpdateKnapItem, proto);
        }
    }
}
