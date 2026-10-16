using UnityEngine;

namespace Event
{
    public struct StopLookingAtEvent
    {
        public GameObject Target;

        public StopLookingAtEvent(GameObject target)
        {
            this.Target = target;
        }
    }
}