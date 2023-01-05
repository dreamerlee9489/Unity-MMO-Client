using UnityEngine;

namespace Control.CMD
{
    public interface IAttackExecutor : IExecutor
    {
        void Attack(Transform target);
        void UnAttack();
    }
}