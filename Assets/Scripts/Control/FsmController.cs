using Control.FSM;
using Items;
using Manage;
using System.Threading;
using System.Threading.Tasks;
using UI;
using UnityEngine;

namespace Control
{
    public class FsmController : GameEntity
    {
        private readonly Proto.Vector3D _pos = new();
        private readonly CancellationTokenSource _tokenSource = new();

        public int id = 0;
        public State currState;
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

            _pos.X = transform.position.x;
            _pos.Y = transform.position.y;
            _pos.Z = transform.position.z;
        }

        private void OnDisable()
        {
            if(patrolPath != null)
                PoolManager.Instance.Push(PoolType.PatrolPath, patrolPath.gameObject);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _tokenSource.Cancel();
        }

        private void PushPosTask()
        {
            while (!_tokenSource.IsCancellationRequested)
            {
                Proto.PushEnemyPos proto = new()
                {
                    Id = id,
                    Pos = _pos
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SPushEnemyPos, proto);
                Thread.Sleep(100);
            }
        }

        public void ParseSyncState(StateType type, int code, PlayerController target)
        {
            currState?.Exit();
            currState = State.GenState(type, code, this, target);
            currState.Enter();
        }

        public void ParseSyncPos(Proto.PushEnemyPos proto)
        {
            gameObject.SetActive(false);
            transform.position = new Vector3(proto.Pos.X, proto.Pos.Y, proto.Pos.Z);
            gameObject.SetActive(true);
        }

        public void LinkPlayer(bool linker)
        {
            if (linker)
                Task.Run(PushPosTask);
            else
                _tokenSource.Cancel();
        }

        public void ResetState()
        {
            currState.Exit();
            currState = new Idle(this);
            currState.Enter();
        }

        public void DropItems(Proto.DropItemList itemList)
        {
            var player = GameManager.Instance.MainPlayer.Obj;
            var potionDict = GameManager.Instance.DropPotionDict;
            var weaponDict = GameManager.Instance.DropWeaponDict;
            foreach (Proto.ItemData data in itemList.Items)
            {
                switch (data.Type)
                {
                    case Proto.ItemData.Types.ItemType.None:
                        if(data.Id == -1)
                        {
                            player.xp += data.Num;
                            UIManager.Instance.FindPanel<PropPanel>().UpdateXp(player.xp);
                        }
                        else
                        {
                            player.gold += data.Num;
                            UIManager.Instance.FindPanel<KnapPanel>().UpdateGold(player.gold);
                        }
                        break;
                    case Proto.ItemData.Types.ItemType.Potion:
                        for (int i = 0; i < data.Num; ++i)
                        {
                            string key = (int)ItemType.Potion + "@" + data.Id; 
                            ResourceManager.Instance.LoadAsync<GameObject>($"Item/Potion/{potionDict[key][0]}", (obj) =>
                            {
                                Potion potion = Instantiate(obj).GetComponent<Potion>();
                                potion.itemType = ItemType.Potion;
                                potion.ItemId = data.Id;
                                potion.ObjName = potionDict[key][0];
                                potion.SetNameBar(potionDict[key][1]);
                                potion.transform.position += transform.position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                            });
                        }
                        break;
                    case Proto.ItemData.Types.ItemType.Weapon:
                        for (int i = 0; i < data.Num; i++)
                        {
                            string key = (int)ItemType.Weapon + "@" + data.Id;
                            ResourceManager.Instance.LoadAsync<GameObject>($"Item/Weapon/{weaponDict[key][0]}", (obj) =>
                            {
                                Weapon weapon = Instantiate(obj).GetComponent<Weapon>();
                                weapon.itemType = ItemType.Weapon;
                                weapon.ItemId = data.Id;
                                weapon.ObjName = weaponDict[key][0];
                                weapon.SetNameBar(weaponDict[key][1]);
                                weapon.transform.position += transform.position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                            });
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
