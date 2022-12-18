using Control.FSM;
using Manage;
using System.Collections;
using UnityEngine;

namespace Control
{
    public class FsmController : GameEntity
    {
        private FsmState _currState;
        private PatrolPath _patrolPath;
        private readonly WaitForSeconds _sleep = new(0.5f);

        public FsmState CurrState => _currState;
        public PatrolPath PatrolPath => _patrolPath;

        public int id = 0;

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
            _currState?.Execute();
        }

        private void OnApplicationQuit()
        {
            PoolManager.Instance.Push(PoolType.PatrolPath, _patrolPath.gameObject);
            _patrolPath = null;
        }

        private IEnumerator UploadData()
        {
            while (true)
            {
                Net.Enemy proto = new()
                {
                    Id = id,
                    Pos = new()
                    {
                        X = transform.position.x,
                        Y = transform.position.y,
                        Z = transform.position.z
                    }
                };
                NetManager.Instance.SendPacket(Net.MsgId.C2SEnemy, proto);
                yield return _sleep;
            }
        }

        public void ParseSyncState(FsmStateType type, int code, PlayerController target)
        {
            if (_currState == null)
            {
                _currState = FsmState.GenState(type, code, this, target);
                _currState.Enter();
                return;
            }
            if (_currState.Type != type)
            {
                _currState.Exit();
                _currState = FsmState.GenState(type, code, this, target);
                _currState.Enter();
            }
        }

        public void ParseEnemy(Net.Enemy proto)
        {
            transform.position = new Vector3(proto.Pos.X, proto.Pos.Y, proto.Pos.Z);
        }

        public void LinkPlayer(bool isLinker)
        {
            if (isLinker)
                MonoManager.Instance.StartCoroutine(UploadData());
            else
                MonoManager.Instance.StopCoroutine(UploadData());
        }

        public void ResetState()
        {
            _currState.Exit();
            _currState = new Idle(this);
            _currState.Enter();
        }
    }
}
