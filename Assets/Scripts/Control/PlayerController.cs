using Control.CMD;
using Items;
using Manage;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

namespace Control
{
    public partial class PlayerController : GameEntity, IMover, IAttacker, IPicker, ITeleporter, IObserver, ILiver, IFighter
    {
        private RaycastHit _hit;
        private ICommand _curCmd;
        private readonly WaitForSeconds _sleep = new(0.2f);

        public int xp = 0, gold = 0;
        public static PlayerBaseData baseData;
        
        public Transform BagPoint { get; set; }
        public Transform HandPoint { get; set; }
        public Transform ActPoint { get; set; }
        public Transform TradePoint { get; set; }

        private void Start()
        {
            baseData ??= GameManager.Instance.playerBaseDatas[lv];
            MonoManager.Instance.StartCoroutine(CmdRunner());
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
            if (Sn != GameManager.Instance.mainPlayer.Sn || EventSystem.current.IsPointerOverGameObject())
                return;

            if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit) && hp != 0)
            {
                Proto.SyncPlayerCmd syncCmd = new();
                switch (_hit.collider.tag)
                {
                    case "Terrain":
                        syncCmd.Type = (int)CommandType.Move;
                        syncCmd.PlayerSn = Sn;
                        syncCmd.Point = new() { X = _hit.point.x, Y = _hit.point.y, Z = _hit.point.z };
                        ReqMoveTo(_hit.point);
                        break;
                    case "Enemy":
                        syncCmd.Type = (int)CommandType.Attack;
                        syncCmd.PlayerSn = Sn;
                        syncCmd.TargetSn = _hit.transform.GetComponent<BtController>().Sn;
                        ReqMoveTo(_hit.transform.position);
                        break;
                    case "Item":
                        syncCmd.Type = (int)CommandType.Pick;
                        syncCmd.PlayerSn = Sn;
                        syncCmd.TargetSn = _hit.transform.GetComponent<GameItem>().Sn;
                        syncCmd.Point = new() { X = _hit.transform.position.x, Y = _hit.transform.position.y, Z = _hit.transform.position.z };
                        ReqMoveTo(_hit.transform.position);
                        break;
                    case "Portal":
                        syncCmd.Type = (int)CommandType.Teleport;
                        syncCmd.PlayerSn = Sn;
                        syncCmd.TargetSn = _hit.transform.GetComponent<GameItem>().Sn;
                        ReqMoveTo(_hit.transform.position);
                        break;
                    case "Player":
                        syncCmd.Type = (int)CommandType.Observe;
                        syncCmd.PlayerSn = Sn;
                        syncCmd.TargetSn = _hit.transform.GetComponent<PlayerController>().Sn;
                        ReqMoveTo(_hit.transform.position);
                        break;
                    case "PvpTarget":
                        syncCmd.Type = (int)CommandType.Pvp;
                        syncCmd.PlayerSn = Sn;
                        syncCmd.TargetSn = _hit.transform.GetComponent<PlayerController>().Sn;
                        ReqMoveTo(_hit.transform.position);
                        break;
                }
                NetManager.Instance.SendPacket(Proto.MsgId.C2SSyncPlayerCmd, syncCmd);
            }

