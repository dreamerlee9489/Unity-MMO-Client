﻿using Manage;
using UnityEngine;

namespace Control.FSM
{
    public class Patrol : FsmState
    {
        private readonly int _index;
        private PatrolPath _patrolPath;

        public Patrol(FsmController owner, PlayerController target = null, int code = 0) : base(owner, target)
        {
            _type = FsmStateType.Patrol;
            _index = code;
        }

        public override void Enter()
        {
            _patrolPath = _owner.patrolPath;
            _owner.Anim.SetBool(GameEntity.Attack, false);
            _owner.Agent.speed = _owner.WalkSpeed;
            _owner.Agent.destination = _patrolPath.Path[_index].position;
            _target = GameManager.Instance.MainPlayer.Obj.GetComponent<PlayerController>();
        }

        public override void Execute()
        {
            if (_owner.CanSee(_target))
            {
                Net.FsmSyncState proto = new()
                {
                    EnemyId = _owner.id,
                    PlayerSn = _target.Sn,
                    State = (int)FsmStateType.Pursuit,
                    Code = -1,
                };
                NetManager.Instance.SendPacket(Net.MsgId.C2SFsmSyncState, proto);
            }
        }

        public override void Exit()
        {
        }
    }
}
