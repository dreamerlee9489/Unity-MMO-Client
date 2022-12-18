using Manage;

namespace Control.FSM
{
    public class Idle : FsmState
    {
        public Idle(FsmController owner, PlayerController target = null) : base(owner, target)
        {
            _type = FsmStateType.Idle;
        }

        public override void Enter()
        {
            _owner.Agent.speed = _owner.WalkSpeed;
            _owner.Agent.isStopped = true;
            _target = GameManager.Instance.MainPlayer.Obj.GetComponent<PlayerController>();
        }

        public override void Execute()
        {
        }

        public override void Exit()
        {
            _owner.Agent.isStopped = false;
        }
    }
}
