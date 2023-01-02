using Control.CMD;
using Manage;
using System.Collections;
using UnityEngine;

namespace Control
{
    public class PlayerController : GameEntity, IMoveExecutor, IAttackExecutor, IPickupExecutor
    {
        private RaycastHit _hit;
        private ICommand _currCmd;
        private CommandType _cmdType = CommandType.None;
        private readonly WaitForSeconds _sleep = new(0.02f);

        public ICommand CurrCmd => _currCmd;

        public ulong sn = 0;
        public int xp = 0, gold = 0;

        protected override void Awake() => base.Awake();

        private void Start()
        {
            _agent.speed = RunSpeed * 1.5f;
            if (sn == GameManager.Instance.MainPlayer.Sn)
                MonoManager.Instance.StartCoroutine(SyncStateCoroutine());
        }

        protected override void Update()
        {
            base.Update();
            _currCmd?.Execute();
            if (Input.GetMouseButtonDown(0) && sn == GameManager.Instance.MainPlayer.Sn)
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

            if (Input.GetKeyDown(KeyCode.UpArrow) && sn == GameManager.Instance.MainPlayer.Sn)
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

        private void OnApplicationQuit() => MonoManager.Instance.StopAllCoroutines();

        private IEnumerator SyncStateCoroutine()
        {
            yield return new WaitForSeconds(1f);
            while (true)
            {
                yield return _sleep;
                Proto.PlayerSyncPos syncPos = new() 
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

                NetManager.Instance.SendPacket(Proto.MsgId.C2SPlayerSyncPos, syncPos);
                NetManager.Instance.SendPacket(Proto.MsgId.C2SPlayerSyncCmd, syncCmd);
            }
        }

        public void ParseSyncCmd(Proto.PlayerSyncCmd proto)
        {
            Vector3 point;
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
            this.target = target;
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
    }
}
