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
            _owner.agent.speed = _owner.walkSpeed;
            _owner.agent.isStopped = true;
        }

        public override void Execute()
        {
        }

        public override void Exit()
        {
            _owner.currState = null;
            _owner.agent.isStopped = false;
        }
    }
}
