namespace Control.FSM
{
    public class Pursuit : State
    {
        public Pursuit(FsmController owner, PlayerController target) : base(owner, target)
        {
            _type = StateType.Pursuit;
        }

        public override void Enter()
        {
            _owner.anim.SetBool(GameEntity.attack, false);
            _owner.agent.speed = _owner.runSpeed;
        }

        public override void Execute()
        {
            _owner.agent.destination = _target.transform.position;
        }

        public override void Exit()
        {
            _owner.currState = null;
        }
    }
}
