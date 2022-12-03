using Control.FSM;
using Frame;
using System.Collections;
using UnityEngine;

namespace Control
{
    public class EnemyController : GameEntity
    {
        private FsmState _prevState;
        private FsmState _currState;
        private PatrolPath _patrolPath;

        public FsmState CurrState => _currState;
        public PatrolPath PatrolPath => _patrolPath;

        public int Id { get; set; }

        protected override void Awake()
        {
            base.Awake();
            _agent.speed = RunSpeed;
            _patrolPath = PoolManager.Instance.Pop(PoolType.PatrolPath).GetComponent<PatrolPath>();
            _patrolPath.transform.position = transform.position;
        }

        private void Start()
        {
            //_currState = new Idle(this);
        }

        protected override void Update()
        {
            base.Update();
            if (_currState != null)
                _currState.Execute();
        }

        public void ChangeState(FsmState newState)
        {
            _prevState = _currState;
            _currState.Exit();
            _currState = newState;
            _currState.Enter();
        }

        public void ParseSyncState(FsmStateType type, int code, PlayerController target)
        {
            if (_currState == null)
            {
                _currState = FsmState.GenState(type, code, this, target);
                _currState.Enter();
                print("SyncState null: type=" + type + " code=" + code);
                return;
            }
            else if (_currState.Type == type)
            {
                _currState.UpdateState(code);
                print(_currState.Type + " update code=" + code);
            }
            else
            {
                _prevState = _currState;
                _currState.Exit();
                _currState = FsmState.GenState(type, code, this, target);
                _currState.Enter();
                print(_currState.Type + " enter id=" + Id + ", sn=" + target?.Sn + ", code=" + code);
            }
        }

        public void ParseEnemy(Proto.Enemy proto)
        {
            _agent.enabled = false;
            transform.position = new Vector3(proto.Pos.X, proto.Pos.Y, proto.Pos.Z);
            _agent.enabled = true;
        }

        public FsmStateType GetCurrStateType()
        {
            return _currState.Type;
        }
    }
}
