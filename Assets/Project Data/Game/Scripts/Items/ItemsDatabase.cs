using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Items Database", menuName = "Content/Items/Items Database")]
    public class ItemsDatabase : ScriptableObject
    {
        [SerializeField] Item[] items;
        public Item[] Items => items;

        public void Inititalise()
        {
            for (int i = 0; i < items.Length; i++)
            {
                items[i].Initialise();
            }
        }
    }
}