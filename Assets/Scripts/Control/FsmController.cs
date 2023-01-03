using Control.FSM;
using Item;
using Manage;
using System.Collections;
using UI;
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
                Proto.EnemyPushPos proto = new()
                {
                    Id = id,
                    Pos = new()
                    {
                        X = transform.position.x,
                        Y = transform.position.y,
                        Z = transform.position.z
                    }
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SEnemyPushPos, proto);
                yield return _sleep;
            }
        }

        public void ParseSyncState(FsmStateType type, int code, PlayerController target)
        {
            this.target = target;
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

        public void ParseSyncPos(Proto.EnemyPushPos proto)
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
                                potion.itemId = data.Id;
                                potion.itemType = ItemType.Potion;
                                potion.objName = potionDict[key][0];
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
                                weapon.itemId = data.Id;
                                weapon.itemType = ItemType.Weapon;
                                weapon.objName = weaponDict[key][0];
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
