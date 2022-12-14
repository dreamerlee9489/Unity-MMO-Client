using Control.FSM;
using Manage;
using System.Collections;
using UnityEngine;

namespace Control
{
    public class FsmController : GameEntity
    {
        private FsmState _prevState;
        private FsmState _currState;
        private PatrolPath _patrolPath;
        private readonly WaitForSeconds _sleep = new(0.5f);

        public FsmState CurrState => _currState;
        public PatrolPath PatrolPath => _patrolPath;

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
            _currState?.Execute();
        }

        private void OnApplicationQuit()
        {
            PoolManager.Instance.Push(PoolType.PatrolPath, _patrolPath.gameObject);
            _patrolPath = null;
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
                _prevState = _currState;
                _currState.Exit();
                _currState = FsmState.GenState(type, code, this, target);
                _currState.Enter();
            }
        }

        public void ParseEnemy(Proto.Enemy proto)
        {
            transform.position = new Vector3(proto.Pos.X, proto.Pos.Y, proto.Pos.Z);
            gameObject.SetActive(true);
        }

        public FsmStateType GetCurrStateType()
        {
            return _currState.Type;
        }

        public void LinkPlayer(bool isLinker)
        {
            if (isLinker)
            {
                MonoManager.Instance.StartCoroutine(UploadData());
                GameManager.Instance.Canvas.Debug.text = GameManager.Instance.MainPlayer.Name + " is Linking Enemy:" + Id;
                print(GameManager.Instance.MainPlayer.Name + " is Linking Enemy:" + Id);
            }
            else
            {
                MonoManager.Instance.StopCoroutine(UploadData());
                GameManager.Instance.Canvas.Debug.text = "";
                print(GameManager.Instance.MainPlayer.Name + " is dislink Enemy:" + Id);
            }
        }

        private IEnumerator UploadData()
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
