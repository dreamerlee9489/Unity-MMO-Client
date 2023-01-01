using Manage;
using UnityEngine;

namespace Control
{
    public class AnimExecutor : MonoBehaviour
    {
        private GameEntity _owner;

        private void Awake()
        {
            _owner = GetComponent<GameEntity>();
        }

        public void AtkAnimEvent()
        {
            if(CompareTag("Player") && _owner.GetComponent<PlayerController>().sn == GameManager.Instance.MainPlayer.Sn)
            {
                Proto.AtkAnimEvent proto = new()
                {
                    PlayerSn = GameManager.Instance.MainPlayer.Sn,
                    EnemyId = _owner.target.GetComponent<FsmController>().id,
                    CurrHp = _owner.hp,
                    AtkEnemy = true
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SAtkAnimEvent, proto);
            }
            else if(CompareTag("Enemy") && _owner.target.GetComponent<PlayerController>().sn == GameManager.Instance.MainPlayer.Sn)
            {
                Proto.AtkAnimEvent proto = new()
                { 
                    EnemyId = _owner.GetComponent<FsmController>().id,
                    PlayerSn = GameManager.Instance.MainPlayer.Sn,
                    CurrHp = _owner.hp,
                    AtkEnemy = false
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SAtkAnimEvent, proto);
            }
        }
    }
}
