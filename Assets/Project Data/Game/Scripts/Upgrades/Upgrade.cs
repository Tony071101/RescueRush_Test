using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon
{
    public abstract class Upgrade<T> : BaseUpgrade where T : BaseUpgradeStage
    {
        [SerializeField]
        protected T[] upgrades;
        public override BaseUpgradeStage[] Upgrades => upgrades;

        public T GetCurrentStage()
        {
            if (upgrades.IsInRange(UpgradeLevel))
                return upgrades[UpgradeLevel];

            Debug.LogError("[Perks]: Perk level is out of range!");

            return null;
        }

        public override void UpgradeStage()
        {
            if (upgrades.IsInRange(UpgradeLevel + 1))
            {
                UpgradeLevel += 1;

                InvokeOnUpgraded();
            }
        }
    }
}