using Control.CMD;
using Frame;
using Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Control
{
    public enum PlayerStateType
    {
        Idle, Move, Attack
    }

    [RequireComponent(typeof(Entity))]
    public class PlayerController : MonoBehaviour, ICmdReceiver
    {
        private RaycastHit _hit;
        private PlayerStateType _state = PlayerStateType.Idle;
        private int _code = 0;
        private Transform _target = null;
        private Vector3 _hitPoint = Vector3.zero;

        private Entity _entity;
        private WaitForSeconds _waitForSeconds = new WaitForSeconds(0.02f);
        private readonly List<Command> _commands = new List<Command>();

        public ulong Sn { get; set; }


        private void Awake()
        {
            _entity = GetComponent<Entity>();
            _commands.Add(new CombatCommand(this));
            _target = null;
        }

        private void Start()
        {
            MonoManager.Instance.StartCoroutine(SyncStateCoroutine());
        }

        private void Update()
        {
            switch (_target?.tag)
            {
                case "Terrain":
                    if (Vector3.Distance(_hitPoint, transform.position) < _entity.Agent.stoppingDistance)
                        _target = null;
                    break;
                case "Enemy":
                    if (Vector3.Distance(_target.position, transform.position) <= 1.5f)
                        _code = 1;
                    else
                        _code = 0;
                    break;
            }

            if (Sn == GameManager.Instance.MainPlayer.Sn)
            {
                if (Input.GetMouseButtonDown(1))
                    UndoAll();
                else if (Input.GetMouseButtonDown(0))
                {
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit))
                    {
                        Undo();
                        switch (_hit.collider.tag)
                        {
                            case "Terrain":
                                _target = _hit.transform;
                                _hitPoint = _hit.point;
                                _state = PlayerStateType.Move;
                                break;
                            case "Enemy":
                                _target = _hit.transform;
                                _hitPoint = _hit.transform.position;
                                _state = PlayerStateType.Attack;
                                break;
                        }
                    }
                }
            }
        }

        private void OnApplicationQuit()
        {
            MonoManager.Instance.StopCoroutine(SyncStateCoroutine());
        }

        public void UndoAll()
        {
            foreach (var command in _commands)
                command.Undo();
        }

        public void Execute(Vector3 point)
        {
            Vector3 destPoint = point;
            if (NavMesh.SamplePosition(point, out NavMeshHit meshHit, 10, 1))
                destPoint = meshHit.position;
            NavMeshPath path = new();
            _entity.Agent.CalculatePath(destPoint, path);
            if (path.status != NavMeshPathStatus.PathPartial)
            {
                Proto.Move proto = new();
                foreach (Vector3 pos in path.corners)
                    proto.Position.Add(new Proto.Vector3() { X = pos.x, Y = pos.y, Z = pos.z });
                NetManager.Instance.SendPacket(Proto.MsgId.C2SMove, proto);
            }
        }

        public void Execute(Transform target)
        {
        }

        public void Undo()
        {
            _entity.Agent.destination = transform.position + transform.forward;
        }

        private IEnumerator SyncStateCoroutine()
        {
            while (true)
            {
                yield return _waitForSeconds;
                if (_state != PlayerStateType.Idle)
                {
                    Proto.PlayerSyncState proto = new();
                    proto.PlayerSn = Sn;
                    proto.EnemyId = (!_target || _target.CompareTag("Terrain")) ? -1 : _target.GetComponent<EnemyController>().Id;
                    proto.State = (int)_state;
                    proto.Code = _code;
                    proto.CurrPos = new Proto.Vector3() { X = transform.position.x, Y = transform.position.y, Z = transform.position.z };
                    proto.Target = new Proto.Vector3() { X = _hitPoint.x, Y = _hitPoint.y, Z = _hitPoint.z };
                    NetManager.Instance.SendPacket(Proto.MsgId.C2SPlayerSyncState, proto);
                }
            }
        }

        public void ParseSyncState(Proto.PlayerSyncState proto)
        {
            int id = proto.EnemyId;
            PlayerStateType state = (PlayerStateType)proto.State;
            Vector3 pos = new Vector3()
            {
                x = proto.Target.X,
                y = proto.Target.Y,
                z = proto.Target.Z
            };
            _entity.Agent.destination = pos;
            switch (state)
            {
                case PlayerStateType.Idle:
                    break;
                case PlayerStateType.Move:
                    _entity.Anim.SetBool(Entity.Attack, false);
                    break;
                case PlayerStateType.Attack:
                    bool code = proto.Code == 0 ? false : true;
                    _entity.Anim.SetBool(Entity.Attack, code);
                    transform.LookAt(GameManager.Instance.ActiveWorld.Enemies[id].transform);
                    break;
                default:
                    break;
            }
        }
    }
}
