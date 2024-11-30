using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class Sickness
    {
        [SerializeField] SicknessType sicknessType;
        public SicknessType SicknessType => sicknessType;

        [SerializeField] Item.Type requiredItem;
        public Item.Type RequiredItem => requiredItem;

        [SerializeField] AnimalBehaviourCase[] sicknessBehavioursOverride;

        [SerializeField] GameObject sicknessParticleObject;

        private int particleHash;
        public int ParticleHash => particleHash;

        public void Initialise()
        {
            for (int i = 0; i < sicknessBehavioursOverride.Length; i++)
            {
                sicknessBehavioursOverride[i].Initialise();
            }

            // Register particle
            particleHash = ParticlesController.RegisterParticle(sicknessType.ToString() + requiredItem.ToString(), sicknessParticleObject);
        }

        public void Unload()
        {
            for (int i = 0; i < sicknessBehavioursOverride.Length; i++)
            {
                sicknessBehavioursOverride[i].Pool.Clear();
            }
        }

        public SicknessBehaviour GetSicknessBehaviour(Animal.Type animalType)
        {
            for (int i = 0; i < sicknessBehavioursOverride.Length; i++)
            {
                if (sicknessBehavioursOverride[i].AnimalType == animalType)
                {
                    return sicknessBehavioursOverride[i].Pool.GetPooledObject().GetComponent<SicknessBehaviour>();
                }
            }

            Debug.LogError("Sickness with type " + sicknessType + " for " + animalType + " is missing!");

            return null;
        }

        [System.Serializable]
        public class AnimalBehaviourCase
        {
            [SerializeField] Animal.Type animalType;
            public Animal.Type AnimalType => animalType;

            [SerializeField] SicknessBehaviour[] sicknessBehaviours;
            public SicknessBehaviour[] SicknessBehaviour => sicknessBehaviours;

            private Pool pool;
            public Pool Pool => pool;

            public void Initialise()
            {
                // Create multi pool prefabs list
                List<Pool.MultiPoolPrefab> multiPoolPrefabs = new List<Pool.MultiPoolPrefab>();
                for (int i = 0; i < sicknessBehaviours.Length; i++)
                {
                    multiPoolPrefabs.Add(new Pool.MultiPoolPrefab(sicknessBehaviours[i].gameObject, 10, false));
                }

                // Create pool
                pool = new Pool(new PoolSettings(animalType.ToString(), multiPoolPrefabs, 0, true));
            }
        }
    }
}