using UnityEngine;

namespace Item
{
    public enum ItemType { None, Potion, Weapon }
    public enum ItemState { World, Pack, Equip }

    public abstract class GameItem : MonoBehaviour
    {
        protected ItemState _itemState = ItemState.World;
        protected ItemType _itemType = ItemType.None;

        protected void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Player"))
                PickUp();
        }

        protected void PickUp()
        {
            gameObject.SetActive(false);
            _itemState = ItemState.Pack;
        }
    }
}
