using App;

namespace Control.FSM
{
    public enum AIStateType { Idle, Patrol, Pursuit, Attack }

    public abstract class AIState
    {
        protected EnemyController _owner;
        protected PlayerController _target;

        public AIStateType type;
        public PlayerController Target => _target;

        protected AIState(EnemyController owner, PlayerController target = null)
        {
            _owner = owner;
            _target = target;
        }

        public abstract void Enter();
        public abstract void Execute();
        public abstract void Exit();
        public abstract void UpdateState(int code);

        public static AIState GenState(AIStateType type, EnemyController owner, PlayerController target)
        {
            switch (type)
            {
                case AIStateType.Idle:
                    return new Idle(owner, null);
                case AIStateType.Patrol:
                    return new Patrol(owner, null);
                case AIStateType.Pursuit:
                    return new Pursuit(owner, target);
                case AIStateType.Attack:
                    return new Attack(owner, target);
                default:
                    return null;
            }
        }
    }
}
