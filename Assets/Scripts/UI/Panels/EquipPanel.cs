using Items;
using System.Collections.Generic;

namespace UI
{
    public class EquipPanel : BasePanel
    {
        public ItemSlot helmSlot;
        public ItemSlot chestSlot;
        public ItemSlot shieldSlot;
        public ItemSlot bootsSlot;
        public ItemSlot neckSlot;
        public ItemSlot glovesSlot;
        public ItemSlot weaponSlot;
        public ItemSlot pantsSlot;

        public Dictionary<ItemType, ItemSlot> slotDict = new();

        protected override void Awake()
        {
            base.Awake();
            slotDict.Add(ItemType.Helmet, helmSlot);
            slotDict.Add(ItemType.Chest, chestSlot);
            slotDict.Add(ItemType.Shield, shieldSlot);
            slotDict.Add(ItemType.Boots, bootsSlot);
            slotDict.Add(ItemType.Neck, neckSlot);
            slotDict.Add(ItemType.Gloves, glovesSlot);
            slotDict.Add(ItemType.Weapon, weaponSlot);
            slotDict.Add(ItemType.Pants, pantsSlot);
            Close();
        }
    }
}
