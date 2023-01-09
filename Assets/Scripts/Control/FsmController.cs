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
        public int initHp = 0;
        public State currState;
        public PatrolPath patrolPath;

        protected override void Awake()
        {
            base.Awake();
            agent.speed = runSpeed;
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

        private void OnDestroy()
        {
            _tokenSource.Cancel();
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
                Thread.Sleep(100);
            }
        }

        public void ParseFsmState(StateType type, int code, PlayerController target)
        {
            currState?.Exit();
            currState = State.GenState(type, code, this, target);
            currState.Enter();
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
            player.xp += itemList.Exp;
            player.gold += itemList.Gold;
            UIManager.Instance.GetPanel<PropPanel>().UpdateXp(player.xp);
            UIManager.Instance.GetPanel<KnapPanel>().UpdateGold(player.gold);
            foreach (Proto.ItemData data in itemList.Items)
            {
                switch (data.Type)
                {
                    case Proto.ItemData.Types.ItemType.None:
                        break;
                    case Proto.ItemData.Types.ItemType.Potion:
                        string key1 = (int)ItemType.Potion + "@" + data.Id;
                        ResourceManager.Instance.LoadAsync<GameObject>($"Item/Potion/{potionDict[key1][0]}", (obj) =>
                        {
                            Potion potion = Instantiate(obj).GetComponent<Potion>();
                            potion.itemType = ItemType.Potion;
                            potion.id = data.Id;
                            potion.Sn = data.Sn;
                            potion.ObjName = potionDict[key1][0];
                            potion.SetNameBar(potionDict[key1][1]);
                            potion.transform.position += transform.position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                            GameManager.Instance.CurrWorld.itemDict.Add(data.Sn, potion);
                        });
                        break;
                    case Proto.ItemData.Types.ItemType.Weapon:
                        string key2 = (int)ItemType.Weapon + "@" + data.Id;
                        ResourceManager.Instance.LoadAsync<GameObject>($"Item/Weapon/{weaponDict[key2][0]}", (obj) =>
                        {
                            Weapon weapon = Instantiate(obj).GetComponent<Weapon>();
                            weapon.itemType = ItemType.Weapon;
                            weapon.id = data.Id;
                            weapon.Sn = data.Sn;
                            weapon.ObjName = weaponDict[key2][0];
                            weapon.SetNameBar(weaponDict[key2][1]);
                            weapon.transform.position += transform.position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                            GameManager.Instance.CurrWorld.itemDict.Add(data.Sn, weapon);
                        });
                        break;
                    default:
                        break;
                }
            }
        }

        public void ParseStatus(Proto.SyncEntityStatus proto)
        {
            hp = proto.Hp;
            nameBar.HpBar.UpdateHp(hp, initHp);
        }
    }
}