            if (Input.GetMouseButtonDown(1))
            {
                Proto.SyncPlayerCmd proto = new()
                { 
                    Type = 0,
                    PlayerSn = Sn,
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SSyncPlayerCmd, proto);
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

        private IEnumerator CmdRunner()
        {
            while (true) 
            {
                _curCmd?.Execute();
                yield return _sleep;
            }
        }

        public override void ReqMoveTo(Vector3 hitPoint, bool isRun = true)
        {
            if (Sn == GameManager.Instance.mainPlayer.Sn)
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
                    NetManager.Instance.SendPacket(Proto.MsgId.C2SPlayerMove, proto);
                }
            }
        }

        public void ResetCmd() => _curCmd?.Undo();          

        public void Move(Vector3 point)
        {
            Agent.isStopped = false;
            if (Vector3.Distance(transform.position, point) <= Agent.stoppingDistance)
                _curCmd = null;
        }

        public void UnMove()
        {
            _curCmd = null;
            Agent.isStopped = true;
        }

        public void Attack(BtController target)
        {
            Agent.isStopped = false;
            transform.LookAt(target.transform);
            if (Vector3.Distance(transform.position, target.transform.position) <= attackRadius)
                Anim.SetBool(attack, true);
            else
            {
                Anim.SetBool(attack, false);
                ReqMoveTo(target.transform.position);
            }
        }

        public void UnAttack()
        {
            _curCmd = null;
            Agent.isStopped = true;
            Anim.SetBool(attack, false);
        }

        public void Pick(GameItem item)
        {
            Agent.isStopped = false;
            Agent.destination = item.transform.position;
            if (Vector3.Distance(transform.position, item.transform.position) <= Agent.stoppingDistance)
            {
                _curCmd = null;
                Anim.SetTrigger(pickup);
            }
        }

        public void UnPick()
        {
            _curCmd = null;
            Agent.isStopped = true;
        }

        public void Teleport(GameItem portal)
        {
            Agent.isStopped = false;
            Agent.destination = portal.transform.position;
            if (Sn == GameManager.Instance.mainPlayer.Sn && Vector3.Distance(transform.position, portal.transform.position) <= Agent.stoppingDistance)
            {
                _curCmd = null;
                portal.GetComponent<Portal>().OpenDoor(this);
            }
        }

        public void UnTeleport()
        {
            _curCmd = null;
            Agent.isStopped = true;
        }

        public void Observe(PlayerController target)
        {
            Agent.isStopped = false;
            transform.LookAt(target.transform);
            if (Vector3.Distance(transform.position, target.transform.position) > Agent.stoppingDistance)
                ReqMoveTo(target.transform.position);
            else
            {
                _curCmd = null;
                Agent.isStopped = true;
                if (Sn == GameManager.Instance.mainPlayer.Sn)
                {
                    ObservePanel panel = UIManager.Instance.GetPanel<ObservePanel>();
                    panel.SetPlayer(target);
                    panel.Open();
                }
            }
        }

        public void UnObserve()
        {
            _curCmd = null;
            Agent.isStopped = true;
        }

        public void Pvp(PlayerController target)
        {
            Agent.isStopped = false;
            transform.LookAt(target.transform);
            if (Vector3.Distance(transform.position, target.transform.position) <= attackRadius)
                Anim.SetBool(attack, true);
            else
            {
                Anim.SetBool(attack, false);
                ReqMoveTo(target.transform.position);
            }
        }

        public void UnPvp()
        {
            _curCmd = null;
            Agent.isStopped = true;
            Anim.SetBool(attack, false);
        }

        public void Die(string atkName)
        {
            _curCmd = null;
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
            _curCmd = null;
            Agent.radius = 0.3f;
            Agent.isStopped = false;
            Agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            Anim.SetBool(death, false);
            GetComponent<CapsuleCollider>().enabled = true;
        }

        public void ParseCmd(Proto.SyncPlayerCmd proto)
        {
            CommandType newType = (CommandType)proto.Type;
            if (_curCmd != null && _curCmd.GetCmdType() != newType)
                _curCmd.Undo();
            switch (newType)
            {
                case CommandType.None:
                    _curCmd?.Undo();
                    break;
                case CommandType.Move:
                    _curCmd = new MoveCommand(this, new(proto.Point.X, proto.Point.Y, proto.Point.Z));
                    break;
                case CommandType.Attack:
                    _curCmd = new AttackCommand(this, GameManager.currWorld.npcDict[proto.TargetSn]);
                    break;
                case CommandType.Pick:
                    if (Sn != GameManager.Instance.mainPlayer.Sn)
                        _curCmd = new MoveCommand(this, new(proto.Point.X, proto.Point.Y, proto.Point.Z));
                    else
                        _curCmd = new PickCommand(this, GameManager.currWorld.itemDict[proto.TargetSn]);
                    break;
                case CommandType.Teleport:
                    _curCmd = new TeleportCommand(this, GameManager.currWorld.itemDict[proto.TargetSn]);
                    break;
                case CommandType.Observe:
                    _curCmd = new ObserveCommand(this, GameManager.currWorld.roleDict[proto.TargetSn].obj);
                    break;
                case CommandType.Pvp:
                    _curCmd = new PvpCommand(this, GameManager.currWorld.roleDict[proto.TargetSn].obj);
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
                    _curCmd = new DeathCommand(this, Target, atkName);
                    break;
                default:
                    break;
            }
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

        public void ParseProps(Proto.SyncPlayerProps proto)
        {
            hp = proto.Hp;
            if (TeamManager.Instance.teamDict.ContainsKey(Sn))
                TeamManager.Instance.teamDict[Sn].UpdateHp(hp);            
            if (GameManager.Instance.mainPlayer.Sn == Sn)
                UIManager.Instance.GetPanel<PropPanel>().UpdateHp(hp);
            else if (CompareTag("PvpTarget"))
                NameBar.HpBar.UpdateHp(hp, baseData.hp);
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
