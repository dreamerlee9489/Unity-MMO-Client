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

        protected override void Awake()
        {
            base.Awake();
            Close();
        }
    }
}
