using UnityEngine;

namespace Event
{
    public struct StartLookingAtEvent
    {
        public GameObject Target;

        public StartLookingAtEvent(GameObject target)
        {
            this.Target = target;
        }
    }
}