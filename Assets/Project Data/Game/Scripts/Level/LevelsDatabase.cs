using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Levels Database", menuName = "Content/Levels Database")]
    public class LevelsDatabase : ScriptableObject
    {
        [SerializeField] LevelData[] levels;
        public LevelData[] Levels => levels;

        public LevelData GetLevelByIndex(int index)
        {
            if (levels.IsInRange(index))
            {
                return levels[index];
            }

            return levels.GetRandomItem();
        }
    }
}