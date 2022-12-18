using Control.FSM;
using Manage;
using System.Collections;
using UnityEngine;

namespace Control
{
    public class FsmController : GameEntity
    {
        private readonly WaitForSeconds _sleep = new(0.5f);

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
            PoolManager.Instance.Push(PoolType.PatrolPath, patrolPath.gameObject);
            patrolPath = null;
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
            currState.Exit();
            currState = new Idle(this);
            currState.Enter();
        }
    }
}
