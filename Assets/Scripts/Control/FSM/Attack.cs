namespace Control.FSM
{
    public class Attack : State
    {
        public Attack(FsmController owner, PlayerController target = null) : base(owner, target)
        {
            _type = StateType.Attack;
        }

        public override void Enter()
        {
            _owner.agent.speed = _owner.walkSpeed;
            _owner.agent.destination = _target.transform.position;
            _owner.anim.SetBool(GameEntity.attack, true);
        }

        public override void Execute()
        {
            _owner.transform.LookAt(_target.transform);
        }

        public override void Exit()
        {
            _owner.currState = null;
            _owner.anim.SetBool(GameEntity.attack, false);
        }
    }
}
