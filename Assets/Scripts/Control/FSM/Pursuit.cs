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
            _owner.Anim.SetBool(GameEntity.attack, false);
            _owner.Agent.speed = _owner.RunSpeed;
        }

        public override void Execute()
        {
            _owner.Agent.destination = _target.transform.position;
        }

        public override void Exit()
        {
        }
    }
}
