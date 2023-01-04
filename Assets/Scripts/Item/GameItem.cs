using Control;
using Manage;
using System;
using UI;
using UnityEngine;

namespace Items
{
    public enum ItemType { None, Potion, Weapon, Shield, Helmet, Chest, Boots, Neck, Gloves, Pants }
    public enum ItemState { World, Knap, Equip }

    public abstract class GameItem : MonoBehaviour
    {
        protected ItemState _itemState = ItemState.World;
        protected ItemNameBar _nameBar;

        public int itemId;
        public string objName;
        public int hashCode;
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
            if (other.CompareTag("Player"))
            {
                _itemState = ItemState.Knap;
                Proto.AddItemToKnap proto = new() { Item = new() };
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
                proto.Item.Id = itemId;
                proto.Item.Num = 1;
                proto.Item.Hash = hashCode;
                proto.Item.Index = other.GetComponent<PlayerController>().AddItemToKnap(this);
                NetManager.Instance.SendPacket(Proto.MsgId.C2SAddItemToKnap, proto);
            }
        }

        public void SetNameBar(string name) => _nameBar.Name.text = name;

        public string GetName() => objName;

        public int GenHash()
        {
            return itemType switch
            {
                ItemType.Potion => ($"{itemType}@{itemId}").GetHashCode(),
                ItemType.Weapon or ItemType.Shield or ItemType.Helmet or ItemType.Chest or ItemType.Boots or ItemType.Neck or ItemType.Gloves or ItemType.Pants => Guid.NewGuid().GetHashCode(),
                _ => 0,
            };
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public override bool Equals(object other)
        {
            if(!(other as GameItem))
                return false;
            return GetHashCode().Equals(other.GetHashCode());
        }
    }
}
