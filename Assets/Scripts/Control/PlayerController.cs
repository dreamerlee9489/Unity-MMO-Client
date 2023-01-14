using Control.CMD;
using Items;
using Manage;
using System.Threading;
using System.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace Control
{
    public partial class PlayerController : GameEntity, IMover, IAttacker, IPicker, ITeleporter, IObserver, ILiver
    {
        private readonly Proto.Vector3D _curPos = new();
        private readonly Proto.Vector3D _hitPos = new();

        private RaycastHit _hit;
        private ICommand _cmd;
        private Transform _knapsack, _handPos;

        public int xp = 0, gold = 0;
        public static PlayerBaseData baseData;
        public static CancellationTokenSource tokenSource = new();

        protected override void Awake()
        {
            base.Awake();
            EventManager.Instance.AddListener(EEventType.PlayerLoaded, PlayerLoadedCallback);
        }

        private void OnEnable()
        {
            if (Sn == GameManager.Instance.mainPlayer.Sn)
                Task.Run(SyncPosTask, (tokenSource = new()).Token);
        }

        private void Start()
        {
            agent.speed = runSpeed * 1.5f;
            baseData ??= GameManager.Instance.playerBaseDatas[lv];
            if (Sn == GameManager.Instance.mainPlayer.Sn)
            {
                _knapsack = transform.Find("Knapsack");
                _handPos = transform.Find("HandPos");
                nameBar.HpBar.gameObject.SetActive(false);
                gameObject.layer = 2;
                TeamManager.Instance.Initial();
                NetManager.Instance.SendPacket(Proto.MsgId.C2SGetPlayerKnap, null);
            }
        }

        protected override void Update()
        {
            base.Update();
            _cmd?.Execute();

            if (Sn != GameManager.Instance.mainPlayer.Sn || EventSystem.current.IsPointerOverGameObject())
                return;

            _curPos.X = transform.position.x;
            _curPos.Y = transform.position.y;
            _curPos.Z = transform.position.z;

            if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit))
            {
                _hitPos.X = _hit.point.x;
                _hitPos.Y = _hit.point.y;
                _hitPos.Z = _hit.point.z;

                Proto.SyncPlayerCmd syncCmd = new();
                switch (_hit.collider.tag)
                {
                    case "Terrain":
                        syncCmd.Type = 1;
                        syncCmd.PlayerSn = Sn;
                        syncCmd.TargetSn = 0;
                        syncCmd.Point = _hitPos;
                        break;
                    case "Enemy":
                        syncCmd.Type = 2;
                        syncCmd.PlayerSn = Sn;
                        if (_hit.transform.GetComponent<FsmController>() != null)
                            syncCmd.TargetSn = _hit.transform.GetComponent<FsmController>().Sn;
                        else
                            syncCmd.TargetSn = _hit.transform.GetComponent<PlayerController>().Sn;
                        syncCmd.Point = null;
                        break;
                    case "Item":
                        syncCmd.Type = 3;
                        syncCmd.PlayerSn = Sn;
                        syncCmd.TargetSn = _hit.transform.GetComponent<GameItem>().Sn;
                        syncCmd.Point = _hitPos;
                        break;
                    case "Portal":
                        syncCmd.Type = 4;
                        syncCmd.PlayerSn = Sn;
                        syncCmd.TargetSn = _hit.transform.GetComponent<GameItem>().Sn;
                        syncCmd.Point = _hitPos;
                        break;
                    case "Player":
                        syncCmd.Type = 5;
                        syncCmd.PlayerSn = Sn;
                        syncCmd.TargetSn = _hit.transform.GetComponent<PlayerController>().Sn;
                        syncCmd.Point = null;
                        break;
                }
                NetManager.Instance.SendPacket(Proto.MsgId.C2SSyncPlayerCmd, syncCmd);
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Proto.PlayerAtkEvent proto = new()
                {
                    PlayerSn = Sn,
                    TargetSn = 0,
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SPlayerAtkEvent, proto);
            }
        }

        private void OnApplicationQuit()
        {
            if (Sn == GameManager.Instance.mainPlayer.Sn)
                TeamManager.Instance.Destroy();
        }

        private void OnDisable()
        {
            if (Sn == GameManager.Instance.mainPlayer.Sn)
                tokenSource.Cancel();
        }

        private void OnDestroy()
        {
            EventManager.Instance.RemoveListener(EEventType.PlayerLoaded, PlayerLoadedCallback);
        }

        private void PlayerLoadedCallback()
        {
            if (Sn != GameManager.Instance.mainPlayer.Sn)
            {
                Proto.ReqSyncPlayer proto = new() { PlayerSn = Sn };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SReqSyncPlayer, proto);
            }
        }

        private void SyncPosTask()
        {
            Thread.Sleep(1000);
            while (!tokenSource.IsCancellationRequested)
            {
                Proto.SyncPlayerPos syncPos = new() { Pos = _curPos };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SSyncPlayerPos, syncPos);
                Thread.Sleep(1000);
            }
        }

        public void ResetCmd() => _cmd?.Undo();

        public void ParseCmd(Proto.SyncPlayerCmd proto)
        {
            CommandType newType = (CommandType)proto.Type;
            if (_cmd != null && _cmd.GetCommandType() != newType)
                _cmd.Undo();
            switch (newType)
            {
                case CommandType.None:
                    _cmd?.Undo();
                    break;
                case CommandType.Move:
                    target = null;
                    _cmd = new MoveCommand(this, new(proto.Point.X, proto.Point.Y, proto.Point.Z));
                    break;
                case CommandType.Attack:
                    if(GameManager.currWorld.npcDict.ContainsKey(proto.TargetSn))
                        target = GameManager.currWorld.npcDict[proto.TargetSn].transform;
                    else
                        target = GameManager.currWorld.roleDict[proto.TargetSn].obj.transform;
                    _cmd = new AttackCommand(this, target);
                    break;
                case CommandType.Pickup:
                    if (!GameManager.currWorld.itemDict.ContainsKey(proto.TargetSn))
                        _cmd = new MoveCommand(this, new(proto.Point.X, proto.Point.Y, proto.Point.Z));
                    else
                    {
                        target = GameManager.currWorld.itemDict[proto.TargetSn].transform;
                        _cmd = new PickupCommand(this, target);
                    }
                    break;
                case CommandType.Teleport:
                    target = GameManager.currWorld.itemDict[proto.TargetSn].transform;
                    _cmd = new TeleportCommand(this, target);
                    break;
                case CommandType.Dialog:
                    target = GameManager.currWorld.roleDict[proto.TargetSn].obj.transform;
                    _cmd = new ObserveCommand(this, target);
                    break;
                case CommandType.Death:
                    string atkName;
                    if (GameManager.currWorld.roleDict.ContainsKey(proto.TargetSn))
                    {
                        var tmp = GameManager.currWorld.roleDict[proto.TargetSn].obj;
                        target = tmp.transform;
                        atkName = tmp.GetNameBar();
                    }
                    else
                    {
                        var tmp = GameManager.currWorld.npcDict[proto.TargetSn];
                        target = tmp.transform;
                        atkName = tmp.GetNameBar();
                    }
                    _cmd = new DeathCommand(this, target, atkName);
                    break;
                default:
                    break;
            }
        }

        public int AddItemToBag(GameItem item, int index = 0)
        {
            item.gameObject.SetActive(false);
            item.transform.SetParent(_knapsack);
            GameObject obj = ResourceManager.Instance.Load<GameObject>($"UI/ItemUI/{item.itemType}/{item.ObjName}");
            ItemUI itemUI = Instantiate(obj).GetComponent<ItemUI>();
            itemUI.Item = item;
            item.ItemUI = itemUI;
            PoolManager.Instance.Push(item.ObjName, itemUI.gameObject);
            return itemUI.AddToBagUI(index);
        }

        public void ParsePlayerKnap(Proto.PlayerKnap playerKnap)
        {
            gold = playerKnap.Gold;
            UIManager.Instance.GetPanel<KnapPanel>().UpdateGold(gold);
            var potionDict = GameManager.Instance.dropPotionDict;
            var weaponDict = GameManager.Instance.dropWeaponDict;
            foreach (Proto.ItemData data in playerKnap.BagItems)
            {
                switch (data.Type)
                {
                    case Proto.ItemData.Types.ItemType.None:
                        break;
                    case Proto.ItemData.Types.ItemType.Potion:
                        string key1 = new((int)ItemType.Potion + "@" + data.Id);
                        ResourceManager.Instance.LoadAsync<GameObject>($"Item/Potion/{potionDict[key1][0]}", (obj) =>
                        {
                            Potion potion = Instantiate(obj).GetComponent<Potion>();
                            potion.itemType = ItemType.Potion;
                            potion.id = data.Id;
                            potion.Sn = data.Sn;
                            potion.ObjName = potionDict[key1][0];
                            potion.SetNameBar(potionDict[key1][1]);
                            potion.transform.position = transform.position;
                            AddItemToBag(potion, data.Index);
                        });
                        break;
                    case Proto.ItemData.Types.ItemType.Weapon:
                        string key2 = new((int)ItemType.Weapon + "@" + data.Id);
                        ResourceManager.Instance.LoadAsync<GameObject>($"Item/Weapon/{weaponDict[key2][0]}", (obj) =>
                        {
                            Weapon weapon = Instantiate(obj).GetComponent<Weapon>();
                            weapon.itemType = ItemType.Weapon;
                            weapon.id = data.Id;
                            weapon.Sn = data.Sn;
                            weapon.ObjName = weaponDict[key2][0];
                            weapon.SetNameBar(weaponDict[key2][1]);
                            weapon.transform.position = transform.position;
                            AddItemToBag(weapon, data.Index);
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
            if (CompareTag("Enemy"))
                nameBar.HpBar.UpdateHp(hp, baseData.hp);
            if (GameManager.Instance.mainPlayer.Sn == Sn)
                UIManager.Instance.GetPanel<PropPanel>().UpdateHp(hp);
            if (TeamManager.Instance.teamDict.ContainsKey(Sn))
                TeamManager.Instance.teamDict[Sn].UpdateHp(hp);
        }

        public void Move(Vector3 point)
        {
            agent.destination = point;
            if(Vector3.Distance(transform.position, point) <= agent.stoppingDistance)
                _cmd = null;
        }

        public void UnMove()
        {
        }

        public void Attack(Transform target)
        {
            transform.LookAt(target);
            if (Vector3.Distance(transform.position, target.position) <= attackRadius)
                anim.SetBool(attack, true);
            else
            {
                anim.SetBool(attack, false);
                agent.destination = target.position;
            }
        }

        public void UnAttack()
        {
            anim.SetBool(attack, false);
            _cmd = null;
        }

        public void Pickup(Transform item)
        {
            agent.destination = item.position;
            if (Vector3.Distance(transform.position, item.position) <= agent.stoppingDistance)
            {
                anim.SetTrigger(pickup);
                _cmd = null;
            }
        }

        public void UnPickup()
        {
        }

        public void Teleport(Transform portal)
        {
            agent.destination = portal.position;
            if (Sn == GameManager.Instance.mainPlayer.Sn && Vector3.Distance(transform.position, portal.position) <= agent.stoppingDistance)
            {
                portal.GetComponent<Portal>().OpenDoor(this);
                _cmd = null;
            }
        }

        public void UnTeleport()
        {
        }

        public void Observe(Transform target)
        {
            agent.destination = target.position;
            if (Vector3.Distance(transform.position, target.position) <= agent.stoppingDistance)
            {
                _cmd = null;
                agent.destination = transform.position;
                if (Sn == GameManager.Instance.mainPlayer.Sn)
                {
                    ObservePanel panel = UIManager.Instance.GetPanel<ObservePanel>();
                    panel.SetPlayer(target.GetComponent<PlayerController>());
                    panel.Open();
                } 
            }
        }

        public void UnObserve()
        {
        }

        public void Die(string atkName)
        {
            _cmd = null;
            agent.radius = 0;
            agent.isStopped = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            anim.SetBool(death, true);
            GetComponent<CapsuleCollider>().enabled = false;
            if (Sn == GameManager.Instance.mainPlayer.Sn)
                UIManager.Instance.GetPanel<PopupPanel>().Open($"你被[{atkName}]击杀了。", null, null);
        }

        public void Rebirth()
        {
            agent.radius = 0.3f;
            agent.isStopped = false;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            anim.SetBool(death, false);
            GetComponent<CapsuleCollider>().enabled = true;
            _cmd = null;
        }

        public void ParseEnterDungeon(Proto.EnterDungeon proto, string dungeon)
        {
            string text = $"玩家[{proto.Sender}]邀请你进入副本[{dungeon}]，是否同意？";
            UIManager.Instance.GetPanel<PopupPanel>().Open(text, () =>
            {
                Proto.EnterDungeon protoRes = new()
                { 
                    WorldId = proto.WorldId,
                    WorldSn = proto.WorldSn,
                    Agree = true
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2CEnterDungeonRes, protoRes);
            }, null);
        }

        public void ParseReqPvp(Proto.Pvp proto, string atker)
        {
            string text = $"玩家[{atker}]向你发起挑战，是否同意？";
            UIManager.Instance.GetPanel<PopupPanel>().Open(text, () =>
            {
                Proto.Pvp protoRes = new()
                { 
                    Atker = proto.Atker,
                    Defer = Sn,
                    Agree = true
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2CPvpRes, protoRes);
            }, null);
        }

        public void ParsePvpRes(Proto.Pvp proto)
        {
            if(proto.Agree)
            {
                if (Sn == proto.Atker)
                {
                    var defer = GameManager.currWorld.roleDict[proto.Defer].obj;
                    defer.tag = "Enemy";
                    defer.nameBar.ChangeCamp();
                }
                else if(Sn == proto.Defer)
                {
                    var atker = GameManager.currWorld.roleDict[proto.Atker].obj;
                    atker.tag = "Enemy";
                    atker.nameBar.ChangeCamp();
                }
            }
        }
    }
}
