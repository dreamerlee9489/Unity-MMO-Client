namespace Control.FSM
{
    public class Patrol : State
    {
        private readonly int _index;

        public Patrol(FsmController owner, PlayerController target = null, int code = 0) : base(owner, target)
        {
            _type = StateType.Patrol;
            _index = code;
        }

        public override void Enter()
        {
            _owner.Anim.SetBool(GameEntity.attack, false);
            _owner.Agent.speed = _owner.walkSpeed;
            _owner.Agent.destination = _owner.patrolPath.Path[_index].position;
        }

        public override void Execute()
        {
        }

        public override void Exit()
        {
            _owner.currState = null;
        }
    }
}
