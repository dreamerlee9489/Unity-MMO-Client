using UnityEngine;

namespace Control.CMD
{
	public interface ILiver : IExecutor
	{
		void Die(string atkName);
		void Rebirth();
	}

	public class DeathCommand : ICommand
	{
        private string _atkName;
		private Transform _target;

        public DeathCommand(IExecutor executor, Transform target, string atkName) : base(executor)
        {
            _atkName = atkName;
            _target = target;
        }

        public override CommandType GetCommandType() => CommandType.Death;

        public override void Execute()
        {
            (_executor as ILiver).Die(_atkName);
        }

        public override void Undo()
        {
            (_executor as ILiver).Rebirth();
        }
    }
}