using UnityEngine;

namespace Control.CMD
{
    public interface IAttacker : IExecutor
    {
        void Attack(Transform target);
        void UnAttack();
    }

    public class AttackCommand : ICommand
    {
        private Transform _target;

        public AttackCommand(IAttacker executor, Transform target) : base(executor)
        {
            _target = target;
        }

        public override CommandType GetCommandType() => CommandType.Attack;

        public override void Execute()
        {
            (_executor as IAttacker).Attack(_target);
        }

        public override void Undo()
        {
            (_executor as IAttacker).UnAttack();
        }
    }
}
