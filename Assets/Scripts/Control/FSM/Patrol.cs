using Frame;
using Net;

namespace Control.FSM
{
    public class Patrol : FsmState
    {
        private int _index;
        private PatrolPath _patrolPath;

        public Patrol(EnemyController owner, PlayerController target = null, int code = 0) : base(owner, target)
        {
            _type = FsmStateType.Patrol;
            _index = code;
            Enter();
        }

        public override void Enter()
        {
            _patrolPath = _owner.PatrolPath;
            _owner.Anim.SetBool(GameEntity.Attack, false);
            _owner.Agent.speed = _owner.WalkSpeed;
            _owner.Agent.destination = _patrolPath.Path[_index].position;
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
            }
        }

        public override void Exit()
        {
            if (_owner.IsLinker)
            {
                _owner.IsLinker = false;
                _owner.IsLinking = false;
                MonoManager.Instance.StopCoroutine(_owner.UploadData());
            }
        }

        public override void UpdateState(int code)
        {
            _owner.Agent.destination = _patrolPath.Path[_index = code].position;
        }
    }
}
