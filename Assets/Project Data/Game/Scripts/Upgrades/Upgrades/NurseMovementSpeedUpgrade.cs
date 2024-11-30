using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Nurse Movement Speed Upgrade", menuName = "Content/Upgrades/Nurse Movement Speed Upgrade")]
    public class NurseMovementSpeedUpgrade : Upgrade<NurseMovementSpeedUpgrade.NurseMovementSpeedStage>
    {
        public override void Initialise()
        {

        }

        [System.Serializable]
        public class NurseMovementSpeedStage : BaseUpgradeStage
        {
            [SerializeField] float nurseMovementSpeed;
            public float NurseMovementSpeed => nurseMovementSpeed;

            [SerializeField] float nurseAngularSpeed;
            public float NurseAngularSpeed => nurseAngularSpeed;

            [SerializeField] float nurseAcceleration;
            public float NurseAcceleration => nurseAcceleration;

            [SerializeField] float nurseBlendTreeMultiplier;
            public float NurseBlendTreeMultiplier => nurseBlendTreeMultiplier;
        }
    }
}