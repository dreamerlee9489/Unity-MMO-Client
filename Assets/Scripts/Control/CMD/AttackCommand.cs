using UnityEngine;

namespace Control.CMD
{
    public class AttackCommand : ICommand
    {
        private Transform _target;

        public AttackCommand(IAttackExecutor executor, Transform target) : base(executor)
        {
            _target = target;
        }

        public override CommandType GetCommandType() => CommandType.Attack;

        public override void Execute()
        {
            (_executor as IAttackExecutor).Attack(_target);
        }

        public override void Undo()
        {
            (_executor as IAttackExecutor).UnAttack();
        }
    }
}
