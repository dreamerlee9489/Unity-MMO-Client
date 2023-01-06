using Control.CMD;
using Items;
using Manage;
using System.Threading;
using System.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Control
{
    public partial class PlayerController : GameEntity
    {
        private readonly Proto.Vector3D _curPos = new();
        private readonly Proto.Vector3D _hitPos = new();
        private readonly CancellationTokenSource _tokenSource = new();

        private RaycastHit _hit;
        private ICommand _cmd;
        private Transform _knapsack, _handPos;

        public Transform Knapsack => _knapsack;

        public ulong sn = 0;
        public int xp = 0, gold = 0;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            _agent.speed = RunSpeed * 1.5f;
            if (sn == GameManager.Instance.MainPlayer.Sn)
            {
                _knapsack = transform.Find("Knapsack");
                _handPos = transform.Find("HandPos");
                Task.Run(SyncStateTask);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (sn != GameManager.Instance.MainPlayer.Sn || EventSystem.current.IsPointerOverGameObject())
                return;

            _cmd?.Execute();
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
                        syncCmd.PlayerSn = sn;
                        syncCmd.TargetId = "";
                        syncCmd.Point = _hitPos;
                        break;
                    case "Enemy":
                        syncCmd.Type = 2;
                        syncCmd.PlayerSn = sn;
                        syncCmd.TargetId = _hit.transform.GetComponent<GuidObject>().GetGuid();
                        syncCmd.Point = null;
                        break;
                    case "Item":
                        syncCmd.Type = 3;
                        syncCmd.PlayerSn = sn;
                        syncCmd.TargetId = _hit.transform.GetComponent<GuidObject>().GetGuid();
                        syncCmd.Point = _hitPos;
                        break;
                    case "Portal":
                        syncCmd.Type = 4;
                        syncCmd.PlayerSn = sn;
                        syncCmd.TargetId = _hit.transform.GetComponent<GuidObject>().GetGuid();
                        syncCmd.Point = null;
                        break;
                }
                NetManager.Instance.SendPacket(Proto.MsgId.C2SSyncPlayerCmd, syncCmd);
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Proto.AtkAnimEvent proto = new()
                {
                    PlayerSn = GameManager.Instance.MainPlayer.Sn,
                    EnemyId = -1,
                    CurrHp = hp,
                    AtkEnemy = true
                };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SAtkAnimEvent, proto);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _tokenSource.Cancel();
        }

        private void OnApplicationQuit()
        {
            _tokenSource.Cancel();
        }

        private void SyncStateTask()
        {
            Thread.Sleep(500);
            NetManager.Instance.SendPacket(Proto.MsgId.C2SGetPlayerKnap, null);
            while (!_tokenSource.IsCancellationRequested)
            {
                Proto.PushPlayerPos syncPos = new() { Pos = _curPos };
                NetManager.Instance.SendPacket(Proto.MsgId.C2SPushPlayerPos, syncPos);
                Thread.Sleep(100);
            }
        }

        public void ParseSyncCmd(Proto.SyncPlayerCmd proto)
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
                    target = _currWorld.inWorldObjDict[proto.TargetId];
                    _cmd = new AttackCommand(this, target);
                    break;
                case CommandType.Pickup:
                    target = _currWorld.inWorldObjDict[proto.TargetId];
                    _cmd = new PickupCommand(this, target);
                    break;
                case CommandType.Teleport:
                    target = _currWorld.inWorldObjDict[proto.TargetId];
                    _cmd = new TeleportCommand(this, target);
                    break;
                default:
                    break;
            }
        }

        public void ResetCmd()
        {
            Proto.SyncPlayerCmd proto = new() { Type = 0, PlayerSn = sn, TargetId = "", Point = null };
            NetManager.Instance.SendPacket(Proto.MsgId.C2SSyncPlayerCmd, proto);
        }

        public int UpdateKnapItem(GameItem item, int index = 0)
        {
            item.gameObject.SetActive(false);
            item.transform.SetParent(_knapsack);
            GameObject obj = ResourceManager.Instance.Load<GameObject>($"UI/ItemUI/{item.itemType}/{item.ObjName}");
            ItemUI itemUI = Instantiate(obj).GetComponent<ItemUI>();
            itemUI.Item = item;
            item.ItemUI = itemUI;
            PoolManager.Instance.Push(item.ObjName, itemUI.gameObject);
            return itemUI.AddToKnap(index);
        }

        public void ParsePlayerKnap(Proto.PlayerKnap playerKnap)
        {
            gold = playerKnap.Gold;
            UIManager.Instance.FindPanel<KnapPanel>().UpdateGold(gold);
            var potionDict = GameManager.Instance.DropPotionDict;
            var weaponDict = GameManager.Instance.DropWeaponDict;
            foreach (Proto.ItemData data in playerKnap.ItemsInBag)
            {
                switch (data.Type)
                {
                    case Proto.ItemData.Types.ItemType.None:
                        break;
                    case Proto.ItemData.Types.ItemType.Potion:
                        for (int i = 0; i < data.Num; i++)
                        {
                            string key = (int)ItemType.Potion + "@" + data.Id;
                            ResourceManager.Instance.LoadAsync<GameObject>($"Item/Potion/{potionDict[key][0]}", (obj) =>
                            {
                                Potion potion = Instantiate(obj).GetComponent<Potion>();
                                potion.itemType = ItemType.Potion;
                                potion.ItemId = data.Id;
                                potion.ObjName = potionDict[key][0];
                                potion.SetNameBar(potionDict[key][1]);
                                potion.SetKnapId(data.Key);
                                potion.transform.position = transform.position;
                                UpdateKnapItem(potion, data.Index);
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
                                weapon.SetKnapId(data.Key);
                                weapon.transform.position = transform.position;
                                UpdateKnapItem(weapon, data.Index);
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
