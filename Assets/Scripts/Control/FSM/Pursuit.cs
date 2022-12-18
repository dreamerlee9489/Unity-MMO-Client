namespace Control.FSM
{
    public class Pursuit : FsmState
    {
        public Pursuit(FsmController owner, PlayerController target) : base(owner, target)
        {
            _type = FsmStateType.Pursuit;
        }

        public override void Enter()
        {
            _owner.Anim.SetBool(GameEntity.Attack, false);
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
