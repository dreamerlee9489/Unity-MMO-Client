using Control;
using Manage;
using UnityEngine;

namespace Items
{
    public class Portal : GameItem
	{
        public int worldId = 0;

        protected override void Awake()
        {
            base.Awake();
            itemType = ItemType.Portal;
        }

        private void Start()
        {
            Sn = (ulong)$"{itemType}@{id}".GetHashCode();
            GameManager.currWorld.itemDict.Add(Sn, this);
        }

        public override void RequestPickup(PlayerController player)
        {
            throw new System.Exception("Can't pick up portal.");
        }

        public void OpenDoor(PlayerController player)
        {
            player.gameObject.SetActive(false);
            Proto.EnterWorld proto = new()
            {
                WorldId = worldId,
                Position = null
            };
            NetManager.Instance.SendPacket(Proto.MsgId.C2GEnterWorld, proto);
        }
    }
}
