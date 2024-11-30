using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class Animal
    {
        [SerializeField] Type animalType;
        public Type AnimalType => animalType;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [SerializeField] float carryingHeight;
        public float CarryingHeight => carryingHeight;

        [Space]
        [SerializeField] CurrencyType rewardType;
        public CurrencyType RewardType => rewardType;

        [SerializeField] int rewardAmount;
        public int RewardAmount => rewardAmount;

        private Pool pool;
        public Pool Pool => pool;

        public void Initialise()
        {
            pool = new Pool(new PoolSettings(prefab.name, prefab, 0, true));
        }

        public enum Type
        {
            Cat_01 = 0,
            Cat_02 = 1,
            Cat_03 = 2,
            Cat_04 = 3,
            Cat_05 = 4,
        }
    }
}