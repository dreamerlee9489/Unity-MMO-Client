using Control.FSM;
using Frame;
using UnityEngine;

namespace Control
{
    public class EnemyController : GameEntity
    {
        private AIState _prevState;
        private AIState _currState;
        private PatrolPath _patrolPath;

        public AIState CurrState => _currState;
        public PatrolPath PatrolPath => _patrolPath;

        public int Id { get; set; }

        protected override void Awake()
        {
            base.Awake();
            _patrolPath = PoolManager.Instance.Pop(PoolType.PatrolPath).GetComponent<PatrolPath>();
        }

        private void Start()
        {
            _patrolPath.transform.position = transform.position;
            _currState = new Idle(this);
            _agent.speed = RunSpeed;
        }

        protected override void Update()
        {
            base.Update();
            if (_currState != null)
                _currState.Execute();
        }

        public void ChangeState(AIState newState)
        {
            _prevState = _currState;
            _currState.Exit();
            _currState = newState;
            _currState.Enter();
        }

        public void ChangeState(AIStateType type, int code, PlayerController target)
        {
            if (_currState.type == type)
            {
                _currState.UpdateState(code);
                print(_currState.type + " update code=" + code);
            }
            else
            {
                _prevState = _currState;
                _currState.Exit();
                _currState = AIState.GenState(type, this, target);
                _currState.Enter();
                print(_currState.type + " enter id=" + Id + ", sn=" + target?.Sn);
            }
        }

        public void ParseProto(Proto.Enemy proto)
        {
            _agent.enabled = false;
            transform.position = new Vector3(proto.Pos.X, proto.Pos.Y, proto.Pos.Z);
            _agent.enabled = true;
        }

        public AIStateType GetCurrStateType()
        {
            return _currState.type;
        }
    }
}
