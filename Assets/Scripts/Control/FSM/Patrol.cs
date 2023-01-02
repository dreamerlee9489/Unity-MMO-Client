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
            _owner.Anim.SetBool(GameEntity.attack, false);
            _owner.Agent.speed = _owner.WalkSpeed;
            _owner.Agent.destination = _patrolPath.Path[_index].position;
        }

        public override void Execute()
        {
        }

        public override void Exit()
        {
        }
    }
}
