using Control.BT;
using Items;
using Manage;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.AI;

namespace Control
{
    public class BtController : GameEntity
    {
        private readonly WaitForSeconds _sleep = new(0.1f);

        public int id = 0, initHp = 0;
        public bool isLinker = false;
        public Vector3 initPos = Vector3.zero;
        public PatrolPath patrolPath = null;
        public Selector root = null;

        protected override void Awake()
        {
            base.Awake();
            Agent.speed = runSpeed;
            EventManager.Instance.AddListener(EventId.PlayerLoaded, PlayerLoadedCallback);
        }

        private void Start()
        {
            root = new Selector(this);
            root.AddChild(new ActionBirth(this));
            root.AddChild(new ActionDeath(this));
            root.AddChild(new ActionIdle(this));
            root.AddChild(new ActionPatrol(this));
            root.AddChild(new ActionPursue(this));
            root.AddChild(new ActionAttack(this));
            root.AddChild(new ActionFlee(this));
            MonoManager.Instance.StartCoroutine(TickRoot());
        }

        protected override void Update()
        {
            base.Update();
            if (cornerPoints.Count > 0)
            {
                Agent.destination = new()
                {
                    x = cornerPoints[^1].X,
                    y = cornerPoints[^1].Y,
                    z = cornerPoints[^1].Z
                };
                cornerPoints.Clear();
            }
        }

        private void OnDestroy()
        {
            EventManager.Instance.RemoveListener(EventId.PlayerLoaded, PlayerLoadedCallback);
        }

        private IEnumerator TickRoot()
        {
            while (true)
            {
                root.Tick();
                yield return _sleep;
            }
        }

        private void PlayerLoadedCallback()
        {
            Proto.SyncBtAction proto = new()
            {
                Id = id,
                NpcSn = Sn
            };
            NetManager.Instance.SendPacket(Proto.MsgId.C2SSyncBtAction, proto);
        }

        public void LinkPlayer(bool linker) => isLinker = linker;

        public void ReqMoveTo(Vector3 hitPoint, bool isRun)
        {
            if (isLinker)
            {
                Vector3 dstPoint = hitPoint;
                if (NavMesh.SamplePosition(hitPoint, out var meshHit, 100, 1 << NavMesh.GetAreaFromName("Walkable")))
                    dstPoint = meshHit.position;
                NavMeshPath path = new();
                Agent.CalculatePath(dstPoint, path);
                if (path.status != NavMeshPathStatus.PathPartial)
                {
                    Proto.EntityMove proto = new() { Sn = Sn, Running = isRun };
                    foreach (Vector3 point in path.corners)
                        proto.Points.Add(new Proto.Vector3D() { X = point.x, Y = point.y, Z = point.z });
                    NetManager.Instance.SendPacket(Proto.MsgId.C2SNpcMove, proto);
                }
            }
        }

        public void ParseStatus(Proto.SyncEntityStatus proto)
        {
            hp = proto.Hp;
            NameBar.HpBar.UpdateHp(hp, initHp);
        }       

        public void DropItems(Proto.DropItemList itemList)
        {
            var player = GameManager.Instance.mainPlayer.Obj;
            var potionDict = GameManager.Instance.dropPotionDict;
            var weaponDict = GameManager.Instance.dropWeaponDict;
            player.xp += itemList.Exp;
            player.gold += itemList.Gold;
            UIManager.Instance.GetPanel<PropPanel>().UpdateXp(player.xp);
            UIManager.Instance.GetPanel<BagPanel>().UpdateGold(player.gold);
            foreach (Proto.ItemData data in itemList.Items)
            {
                switch (data.ItemType)
                {
                    case Proto.ItemData.Types.ItemType.None:
                        break;
                    case Proto.ItemData.Types.ItemType.Potion:
                        string key1 = (int)ItemType.Potion + "@" + data.Id;
                        ResourceManager.Instance.LoadAsync<GameObject>($"Item/Potion/{potionDict[key1][0]}", (obj) =>
                        {
                            Potion potion = Instantiate(obj).GetComponent<Potion>();
                            potion.itemType = ItemType.Potion;
                            potion.knapType = KnapType.World;
                            potion.id = data.Id;
                            potion.Sn = data.Sn;
                            potion.ObjName = potionDict[key1][0];
                            potion.SetNameBar(potionDict[key1][1]);
                            potion.transform.position += transform.position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                            GameManager.currWorld.itemDict.Add(data.Sn, potion);
                        });
                        break;
                    case Proto.ItemData.Types.ItemType.Weapon:
                        string key2 = (int)ItemType.Weapon + "@" + data.Id;
                        ResourceManager.Instance.LoadAsync<GameObject>($"Item/Weapon/{weaponDict[key2][0]}", (obj) =>
                        {
                            Weapon weapon = Instantiate(obj).GetComponent<Weapon>();
                            weapon.itemType = ItemType.Weapon;
                            weapon.knapType = KnapType.World;
                            weapon.id = data.Id;
                            weapon.Sn = data.Sn;
                            weapon.ObjName = weaponDict[key2][0];
                            weapon.SetNameBar(weaponDict[key2][1]);
                            weapon.transform.position += transform.position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                            GameManager.currWorld.itemDict.Add(data.Sn, weapon);
                        });
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
