using Control.CMD;
using Items;
using Manage;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Control
{
    public class PlayerController : GameEntity, IMoveExecutor, IAttackExecutor, IPickupExecutor
    {
        private RaycastHit _hit;
        private ICommand _currCmd, _prevCmd;
        private CommandType _cmdType = CommandType.None;
        private Transform _knapsack, _handPos;
        private readonly WaitForSeconds _sleep = new(0.02f);

        public Transform Knapsack => _knapsack;

        public ulong sn = 0;
        public int xp = 0, gold = 0;

        protected override void Awake() => base.Awake();

        private void Start()
        {
            _agent.speed = RunSpeed * 1.5f;
            if (sn == GameManager.Instance.MainPlayer.Sn)
            {
                _knapsack = transform.Find("Knapsack");
                _handPos = transform.Find("HandPos");
                MonoManager.Instance.StartCoroutine(SyncStateCoroutine());
            }
        }

        protected override void Update()
        {
            base.Update();
            if (sn != GameManager.Instance.MainPlayer.Sn)
                return;

            if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit))
                {
                    switch (_hit.collider.tag)
                    {
                        case "Terrain":
                            _cmdType = CommandType.Move;
                            break;
                        case "Enemy":
                            _cmdType = CommandType.Attack;
                            break;
                        case "Item":
                            _cmdType = CommandType.Pickup;
                            break;
                    }
                }
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

        private void OnApplicationQuit()
        {
            MonoManager.Instance.StopAllCoroutines();
        }

        private IEnumerator SyncStateCoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            NetManager.Instance.SendPacket(Proto.MsgId.C2SGetPlayerKnap, null);
            while (true)
            {
                yield return _sleep;
                Proto.PlayerPushPos syncPos = new()
                {
                    Pos = new()
                    {
                        X = transform.position.x,
                        Y = transform.position.y,
                        Z = transform.position.z
                    }
                };

                Proto.PlayerSyncCmd syncCmd = new();
                switch (_cmdType)
                {
                    case CommandType.None:
                        syncCmd.Type = 0;
                        syncCmd.PlayerSn = sn;
                        break;
                    case CommandType.Move:
                        syncCmd.Type = 1;
                        syncCmd.PlayerSn = sn;
                        syncCmd.TargetId = -1;
                        syncCmd.Point = new()
                        {
                            X = _hit.point.x,
                            Y = _hit.point.y,
                            Z = _hit.point.z
                        };
                        break;
                    case CommandType.Attack:
                        syncCmd.Type = 2;
                        syncCmd.PlayerSn = sn;
                        syncCmd.TargetId = _hit.transform.GetComponent<FsmController>().id;
                        syncCmd.Point = new()
                        {
                            X = _hit.transform.position.x,
                            Y = _hit.transform.position.y,
                            Z = _hit.transform.position.z
                        };
                        break;
                    case CommandType.Pickup:
                        syncCmd.Type = 3;
                        syncCmd.PlayerSn = sn;
                        syncCmd.TargetId = -1;
                        syncCmd.Point = new()
                        {
                            X = _hit.point.x,
                            Y = _hit.point.y,
                            Z = _hit.point.z
                        };
                        break;
                    default:
                        break;
                }

                NetManager.Instance.SendPacket(Proto.MsgId.C2SPlayerPushPos, syncPos);
                NetManager.Instance.SendPacket(Proto.MsgId.C2SPlayerSyncCmd, syncCmd);
            }
        }

        public void ParseSyncCmd(Proto.PlayerSyncCmd proto)
        {
            Vector3 point;
            _prevCmd = _currCmd;
            switch (proto.Type)
            {
                case 0:
                    _currCmd?.Undo();
                    break;
                case 1:
                    point = new Vector3(proto.Point.X, proto.Point.Y, proto.Point.Z);
                    _currCmd = new MoveCommand(this, point);
                    break;
                case 2:
                    target = GameManager.Instance.ActiveWorld.Enemies[proto.TargetId];
                    _currCmd = new AttackCommand(this, target);
                    break;
                case 3:
                    point = new Vector3(proto.Point.X, proto.Point.Y, proto.Point.Z);
                    _currCmd = new PickupCommand(this, point);
                    break;
                default:
                    break;
            }
            if (_prevCmd != null && _currCmd != null && _prevCmd.GetCommandType() != _currCmd.GetCommandType())
                _prevCmd.Undo();
            _currCmd?.Execute();
        }

        public void ResetCmd() => _cmdType = CommandType.None;

        public void Move(Vector3 point)
        {
            _agent.isStopped = false;
            _agent.destination = point;
        }

        public void Attack(GameEntity target)
        {
            _agent.isStopped = false;
            _agent.destination = target.transform.position;
            transform.LookAt(target.transform);
            if (Vector3.Distance(transform.position, target.transform.position) <= AttackRadius)
                _anim.SetBool(attack, true);
            else
                _anim.SetBool(attack, false);
        }

        public void Pickup(Vector3 point)
        {
            _agent.isStopped = false;
            _agent.destination = point;
            if (Vector3.Distance(transform.position, point) < _agent.stoppingDistance)
                _cmdType = CommandType.None;
        }

        public void UnMove()
        {
            _agent.isStopped = true;
            _currCmd = null;
        }

        public void UnAttack()
        {
            _agent.isStopped = true;
            _anim.SetBool(attack, false);
            _currCmd = null;
        }

        public void UnPickup()
        {
            _agent.isStopped = true;
            _currCmd = null;
        }

        public int AddItemToKnap(GameItem item, int index = 0)
        {
            item.gameObject.SetActive(false);
            item.transform.SetParent(_knapsack);
            GameObject obj = ResourceManager.Instance.Load<GameObject>($"UI/ItemUI/{item.itemType}/{item.objName}");
            ItemUI itemUI = Instantiate(obj).GetComponent<ItemUI>();
            itemUI.Item = item;
            item.ItemUI = itemUI;
            PoolManager.Instance.Push(item.objName, itemUI.gameObject);
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
                                potion.itemId = data.Id;
                                potion.itemType = ItemType.Potion;
                                potion.hashCode = data.Hash;
                                potion.objName = potionDict[key][0];
                                potion.SetNameBar(potionDict[key][1]);
                                potion.transform.position = transform.position;
                                AddItemToKnap(potion, data.Index);
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
                                weapon.hashCode = data.Hash;
                                weapon.objName = weaponDict[key][0];
                                weapon.SetNameBar(weaponDict[key][1]);
                                weapon.transform.position = transform.position;
                                AddItemToKnap(weapon, data.Index);
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
