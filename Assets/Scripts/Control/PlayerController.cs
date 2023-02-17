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
        private RaycastHit _hit;
        private ICommand _cmd;
        private readonly Proto.Vector3D _curPos = new();
        private readonly Proto.Vector3D _hitPos = new();
        private CancellationTokenSource _tokenSource = new();

        public int xp = 0, gold = 0;
        public static PlayerBaseData baseData;
        
        public Transform BagPoint { get; set; }
        public Transform HandPoint { get; set; }
        public Transform ActPoint { get; set; }
        public Transform TradePoint { get; set; }

        private void OnEnable()
        {
            if (Sn == GameManager.Instance.mainPlayer.Sn)
                Task.Run(SyncPosTask, (_tokenSource = new()).Token);
        }

        private void Start()
        {
            baseData ??= GameManager.Instance.playerBaseDatas[lv];
            if (Sn == GameManager.Instance.mainPlayer.Sn)
            {
                BagPoint = transform.Find("BagPoint");
                HandPoint = transform.Find("HandPoint");
                ActPoint = transform.Find("ActPoint");
                TradePoint = transform.Find("TradePoint");
                NameBar.HpBar.gameObject.SetActive(false);
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
                        if (_hit.transform.GetComponent<BtController>() != null)
                            syncCmd.TargetSn = _hit.transform.GetComponent<BtController>().Sn;
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

        private void OnDisable()
        {
            if (Sn == GameManager.Instance.mainPlayer.Sn)
                _tokenSource.Cancel();
        }

        private void SyncPosTask()
        {
            Thread.Sleep(1000);
            while (!_tokenSource.IsCancellationRequested)
            {
                Proto.SyncPlayerPos syncPos = new() { Pos = _curPos };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SSyncPlayerPos, syncPos);
                Thread.Sleep(200);
            }
        }

        public void ResetCmd() => _cmd?.Undo();          

        public void Move(Vector3 point)
        {
            Agent.destination = point;
            if(Vector3.Distance(transform.position, point) <= Agent.stoppingDistance)
                _cmd = null;
        }

        public void UnMove()
        {
        }

        public void Attack(Transform target)
        {
            transform.LookAt(target);
            if (Vector3.Distance(transform.position, target.position) <= attackRadius)
                Anim.SetBool(attack, true);
            else
            {
                Anim.SetBool(attack, false);
                Agent.destination = target.position;
            }
        }

        public void UnAttack()
        {
            Anim.SetBool(attack, false);
            _cmd = null;
        }

        public void Pickup(Transform item)
        {
            Agent.destination = item.position;
            if (Vector3.Distance(transform.position, item.position) <= Agent.stoppingDistance)
            {
                Anim.SetTrigger(pickup);
                _cmd = null;
            }
        }

        public void UnPickup()
        {
        }

        public void Teleport(Transform portal)
        {
            Agent.destination = portal.position;
            if (Sn == GameManager.Instance.mainPlayer.Sn && Vector3.Distance(transform.position, portal.position) <= Agent.stoppingDistance)
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
            Agent.destination = target.position;
            if (Vector3.Distance(transform.position, target.position) <= Agent.stoppingDistance)
            {
                _cmd = null;
                Agent.destination = transform.position;
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
            Agent.radius = 0;
            Agent.isStopped = true;
            Agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            Anim.SetBool(death, true);
            GetComponent<CapsuleCollider>().enabled = false;
            if (Sn == GameManager.Instance.mainPlayer.Sn)
                UIManager.Instance.GetPanel<PopPanel>().Open($"你被[{atkName}]击杀了。", null, null);
        }

        public void Rebirth()
        {
            Agent.radius = 0.3f;
            Agent.isStopped = false;
            Agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            Anim.SetBool(death, false);
            GetComponent<CapsuleCollider>().enabled = true;
            _cmd = null;
        }

        public int AddItemToKnap(KnapType knapType, GameItem item, int index = 0)
        {
            if (item.ItemUI == null)
            {
                GameObject obj = ResourceManager.Instance.Load<GameObject>($"UI/ItemUI/{item.itemType}/{item.ObjName}");
                ItemUI itemUI = Instantiate(obj).GetComponent<ItemUI>();
                itemUI.Item = item;
                item.ItemUI = itemUI;
            }
            item.knapType = knapType;
            item.gameObject.SetActive(false);
            switch (item.knapType)
            {
                case KnapType.Bag:
                    item.transform.SetParent(BagPoint);
                    return item.ItemUI.AddToBagUI(index);
                case KnapType.Equip:
                    item.transform.SetParent(HandPoint);
                    return item.ItemUI.AddToEquipUI(index);
                case KnapType.Action:
                    item.transform.SetParent(ActPoint);
                    return item.ItemUI.AddToActionUI(index);
                case KnapType.Trade:
                    item.transform.SetParent(TradePoint);
                    return item.ItemUI.AddToTradeUI(index);
                default:
                    return -1;
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
                    Target = null;
                    _cmd = new MoveCommand(this, new(proto.Point.X, proto.Point.Y, proto.Point.Z));
                    break;
                case CommandType.Attack:
                    if (GameManager.currWorld.npcDict.ContainsKey(proto.TargetSn))
                        Target = GameManager.currWorld.npcDict[proto.TargetSn].transform;
                    else
                        Target = GameManager.currWorld.roleDict[proto.TargetSn].obj.transform;
                    _cmd = new AttackCommand(this, Target);
                    break;
                case CommandType.Pickup:
                    if (!GameManager.currWorld.itemDict.ContainsKey(proto.TargetSn))
                        _cmd = new MoveCommand(this, new(proto.Point.X, proto.Point.Y, proto.Point.Z));
                    else
                    {
                        Target = GameManager.currWorld.itemDict[proto.TargetSn].transform;
                        _cmd = new PickupCommand(this, Target);
                    }
                    break;
                case CommandType.Teleport:
                    Target = GameManager.currWorld.itemDict[proto.TargetSn].transform;
                    _cmd = new TeleportCommand(this, Target);
                    break;
                case CommandType.Dialog:
                    Target = GameManager.currWorld.roleDict[proto.TargetSn].obj.transform;
                    _cmd = new ObserveCommand(this, Target);
                    break;
                case CommandType.Death:
                    string atkName;
                    if (GameManager.currWorld.roleDict.ContainsKey(proto.TargetSn))
                    {
                        var tmp = GameManager.currWorld.roleDict[proto.TargetSn].obj;
                        Target = tmp.transform;
                        atkName = tmp.GetNameBar();
                    }
                    else
                    {
                        var tmp = GameManager.currWorld.npcDict[proto.TargetSn];
                        Target = tmp.transform;
                        atkName = tmp.GetNameBar();
                    }
                    _cmd = new DeathCommand(this, Target, atkName);
                    break;
                default:
                    break;
            }
        }

        public void ParseStatus(Proto.SyncEntityStatus proto)
        {
            hp = proto.Hp;
            if (CompareTag("Enemy"))
                NameBar.HpBar.UpdateHp(hp, baseData.hp);
            if (GameManager.Instance.mainPlayer.Sn == Sn)
                UIManager.Instance.GetPanel<PropPanel>().UpdateHp(hp);
            if (TeamManager.Instance.teamDict.ContainsKey(Sn))
                TeamManager.Instance.teamDict[Sn].UpdateHp(hp);
        }

        public void ParsePlayerKnap(Proto.PlayerKnap proto)
        {
            gold = proto.Gold;
            UIManager.Instance.GetPanel<BagPanel>().UpdateGold(gold);
            foreach (Proto.ItemData data in proto.Items)
                ParseItemToKnap(data);
        }

        public void ParseItemToKnap(Proto.ItemData data)
        {
            switch (data.ItemType)
            {
                case Proto.ItemData.Types.ItemType.Potion:
                    string key1 = new((int)ItemType.Potion + "@" + data.Id);
                    ResourceManager.Instance.LoadAsync<GameObject>($"Item/Potion/{GameManager.Instance.dropPotionDict[key1][0]}", (obj) =>
                    {
                        Potion potion = Instantiate(obj).GetComponent<Potion>();
                        potion.itemType = ItemType.Potion;
                        potion.id = data.Id;
                        potion.Sn = data.Sn;
                        potion.ObjName = GameManager.Instance.dropPotionDict[key1][0];
                        potion.SetNameBar(GameManager.Instance.dropPotionDict[key1][1]);
                        potion.transform.position = transform.position;
                        AddItemToKnap((KnapType)data.KnapType, potion, data.Index);
                    });
                    break;
                case Proto.ItemData.Types.ItemType.Weapon:
                    string key2 = new((int)ItemType.Weapon + "@" + data.Id);
                    ResourceManager.Instance.LoadAsync<GameObject>($"Item/Weapon/{GameManager.Instance.dropWeaponDict[key2][0]}", (obj) =>
                    {
                        Weapon weapon = Instantiate(obj).GetComponent<Weapon>();
                        weapon.itemType = ItemType.Weapon;
                        weapon.id = data.Id;
                        weapon.Sn = data.Sn;
                        weapon.ObjName = GameManager.Instance.dropWeaponDict[key2][0];
                        weapon.SetNameBar(GameManager.Instance.dropWeaponDict[key2][1]);
                        weapon.transform.position = transform.position;
                        AddItemToKnap((KnapType)data.KnapType, weapon, data.Index);
                    });
                    break;
                default:
                    break;
            }
        }

        public void ParseTradeClose(Proto.TradeClose proto)
        {
            if (proto.Success)
            {
                var tradePanel = UIManager.Instance.GetPanel<TradePanel>();
                tradePanel.LocalRect.RemoveAllFromBag();
                tradePanel.RemoteRect.AddAllToBag();
                if (tradePanel.LocalRect.GoldField.text.Length > 0)
                    gold -= int.Parse(tradePanel.LocalRect.GoldField.text);
                if (tradePanel.RemoteRect.GoldField.text.Length > 0)
                    gold += int.Parse(tradePanel.RemoteRect.GoldField.text);
                UIManager.Instance.GetPanel<BagPanel>().UpdateGold(gold);
                Proto.UpdateKnapGold protoGold = new() { Gold = gold };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SUpdateKnapGold, protoGold);
            }
        }
    }
}
