using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class SpawnType
    {
        [SerializeField] Animal.Type animalType;
        public Animal.Type AnimalType => animalType;

        [SerializeField] SicknessType sicknessType;
        public SicknessType SicknessType => sicknessType;
    }
}