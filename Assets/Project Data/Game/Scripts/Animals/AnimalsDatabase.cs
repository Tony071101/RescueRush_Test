using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Animals Database", menuName = "Content/Animals/Animals Database")]
    public class AnimalsDatabase : ScriptableObject
    {
        [SerializeField] Animal[] animals;
        public Animal[] Animals => animals;

        [SerializeField] GameObject[] visitorPrefabs;
        public GameObject[] VisitorPrefabs => visitorPrefabs;

        public void Initialise()
        {
            for (int i = 0; i < animals.Length; i++)
            {
                animals[i].Initialise();
            }
        }
    }
}