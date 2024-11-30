using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Strength Upgrade", menuName = "Content/Upgrades/Strength Upgrade")]
    public class StrengthUpgrade : Upgrade<StrengthUpgrade.StrengthStage>
    {
        public override void Initialise()
        {

        }

        [System.Serializable]
        public class StrengthStage : BaseUpgradeStage
        {
            [SerializeField] int playerAnimalCarryingAmount;
            public float PlayerAnimalCarryingAmount => playerAnimalCarryingAmount;

            [SerializeField] int playerItemCarryingAmount;
            public float PlayerItemCarryingAmount => playerItemCarryingAmount;
        }
    }
}