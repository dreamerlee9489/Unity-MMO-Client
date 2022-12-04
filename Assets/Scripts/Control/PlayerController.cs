using Frame;
using Net;
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
        private WaitForSeconds _sleep = new(0.02f);

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

            switch (_hitTarget?.tag)
            {
                case "Terrain":
                    if (Vector3.Distance(transform.position, _hitPoint) <= _agent.stoppingDistance)
                        _hitTarget = null;
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

        private IEnumerator SyncStateCoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            while (true)
            {
                yield return _sleep;
                if (_state != PlayerStateType.Idle)
                {
                    Proto.PlayerSyncState proto = new()
                    {
                        PlayerSn = Sn,
                        EnemyId = (!_hitTarget || _hitTarget.CompareTag("Terrain")) ? -1 : _hitTarget.GetComponent<EnemyController>().Id,
                        State = (int)_state,
                        Code = _stateCode,
                        CurPos = new Proto.Vector3() { X = transform.position.x, Y = transform.position.y, Z = transform.position.z },
                        HitPos = new Proto.Vector3() { X = _hitPoint.x, Y = _hitPoint.y, Z = _hitPoint.z }
                    };
                    NetManager.Instance.SendPacket(Proto.MsgId.C2SPlayerSyncState, proto);
                }
            }
        }

        public void ParseSyncState(Proto.PlayerSyncState proto)
        {
            int id = proto.EnemyId;
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
                    bool code = proto.Code == 0 ? false : true;
                    _anim.SetBool(Attack, code);
                    Transform target = GameManager.Instance.ActiveWorld.Enemies[id].transform;
                    transform.LookAt(target);
                    if (!code)
                        _agent.destination = target.position;
                    break;
                default:
                    break;
            }
        }
    }
}
