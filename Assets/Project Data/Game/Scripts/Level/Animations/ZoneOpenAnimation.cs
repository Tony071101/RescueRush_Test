using UnityEngine;

namespace Watermelon
{
    [RequireComponent(typeof(Zone))]
    public abstract class ZoneOpenAnimation : MonoBehaviour
    {
        protected Zone zone;

        public abstract void OnZoneInitialised(Zone zone);
        public abstract void OnZoneOpened();
    }
}