using Control;
using Manage;
using System;
using UI;
using UnityEngine;

namespace Items
{
    public enum ItemType { None, Potion, Weapon, Shield, Helmet, Chest, Boots, Neck, Gloves, Pants, Portal }
    public enum ItemState { World, Knap, Equip }

    public abstract class GameItem : MonoBehaviour
    {
        protected int _hashCode, _keyCode;
        protected static WorldManager _currWorld;
        protected ItemState _itemState = ItemState.World;
        protected ItemNameBar _nameBar;

        public int ItemId { get; set; }
        public string ObjName { get; set; }
        public ItemUI ItemUI { get; set; }

        public ItemType itemType = ItemType.None;

        public int GenKeyCode() => $"{itemType}@{ItemId}".GetHashCode();

        public int GetKeyCode() => _keyCode;

        public void SetKeyCode(int keyCode) => _keyCode = keyCode;

        public override int GetHashCode() => _hashCode;

        public override bool Equals(object other) => _hashCode == other.GetHashCode();

        protected virtual void Awake()
        {
            _hashCode = Guid.NewGuid().GetHashCode();
            _nameBar = transform.Find("ItemNameBar").GetComponent<ItemNameBar>();
            if (_currWorld)
                _currWorld.inWorldObjDict.TryAdd(_hashCode, transform);
            EventManager.Instance.AddListener(EEventType.SceneUnload, SceneUnloadCallback);
            EventManager.Instance.AddListener(EEventType.SceneLoaded, SceneLoadedCallback);
        }

        private void Update()
        {
            if (_itemState != ItemState.World && gameObject.activeSelf)
                gameObject.SetActive(false);
            else if (_itemState == ItemState.World && !gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        protected virtual void OnApplicationQuit()
        {
            EventManager.Instance.RemoveListener(EEventType.SceneUnload, SceneUnloadCallback);
            EventManager.Instance.RemoveListener(EEventType.SceneLoaded, SceneLoadedCallback);
        }

        protected void SceneUnloadCallback()
        {
            if (_currWorld)
                _currWorld.inWorldObjDict.Clear();
            _currWorld = null;
        }

        protected void SceneLoadedCallback()
        {
            if (!_currWorld)
                _currWorld = FindObjectOfType<WorldManager>();
            _currWorld.inWorldObjDict.TryAdd(_hashCode, transform);
        }

        public virtual void RequestPickup(PlayerController player)
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
            proto.Item.Id = ItemId;
            proto.Item.Num = 1;
            proto.Item.Key = _keyCode;
            proto.Item.Index = player.AddItemToKnap(this);
            NetManager.Instance.SendPacket(Proto.MsgId.C2SAddItemToKnap, proto);
        }
        
        public void SetNameBar(string name) => _nameBar.Name.text = name;
    }
}
