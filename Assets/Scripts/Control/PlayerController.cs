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
        private Transform _hitTarget = null;
        private RaycastHit _hit;
        private Vector3 _hitPoint = Vector3.zero;
        private readonly WaitForSeconds _sleep = new(0.02f);
        private readonly Net.Vector3D _curPos = new();
        private readonly Net.Vector3D _hitPos = new();

        public ulong Sn { get; set; }

        protected override void Awake()
        {
            base.Awake();
            _hitTarget = null;
        }

        private void Start()
        {
            _agent.speed = RunSpeed * 1.5f;
            MonoManager.Instance.StartCoroutine(SyncStateCoroutine());
        }

        protected override void Update()
        {
            base.Update();
            if (Input.GetMouseButtonDown(0) && Sn == GameManager.Instance.MainPlayer.Sn)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit))
                {
                    switch (_hit.collider.tag)
                    {
                        case "Terrain":
                            _hitTarget = _hit.transform;
                            _hitPoint = _hit.point;
                            _state = PlayerStateType.Move;
                            break;
                        case "Enemy":
                            _hitTarget = _hit.transform;
                            _hitPoint = _hit.transform.position;
                            _state = PlayerStateType.Attack;
                            break;
                    }
                }
            }

            switch (_hitTarget != null ? _hitTarget.tag : null)
            {
                case "Terrain":
                    if (Vector3.Distance(transform.position, _hitPoint) <= _agent.stoppingDistance)
                    {
                        _hitTarget = null;
                        Invoke(nameof(ResetState), 3);
                    }
                    break;
                case "Enemy":
                    if (Vector3.Distance(transform.position, _hitTarget.transform.position) <= AttackRadius)
                        _stateCode = 1;
                    else
                        _stateCode = 0;
                    break;
            }
        }

        private void OnApplicationQuit()
        {
            MonoManager.Instance.StopCoroutine(SyncStateCoroutine());
        }

        private void ResetState()
        {
            if (_hitTarget == null)
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

                    Net.PlayerSyncState proto = new()
                    {
                        PlayerSn = Sn,
                        EnemyId = (!_hitTarget || _hitTarget.CompareTag("Terrain")) ? -1 : _hitTarget.GetComponent<FsmController>().Id,
                        State = (int)_state,
                        Code = _stateCode,
                        CurPos = _curPos,
                        HitPos = _hitPos
                    };
                    NetManager.Instance.SendPacket(Net.MsgId.C2SPlayerSyncState, proto);
                }
            }
        }

        public void ParseSyncState(Net.PlayerSyncState proto)
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
