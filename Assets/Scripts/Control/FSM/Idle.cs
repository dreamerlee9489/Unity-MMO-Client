namespace Control.FSM
{
    public class Idle : State
    {
        public Idle(FsmController owner, PlayerController target = null) : base(owner, target)
        {
            _type = StateType.Idle;
        }

        public override void Enter()
        {
            _owner.Agent.speed = _owner.WalkSpeed;
            _owner.Agent.isStopped = true;
        }

        public override void Execute()
        {
        }

        public override void Exit()
        {
            _owner.currState = null;
            _owner.Agent.isStopped = false;
        }
    }
}
