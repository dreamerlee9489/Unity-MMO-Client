using Control;
using Manage;

namespace Items
{
    public class Portal : GameItem
	{
        public int targetId = 0;

        public override void RequestPickup(PlayerController player)
        {
            throw new System.Exception("Can't pick up portal.");
        }

        public void OpenDoor(PlayerController player)
        {
            player.Agent.enabled = false;
            player.gameObject.SetActive(false);
            Proto.EnterWorld proto = new()
            {
                WorldId = targetId,
                Position = null
            };
            NetManager.Instance.SendPacket(Proto.MsgId.C2GEnterWorld, proto);
        }
    }
}
