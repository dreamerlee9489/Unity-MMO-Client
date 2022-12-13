using Frame;
using Net;

namespace Control.FSM
{
    public class Idle : FsmState
    {
        public Idle(FsmController owner, PlayerController target = null) : base(owner, target)
        {
            _type = FsmStateType.Idle;
            Enter();
        }

        public override void Enter()
        {
            _owner.Anim.SetBool(GameEntity.Attack, false);
            _owner.Agent.speed = _owner.WalkSpeed;
        }

        public override void Execute()
        {
            if (!_target && GameManager.Instance.MainPlayer.GetGameObject())
                _target = GameManager.Instance.MainPlayer.GetGameObject().GetComponent<PlayerController>();
            if (_owner.IsLinker && !_owner.IsLinking)
            {
                _owner.IsLinking = true;
                MonoManager.Instance.StartCoroutine(_owner.UploadData());
            }
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
                if (_owner.IsLinker)
                {
                    _owner.IsLinker = false;
                    _owner.IsLinking = false;
                    MonoManager.Instance.StopCoroutine(_owner.UploadData());
                }
            }
        }

        public override void Exit()
        {
        }
    }
}
