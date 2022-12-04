using Control.FSM;
using Frame;
using Net;
using System.Collections;
using UnityEngine;

namespace Control
{
    public class EnemyController : GameEntity
    {
        private FsmState _prevState;
        private FsmState _currState;
        private PatrolPath _patrolPath;
        private WaitForSeconds _sleep = new(0.5f);

        public FsmState CurrState => _currState;
        public PatrolPath PatrolPath => _patrolPath;
        public bool IsLinker { get; set; }
        public bool IsLinking { get; set; }

        public int Id { get; set; }

        protected override void Awake()
        {
            base.Awake();
            _agent.speed = RunSpeed;
        }

        private void Start()
        {
            _patrolPath = PoolManager.Instance.Pop(PoolType.PatrolPath).GetComponent<PatrolPath>();
            _patrolPath.transform.position = transform.position;
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
                return;
            }
            if (_currState.Type == type)
                _currState.UpdateState(code);
            else
            {
                _prevState = _currState;
                _currState.Exit();
                _currState = FsmState.GenState(type, code, this, target);
                _currState.Enter();
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

        public void LinkPlayer() => IsLinker = true;

        public IEnumerator UploadData()
        {
            while (true)
            {
                Proto.Enemy proto = new()
                {
                    Id = Id,
                    Pos = new()
                    {
                        X = transform.position.x,
                        Y = transform.position.y,
                        Z = transform.position.z
                    }
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SEnemy, proto);
                yield return _sleep;
            }
        }
    }
}
