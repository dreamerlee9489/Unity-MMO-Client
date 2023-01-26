using Manage;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Control
{
    public class BtController : GameEntity
    {
        private CancellationTokenSource _tokenSource = new();
        private readonly Proto.Vector3D _pos = new();

        public int id = 0;
        public int initHp = 0;
        public PatrolPath patrolPath;

        protected override void Awake()
        {
            base.Awake();
            agent.speed = runSpeed;
            EventManager.Instance.AddListener(EventId.PlayerLoaded, PlayerLoadedCallback);
        }

        protected override void Update()
        {
            base.Update();
            _pos.X = transform.position.x;
            _pos.Y = transform.position.y;
            _pos.Z = transform.position.z;
        }

        private void OnDisable()
        {
            _tokenSource.Cancel();
            if (patrolPath != null)
                PoolManager.Instance.Push(PoolType.PatrolPath, patrolPath.gameObject);
        }

        private void OnDestroy()
        {
            EventManager.Instance.RemoveListener(EventId.PlayerLoaded, PlayerLoadedCallback);
        }

        private void PlayerLoadedCallback()
        {
            Proto.ReqSyncNpc proto = new()
            {
                NpcId = id,
                NpcSn = Sn
            };
            NetManager.Instance.SendPacket(Proto.MsgId.C2SReqSyncNpc, proto);
        }

        private void PushPosTask()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                Proto.SyncNpcPos proto = new()
                {
                    NpcSn = Sn,
                    Pos = _pos
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SSyncNpcPos, proto);
                Thread.Sleep(500);
            }
        }

        public void ParsePos(Proto.Vector3D pos)
        {
            gameObject.SetActive(false);
            transform.position = new Vector3(pos.X, pos.Y, pos.Z);
            gameObject.SetActive(true);
        }

        public void LinkPlayer(bool linker)
        {
            if (linker)
                Task.Run(PushPosTask, (_tokenSource = new()).Token);
            else
                _tokenSource.Cancel();
        }
    }
}
