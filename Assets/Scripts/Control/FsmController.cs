using Control.FSM;
using Manage;
using System.Collections;
using UnityEngine;

namespace Control
{
    public class FsmController : GameEntity
    {
        private readonly WaitForSeconds _sleep = new(0.1f);

        public int id = 0;
        public FsmState currState;
        public PatrolPath patrolPath;

        protected override void Awake()
        {
            base.Awake();
            _agent.speed = RunSpeed;
        }

        protected override void Update()
        {
            base.Update();
            currState?.Execute();
        }

        private void OnApplicationQuit()
        {
            ResetState();
            PoolManager.Instance.Push(PoolType.PatrolPath, patrolPath.gameObject);
            patrolPath = null;
        }

        private IEnumerator UploadData()
        {
            while (true)
            {
                Proto.EnemySyncPos proto = new()
                {
                    Id = id,
                    Pos = new()
                    {
                        X = transform.position.x,
                        Y = transform.position.y,
                        Z = transform.position.z
                    }
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SEnemySyncPos, proto);
                yield return _sleep;
            }
        }

        public void ParseSyncState(FsmStateType type, int code, PlayerController target)
        {
            this.target = target != null ? target.transform : null;
            if (currState == null)
            {
                currState = FsmState.GenState(type, code, this, target);
                currState.Enter();
            }
            else
            {
                currState.Exit();
                currState = FsmState.GenState(type, code, this, target);
                currState.Enter();
            }
        }

        public void ParseSyncPos(Proto.EnemySyncPos proto)
        {
            gameObject.SetActive(false);
            transform.position = new Vector3(proto.Pos.X, proto.Pos.Y, proto.Pos.Z);
            gameObject.SetActive(true);
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
            currState.Exit();
            currState = new Idle(this);
            currState.Enter();
        }
    }
}
