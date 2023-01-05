using UnityEngine;

namespace Control.CMD
{
    public interface IPickupExecutor : IExecutor
    {
        void Pickup(Transform item);
        void UnPickup();
    }
}
