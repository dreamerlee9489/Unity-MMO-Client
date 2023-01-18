using Items;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class EquipPanel : BasePanel
    {
        public ItemSlot HelmSlot { get; private set; }
        public ItemSlot ChestSlot { get; private set; }
        public ItemSlot ShieldSlot { get; private set; }
        public ItemSlot BootsSlot { get; private set; }
        public ItemSlot NeckSlot { get; private set; }
        public ItemSlot GlovesSlot { get; private set; }
        public ItemSlot WeaponSlot { get; private set; }
        public ItemSlot PantsSlot { get; private set; }

        private readonly Dictionary<int, ItemSlot> _slotDict = new();

        protected override void Awake()
        {
            base.Awake();
            HelmSlot = transform.GetChild(1).Find("Helm").GetComponent<ItemSlot>();
            ChestSlot = transform.GetChild(1).Find("Chest").GetComponent<ItemSlot>();
            ShieldSlot = transform.GetChild(1).Find("Shield").GetComponent<ItemSlot>();
            BootsSlot = transform.GetChild(1).Find("Boots").GetComponent<ItemSlot>();
            NeckSlot = transform.GetChild(2).Find("Neck").GetComponent<ItemSlot>();
            GlovesSlot = transform.GetChild(2).Find("Gloves").GetComponent<ItemSlot>();
            WeaponSlot = transform.GetChild(2).Find("Weapon").GetComponent<ItemSlot>();
            PantsSlot = transform.GetChild(2).Find("Pants").GetComponent<ItemSlot>();
            HelmSlot.knapType = KnapType.Equip;
            ChestSlot.knapType = KnapType.Equip;
            ShieldSlot.knapType = KnapType.Equip;
            BootsSlot.knapType = KnapType.Equip;
            NeckSlot.knapType = KnapType.Equip;
            GlovesSlot.knapType = KnapType.Equip;
            WeaponSlot.knapType = KnapType.Equip;
            PantsSlot.knapType = KnapType.Equip;
            _slotDict.Add(HelmSlot.Index = (int)ItemType.Helmet, HelmSlot);
            _slotDict.Add(ChestSlot.Index = (int)ItemType.Chest, ChestSlot);
            _slotDict.Add(ShieldSlot.Index = (int)ItemType.Shield, ShieldSlot);
            _slotDict.Add(BootsSlot.Index = (int)ItemType.Boots, BootsSlot);
            _slotDict.Add(NeckSlot.Index = (int)ItemType.Neck, NeckSlot);
            _slotDict.Add(GlovesSlot.Index = (int)ItemType.Gloves, GlovesSlot);
            _slotDict.Add(WeaponSlot.Index = (int)ItemType.Weapon, WeaponSlot);
            _slotDict.Add(PantsSlot.Index = (int)ItemType.Pants, PantsSlot);
            Close();
        }

        public ItemSlot GetSlotByIndex(int index) => _slotDict[index];
    }
}
