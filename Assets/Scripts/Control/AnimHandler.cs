using Items;
using Manage;
using UnityEngine;

namespace Control
{
    public class AnimHandler : MonoBehaviour
    {
        private GameEntity _owner;

        private void Awake()
        {
            _owner = GetComponent<GameEntity>();
        }

        public void AtkAnimEvent()
        {
            if(CompareTag("Player") && _owner.target && _owner.GetComponent<PlayerController>().Sn == GameManager.Instance.mainPlayer.Sn)
            {
                Proto.PlayerAtkEvent proto = new()
                {
                    PlayerSn = GameManager.Instance.mainPlayer.Sn,
                    TargetSn = _owner.target.GetComponent<FsmController>().Sn
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SPlayerAtkEvent, proto);
            }
            else if(CompareTag("Enemy"))
            {
                if (_owner.target && _owner.target.GetComponent<PlayerController>().Sn == GameManager.Instance.mainPlayer.Sn)
                {
                    Proto.NpcAtkEvent proto = new()
                    {
                        NpcSn = _owner.Sn,
                        TargetSn = GameManager.Instance.mainPlayer.Sn
                    };
                    NetManager.Instance.SendPacket(Proto.MsgId.C2SNpcAtkEvent, proto);
                }
            }
        }

        public void PickupEvent()
        {
            if (_owner.target)
            {
                _owner.target.GetComponent<GameItem>().RequestPickup(_owner.GetComponent<PlayerController>());
                _owner.target = null;
            }
        }
    }
}
