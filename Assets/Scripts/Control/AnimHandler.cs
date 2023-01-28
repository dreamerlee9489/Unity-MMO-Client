using Items;
using Manage;
using UnityEngine;

namespace Control
{
    public class AnimHandler : MonoBehaviour
    {
        private PlayerController _player;
        private BtController _npc;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            _npc = GetComponent<BtController>();
        }

        public void AtkAnimEvent()
        {
            if (_player && _player.Target && _player.Sn == GameManager.Instance.mainPlayer.Sn)
            {
                Proto.PlayerAtkEvent proto = new() { PlayerSn = GameManager.Instance.mainPlayer.Sn };
                if (_player.Target.GetComponent<BtController>() != null)
                    proto.TargetSn = _player.Target.GetComponent<BtController>().Sn;
                else
                    proto.TargetSn = _player.Target.GetComponent<PlayerController>().Sn;
                NetManager.Instance.SendPacket(Proto.MsgId.C2SPlayerAtkEvent, proto);
            }
            else if (_npc)
            {
                if (_npc.Target && _npc.Target.GetComponent<PlayerController>().Sn == GameManager.Instance.mainPlayer.Sn)
                {
                    Proto.NpcAtkEvent proto = new()
                    {
                        NpcSn = _npc.Sn,
                        TargetSn = GameManager.Instance.mainPlayer.Sn
                    };
                    NetManager.Instance.SendPacket(Proto.MsgId.C2SNpcAtkEvent, proto);
                }
            }
        }

        public void PickupEvent()
        {
            if (_player.Target)
            {
                _player.Target.GetComponent<GameItem>().RequestPickup(_player);
                _player.Target = null;
            }
        }
    }
}
