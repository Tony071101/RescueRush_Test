using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Sicknesses Database", menuName = "Content/Sicknesses/Sicknesses Database")]
    public class SicknessDatabase : ScriptableObject
    {
        [SerializeField] Sickness[] sicknesses;
        public Sickness[] Sicknesses => sicknesses;

        public void Initialise()
        {
            for (int i = 0; i < sicknesses.Length; i++)
            {
                sicknesses[i].Initialise();
            }
        }
    }
}