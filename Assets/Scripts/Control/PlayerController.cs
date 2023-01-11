using Control.CMD;
using Items;
using Manage;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Control
{
    public partial class PlayerController : GameEntity, IMover, IAttacker, IPicker, ITeleporter, IObserver
    {
        private readonly Proto.Vector3D _curPos = new();
        private readonly Proto.Vector3D _hitPos = new();

        private RaycastHit _hit;
        private ICommand _cmd;
        private Transform _knapsack, _handPos;
        private HUDPanel hudPanel;
        
        public int xp = 0, gold = 0;
        public PlayerBaseData baseData;
        public WorldManager currWorld;
        public CancellationTokenSource tokenSource = new();
        public List<ulong> team = new();

        protected override void Awake()
        {
            base.Awake();
            EventManager.Instance.AddListener(EEventType.PlayerLoaded, PlayerLoadedCallback);
        }

        private void OnEnable()
        {
            if (Sn == GameManager.Instance.MainPlayer.Sn)
                Task.Run(SyncPosTask, (tokenSource = new()).Token);
        }

        private void Start()
        {
            agent.speed = runSpeed * 1.5f;
            currWorld = GameManager.Instance.CurrWorld;
            baseData = GameManager.Instance.PlayerBaseDatas[lv];
            if (Sn == GameManager.Instance.MainPlayer.Sn)
            {
                _knapsack = transform.Find("Knapsack");
                _handPos = transform.Find("HandPos");
                nameBar.HpBar.gameObject.SetActive(false);
                PoolManager.Instance.LoadPush(PoolType.HUDPanel, "UI/Panel/HUDPanel", 4);
                hudPanel = PoolManager.Instance.Pop(PoolType.HUDPanel, UIManager.Instance.hudGroup).GetComponent<HUDPanel>();
                hudPanel.InitPanel(currWorld.roleDict[Sn]);
                gameObject.layer = 2;
                NetManager.Instance.SendPacket(Proto.MsgId.C2SGetPlayerKnap, null);
            }
        }

        protected override void Update()
        {
            base.Update();
            _cmd?.Execute();

            if (Sn != GameManager.Instance.MainPlayer.Sn || EventSystem.current.IsPointerOverGameObject())
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
                        syncCmd.TargetSn = _hit.transform.GetComponent<FsmController>().Sn;
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
                    PlayerSn = GameManager.Instance.MainPlayer.Sn,
                    TargetSn = 0,
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SPlayerAtkEvent, proto);
            }
        }

        private void OnDisable()
        {
            if (Sn == GameManager.Instance.MainPlayer.Sn)
                tokenSource.Cancel();
        }

        private void OnDestroy()
        {
            EventManager.Instance.RemoveListener(EEventType.PlayerLoaded, PlayerLoadedCallback);
        }

        private void PlayerLoadedCallback()
        {
            if (Sn != GameManager.Instance.MainPlayer.Sn)
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
                    target = currWorld.npcDict[proto.TargetSn].transform;
                    _cmd = new AttackCommand(this, target);
                    break;
                case CommandType.Pickup:
                    if (!currWorld.itemDict.ContainsKey(proto.TargetSn))
                        _cmd = new MoveCommand(this, new(proto.Point.X, proto.Point.Y, proto.Point.Z));
                    else
                    {
                        target = currWorld.itemDict[proto.TargetSn].transform;
                        _cmd = new PickupCommand(this, target);
                    }
                    break;
                case CommandType.Teleport:
                    target = currWorld.itemDict[proto.TargetSn].transform;
                    _cmd = new TeleportCommand(this, target);
                    break;
                case CommandType.Dialog:
                    target = currWorld.roleDict[proto.TargetSn].obj.transform;
                    _cmd = new ObserveCommand(this, target);
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
            var potionDict = GameManager.Instance.DropPotionDict;
            var weaponDict = GameManager.Instance.DropWeaponDict;
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
            if (GameManager.Instance.MainPlayer.Sn == proto.Sn)
                UIManager.Instance.GetPanel<PropPanel>().UpdateHp(hp);
            for (int i = 0; i < UIManager.Instance.hudGroup.childCount; i++)
            {
                HUDPanel panel = UIManager.Instance.hudGroup.GetChild(i).GetComponent<HUDPanel>();
                if(panel.player == this)
                    panel.UpdateHp(hp);
            }
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
            if (Sn == GameManager.Instance.MainPlayer.Sn && Vector3.Distance(transform.position, portal.position) <= agent.stoppingDistance)
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
                if (Sn == GameManager.Instance.MainPlayer.Sn)
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

        public void ParseJoinTeam(Proto.ReqJoinTeam proto)
        {
            string text = $"玩家[{currWorld.roleDict[proto.Applicant].name}]申请入队，是否同意？";
            UIManager.Instance.GetPanel<PopupPanel>().Open(text, () =>
            {
                if (team.Count == 0)
                    team.Add(Sn);
                team.Add(proto.Applicant);
                hudPanel.teamTxt.text = "队长";
                HUDPanel panel = PoolManager.Instance.Pop(PoolType.HUDPanel, UIManager.Instance.hudGroup).GetComponent<HUDPanel>();
                panel.InitPanel(currWorld.roleDict[proto.Applicant], "队员");
                currWorld.roleDict[proto.Applicant].obj.hudPanel = panel;
                Proto.JoinTeamRes joinRes = new()
                {
                    Applicant = proto.Applicant,
                    Responder = Sn,
                    Agree = true
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2CJoinTeamRes, joinRes);                        
            }, () =>
            {
                Proto.JoinTeamRes joinRes = new()
                {
                    Applicant = proto.Applicant,
                    Responder = Sn,
                    Agree = false
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2CJoinTeamRes, joinRes);
            });
        }

        public void ParseJoinTeamRes(Proto.JoinTeamRes proto)
        {
            if (!proto.Agree)
            {
                string text = $"玩家[{currWorld.roleDict[proto.Responder].name}]拒绝了你的入队请求。";
                UIManager.Instance.GetPanel<PopupPanel>().Open(text, null, null);
            }
            else
            {
                hudPanel = null;
                team.Clear();
                int count = UIManager.Instance.hudGroup.childCount;
                for (int i = 0; i < count; i++)
                    PoolManager.Instance.Push(PoolType.HUDPanel, UIManager.Instance.hudGroup.GetChild(0).gameObject);
                for (int i = 0; i < proto.Members.Count; i++)
                {
                    team.Add(proto.Members[i].MemberSn);
                    HUDPanel panel = PoolManager.Instance.Pop(PoolType.HUDPanel, UIManager.Instance.hudGroup).GetComponent<HUDPanel>();
                    panel.InitPanel(currWorld.roleDict[proto.Members[i].MemberSn], i == 0 ? "队长" : "队员");
                    currWorld.roleDict[proto.Members[i].MemberSn].obj.hudPanel = panel;
                }
            }
        }
    }
}
