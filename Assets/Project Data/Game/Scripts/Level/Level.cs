using System;
using UnityEngine;

namespace Watermelon
{
    public class Level : MonoBehaviour
    {
        [SerializeField] Transform spawnPoint;

        [SerializeField] LevelTutorialBehaviour levelTutorialBehaviour;

        [SerializeField] Zone[] zones;
        public Zone[] Zones => zones;

        public void Start()
        {
            LevelController.OnLevelCreated(this);
        }

        public void OnLevelLoaded(LevelSave levelSave)
        {
            // Check if save is exists (should be null on first launch)
            if (levelSave != null && !levelSave.ZoneSaves.IsNullOrEmpty())
            {
                for (int i = 0; i < levelSave.ZoneSaves.Length; i++)
                {
                    for (int j = 0; j < zones.Length; j++)
                    {
                        if (zones[j].ID == levelSave.ZoneSaves[i].ID)
                        {
                            zones[j].Load(levelSave.ZoneSaves[i]);

                            break;
                        }
                    }
                }
            }

            levelSave.LinkZones(zones);
        }

        public void InitialisePlayer(PlayerBehavior playerBehavior)
        {
            for (int i = 0; i < zones.Length; i++)
            {
                zones[i].Initialise(playerBehavior);
            }

            if (levelTutorialBehaviour != null)
                levelTutorialBehaviour.Initialise(this);
        }

        public void OnGameLoaded()
        {
            if (levelTutorialBehaviour != null)
            {
                levelTutorialBehaviour.OnGameLoaded();
            }
        }

        public void RecalculateNurses()
        {
            for (int i = 0; i < zones.Length; i++)
            {
                zones[i].RecalculateNurses();
            }
        }

        public void Unload()
        {
            for (int i = 0; i < zones.Length; i++)
            {
                zones[i].AnimalSpawner.DisableAutoSpawn();
            }
        }

        public Vector3 GetSpawnPoint()
        {
            return spawnPoint.position;
        }

        [Button("Pick Zones")]
        public void PickZones()
        {
            zones = GetComponentsInChildren<Zone>();
            for (int i = 0; i < zones.Length; i++)
            {
                zones[i].ID = i;

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(zones[i]);
#endif
            }
        }
    }
}