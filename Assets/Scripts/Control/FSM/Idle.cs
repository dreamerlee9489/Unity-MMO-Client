using Frame;
using Net;
using UnityEngine;

namespace Control.FSM
{
    public class Idle : FsmState
    {
        public Idle(EnemyController owner, PlayerController target = null) : base(owner, target)
        {
            _type = FsmStateType.Idle;
            _target = GameManager.Instance.MainPlayer.GetGameObject().GetComponent<PlayerController>();
            Enter();
        }

        public override void Enter()
        {
            _owner.Anim.SetBool(GameEntity.Attack, false);
            _owner.Agent.speed = _owner.WalkSpeed;
            if (_owner.IsLinker)
                MonoManager.Instance.StartCoroutine(_owner.UploadData());
        }

        public override void Execute()
        {
            if (_owner.CanSee(_target))
            {
                Proto.FsmSyncState proto = new()
                {
                    EnemyId = _owner.Id,
                    PlayerSn = _target.Sn,
                    State = (int)FsmStateType.Pursuit,
                    Code = -1,
                    CurPos = new()
                    {
                        X = _owner.transform.position.x,
                        Y = _owner.transform.position.y,
                        Z = _owner.transform.position.z
                    },
                    NxtPos = new()
                    {
                        X = _target.transform.position.x,
                        Y = _target.transform.position.y,
                        Z = _target.transform.position.z
                    }
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SFsmSyncState, proto);
            }
        }

        public override void Exit()
        {
        }

        public override void UpdateState(int code)
        {

        }
    }
}
