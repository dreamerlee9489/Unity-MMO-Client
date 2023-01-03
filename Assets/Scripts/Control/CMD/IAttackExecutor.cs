namespace Control.CMD
{
    public interface IAttackExecutor : IExecutor
    {
        void Attack(GameEntity target);

        void UnAttack();
    }
}