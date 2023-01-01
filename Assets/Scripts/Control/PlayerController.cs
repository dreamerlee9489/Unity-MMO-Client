using Manage;
using System.Collections;
using UnityEngine;

namespace Control
{
    public enum PlayerStateType { Idle, Move, Attack }

    public class PlayerController : GameEntity
    {
        private PlayerStateType _state = PlayerStateType.Idle;
        private int _stateCode = 0;
        private RaycastHit _hit;
        private Vector3 _hitPoint = Vector3.zero;
        private readonly WaitForSeconds _sleep = new(0.02f);
        private readonly Proto.Vector3D _curPos = new();
        private readonly Proto.Vector3D _hitPos = new();

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
                MonoManager.Instance.StartCoroutine(SyncStateCoroutine());
        }

        protected override void Update()
        {
            base.Update();
            if (Input.GetMouseButtonDown(0) && sn == GameManager.Instance.MainPlayer.Sn)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit))
                {
                    switch (_hit.collider.tag)
                    {
                        case "Terrain":
                            target = _hit.transform;
                            _hitPoint = _hit.point;
                            _state = PlayerStateType.Move;
                            break;
                        case "Enemy":
                            target = _hit.transform;
                            _hitPoint = _hit.transform.position;
                            _state = PlayerStateType.Attack;
                            break;
                        case "Item":
                            target = _hit.transform;
                            _hitPoint = _hit.point;
                            _state = PlayerStateType.Move;
                            break;
                    }
                }
            }

            if(Input.GetKeyDown(KeyCode.UpArrow) && sn == GameManager.Instance.MainPlayer.Sn)
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

            switch (target != null ? target.tag : null)
            {
                case "Terrain":
                    if (Vector3.Distance(transform.position, _hitPoint) <= _agent.stoppingDistance)
                    {
                        target = null;
                        Invoke(nameof(ResetState), 3);
                    }
                    break;
                case "Enemy":
                    if (Vector3.Distance(transform.position, target.transform.position) <= AttackRadius)
                        _stateCode = 1;
                    else
                        _stateCode = 0;
                    break;
            }
        }

        private void OnApplicationQuit()
        {
            _state = PlayerStateType.Idle;
            MonoManager.Instance.StopAllCoroutines();
        }

        private void ResetState()
        {
            if (target == null)
                _state = PlayerStateType.Idle;
        }

        private IEnumerator SyncStateCoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            while (true)
            {
                yield return _sleep;
                if (_state != PlayerStateType.Idle)
                {
                    _curPos.X = transform.position.x;
                    _curPos.Y = transform.position.y;
                    _curPos.Z = transform.position.z;
                    _hitPos.X = _hitPoint.x;
                    _hitPos.Y = _hitPoint.y;
                    _hitPos.Z = _hitPoint.z;

                    Proto.PlayerSyncState proto = new()
                    {
                        PlayerSn = sn,
                        EnemyId = (!target || !target.CompareTag("Enemy")) ? -1 : target.GetComponent<FsmController>().id,
                        State = target ? (int)_state : 1,
                        Code = _stateCode,
                        CurPos = _curPos,
                        HitPos = _hitPos
                    };
                    NetManager.Instance.SendPacket(Proto.MsgId.C2SPlayerSyncState, proto);
                }
            }
        }

        public void ParseSyncState(Proto.PlayerSyncState proto)
        {
            PlayerStateType state = (PlayerStateType)proto.State;
            Vector3 pos = new()
            {
                x = proto.HitPos.X,
                y = proto.HitPos.Y,
                z = proto.HitPos.Z
            };
            switch (state)
            {
                case PlayerStateType.Move:
                    _anim.SetBool(Attack, false);
                    _agent.destination = pos;
                    break;
                case PlayerStateType.Attack:
                    bool atk = proto.Code != 0;
                    int id = proto.EnemyId;
                    _anim.SetBool(Attack, atk);
                    Transform target = GameManager.Instance.ActiveWorld.Enemies[id].transform;
                    if (!atk)
                        _agent.destination = target.position;
                    transform.LookAt(target);
                    break;
                default:
                    break;
            }
        }
    }
}
