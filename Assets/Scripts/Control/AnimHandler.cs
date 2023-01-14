using Items;
using Manage;
using UnityEngine;

namespace Control
{
    public class AnimHandler : MonoBehaviour
    {
        private PlayerController _player;
        private FsmController _npc;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            _npc = GetComponent<FsmController>();
        }

        public void AtkAnimEvent()
        {
            if (_player && _player.target && _player.Sn == GameManager.Instance.mainPlayer.Sn)
            {
                Proto.PlayerAtkEvent proto = new() { PlayerSn = GameManager.Instance.mainPlayer.Sn };
                if (_player.target.GetComponent<FsmController>() != null)
                    proto.TargetSn = _player.target.GetComponent<FsmController>().Sn;
                else
                    proto.TargetSn = _player.target.GetComponent<PlayerController>().Sn;
                NetManager.Instance.SendPacket(Proto.MsgId.C2SPlayerAtkEvent, proto);
            }
            else if (_npc)
            {
                if (_npc.target && _npc.target.GetComponent<PlayerController>().Sn == GameManager.Instance.mainPlayer.Sn)
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
            if (_player.target)
            {
                _player.target.GetComponent<GameItem>().RequestPickup(_player);
                _player.target = null;
            }
        }
    }
}
