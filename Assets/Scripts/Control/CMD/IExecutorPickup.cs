using UnityEngine;

namespace Control.CMD
{
    public interface IPickupExecutor : IExecutor
    {
        void Pickup(Vector3 point);

        void UnPickup();
    }
}
