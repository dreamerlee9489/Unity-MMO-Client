using Control;
using Manage;
using UI;
using UnityEngine;

namespace Items
{
    public enum ItemType { None, Potion, Weapon, Shield, Helmet, Chest, Boots, Neck, Gloves, Pants, Portal }
    public enum KnapType { World, Bag, Equip, Action, Trade }

    public abstract class GameItem : MonoBehaviour
    {
        protected ItemNameBar _nameBar;

        public int id = 0;
        public ItemType itemType = ItemType.None;
        public KnapType knapType = KnapType.World;

        public ulong Sn { get; set; }
        public string ObjName { get; set; }
        public ItemUI ItemUI { get; set; }

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
            if (knapType != KnapType.World && gameObject.activeSelf)
                gameObject.SetActive(false);
            else if (knapType == KnapType.World && !gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void SetNameBar(string name) => _nameBar.Name.text = name;

        public virtual void RequestPickup(PlayerController player)
        {
            Proto.UpdateKnapItem proto = new() { Item = new() };
            proto.Item.Sn = Sn;
            proto.Item.Id = id;
            proto.Item.Index = player.AddItemToKnap(KnapType.Bag, this);
            proto.Item.ItemType = (Proto.ItemData.Types.ItemType)itemType;
            proto.Item.KnapType = (Proto.ItemData.Types.KnapType)knapType;
            NetManager.Instance.SendPacket(Proto.MsgId.C2SUpdateKnapItem, proto);
        }

        public void UpdateUiLoc(KnapType knapType, int index)
        {
            this.knapType = knapType;
            if(knapType != KnapType.Trade)
            {
                Proto.UpdateKnapItem proto = new()
                {
                    Item = new()
                    {
                        Sn = Sn,
                        Id = id,
                        Index = index,
                        ItemType = (Proto.ItemData.Types.ItemType)itemType,
                        KnapType = (Proto.ItemData.Types.KnapType)knapType
                    }
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SUpdateKnapItem, proto);
            }
            else
            {
                Proto.UpdateTradeItem proto = new()
                {
                    Sender = UIManager.Instance.GetPanel<TradePanel>().localSn,
                    Recver = UIManager.Instance.GetPanel<TradePanel>().remoteSn,
                    Ack = false,
                    Gold = 0,
                    Item = new()
                    {
                        Sn = Sn,
                        Id = id,
                        Index = index,
                        ItemType = (Proto.ItemData.Types.ItemType)itemType,
                        KnapType = (Proto.ItemData.Types.KnapType)knapType
                    }
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2CUpdateTradeItem, proto);
            }
        }
    }
}
