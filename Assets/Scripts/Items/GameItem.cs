using Control;
using Manage;
using UI;

namespace Items
{
    public enum ItemType { None, Potion, Weapon, Shield, Helmet, Chest, Boots, Neck, Gloves, Pants, Portal }
    public enum ItemState { World, Knap, Equip }

    public abstract class GameItem : GuidObject
    {
        protected string _knapId;
        
        protected ItemState _itemState = ItemState.World;
        protected ItemNameBar _nameBar;

        public int ItemId { get; set; }
        public string ObjName { get; set; }
        public ItemUI ItemUI { get; set; }

        public ItemType itemType = ItemType.None;

        protected override void Awake()
        {
            base.Awake();
            _nameBar = transform.Find("ItemNameBar").GetComponent<ItemNameBar>();
        }

        private void Update()
        {
            if (_itemState != ItemState.World && gameObject.activeSelf)
                gameObject.SetActive(false);
            else if (_itemState == ItemState.World && !gameObject.activeSelf)
                gameObject.SetActive(true);
        }

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
            proto.Item.Id = ItemId;
            proto.Item.Num = 1;
            proto.Item.Key = GenKnapId();
            proto.Item.Index = player.UpdateKnapItem(this);
            NetManager.Instance.SendPacket(Proto.MsgId.C2SUpdateKnapItem, proto);
        }

        public void SetNameBar(string name) => _nameBar.Name.text = name;

        public void SetKnapId(string key) => _knapId = key;

        public string GetKnapId() => _knapId;

        public string GenKnapId()
        {
            return itemType switch
            {
                ItemType.None or ItemType.Potion => _knapId = $"{itemType}@{ItemId}",
                ItemType.Weapon or ItemType.Shield or ItemType.Helmet or ItemType.Chest or ItemType.Boots or ItemType.Neck or ItemType.Gloves or ItemType.Pants or ItemType.Portal => _knapId = _guid,
                _ => "",
            };
        }
    }
}
