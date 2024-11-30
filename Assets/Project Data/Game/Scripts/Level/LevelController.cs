using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;
using Watermelon.LevelSystem;
using Watermelon.Upgrades;

namespace Watermelon
{
    public class LevelController : MonoBehaviour
    {
        private static LevelController levelController;

        [SerializeField] LevelsDatabase levelsDatabase;
        [SerializeField] AnimalsDatabase animalsDatabase;

        [SerializeField] SicknessDatabase sicknessDatabase;
        [SerializeField] NavMeshSurface navMeshSurface;

        [Space]
        [SerializeField] GameObject purchaseZoneObject;

        [SerializeField] GameObject tablePurchaseZoneObject;

        [SerializeField] GameObject tableAreaPurchaseZoneObject;
        [SerializeField] GameObject tableAreaAdButtonObject;

        [SerializeField] GameObject purchaseAreaAdTriggerObject;

        [SerializeField] GameObject waitingIndicatorObject;

        [Header("Player")]
        [SerializeField] GameObject playerPrefabObject;

        [Header("Nurse")]
        [SerializeField] GameObject nursePrefabObject;

        private static Level currentLevel;
        public static Level CurrentLevel => currentLevel;

        private static int currentLevelID;
        public static int CurrentLevelID => currentLevelID;

        // Player
        private static PlayerBehavior playerBehavior;

        // Animals
        private static Pool visitorPool;
        private static Animal[] animals;
        private static Dictionary<Animal.Type, int> animalsLink;

        // Sicknesses
        private static Sickness[] sicknesses;
        private static Dictionary<SicknessType, int> sicknessesLink;

        // Zones
        private static Pool purchaseZonePool;
        private static Pool tablePurchaseZonePool;
        private static Pool tableAreaPurchaseZonePool;
        private static Pool tableAreaAdButtonPool;
        private static Pool purchaseAreaAdTriggerPool;

        // Waiting Indicator
        private static Pool waitingIndicatorPool;

        // Nurse
        private static Pool nursePool;

        // Upgrades
        private PickUpSpeedUpgrade pickUpSpeedUpgrade;
        private NurseMovementSpeedUpgrade nurseSpeedUpgrade;

        private static float itemPickUpDuration;
        public static float ItemPickUpDuration => itemPickUpDuration;

        private static float animalPickUpDuration;
        public static float AnimalPickUpDuration => animalPickUpDuration;

        private static float nurseMovementSpeed;
        public static float NurseMovementSpeed => nurseMovementSpeed;

        private static float nurseAngularSpeed;
        public static float NurseAngularSpeed => nurseAngularSpeed;

        private static float nurseAcceleration;
        public static float NurseAcceleration => nurseAcceleration;

        private static float nurseBlendTreeMultiplier;
        public static float NurseBlendTreeMultiplier => nurseBlendTreeMultiplier;

        // Events
        public static OnLevelLoadedCallback OnLevelLoaded;

        private static LevelSave levelSaveData;
        private static GlobalLevelSave globalLevelSaveData;
        private static int loadedLevelIndex;

        public void Initialise()
        {
            levelController = this;

            // Create purchase zone pool
            purchaseZonePool = new Pool(new PoolSettings(purchaseZoneObject.name, purchaseZoneObject, 0, true));
            tablePurchaseZonePool = new Pool(new PoolSettings(tablePurchaseZoneObject.name, tablePurchaseZoneObject, 0, true));
            tableAreaPurchaseZonePool = new Pool(new PoolSettings(tableAreaPurchaseZoneObject.name, tableAreaPurchaseZoneObject, 0, true));
            tableAreaAdButtonPool = new Pool(new PoolSettings(tableAreaAdButtonObject.name, tableAreaAdButtonObject, 0, true));
            purchaseAreaAdTriggerPool = new Pool(new PoolSettings(purchaseAreaAdTriggerObject.name, purchaseAreaAdTriggerObject, 0, true));

            // Create waiting indicator pool
            waitingIndicatorPool = new Pool(new PoolSettings(waitingIndicatorObject.name, waitingIndicatorObject, 1, true));

            // Initialise animals
            animalsDatabase.Initialise();

            // Initialise sicknesses
            sicknessDatabase.Initialise();

            // Link sicknesses
            sicknesses = sicknessDatabase.Sicknesses;
            sicknessesLink = new Dictionary<SicknessType, int>();
            for (int i = 0; i < sicknesses.Length; i++)
            {
                sicknessesLink.Add(sicknesses[i].SicknessType, i);
            }

            // Create visitor pool
            List<Pool.MultiPoolPrefab> multiPoolPrefabs = new List<Pool.MultiPoolPrefab>();
            for (int i = 0; i < animalsDatabase.VisitorPrefabs.Length; i++)
            {
                multiPoolPrefabs.Add(new Pool.MultiPoolPrefab(animalsDatabase.VisitorPrefabs[i], 1, false));
            }

            // Visitor
            visitorPool = new Pool(new PoolSettings("Visitor", multiPoolPrefabs, 3, true));

            // Nurse
            nursePool = new Pool(new PoolSettings("Nurse", nursePrefabObject, 0, true));

            // Link animals list
            animals = animalsDatabase.Animals;
            animalsLink = new Dictionary<Animal.Type, int>();
            for (int i = 0; i < animals.Length; i++)
            {
                animalsLink.Add(animals[i].AnimalType, i);
            }

            pickUpSpeedUpgrade = UpgradesController.GetUpgrade<PickUpSpeedUpgrade>(UpgradeType.PlayerPickUpTime);
            pickUpSpeedUpgrade.OnUpgraded += OnPickUpTimeUpgraded;

            nurseSpeedUpgrade = UpgradesController.GetUpgrade<NurseMovementSpeedUpgrade>(UpgradeType.NurseSpeed);
            nurseSpeedUpgrade.OnUpgraded += OnNurseSpeedUpgraded;

            RecalculatePickUpSpeed();
            RecalculateNurseSpeed();

            globalLevelSaveData = SaveController.GetSaveObject<GlobalLevelSave>("globalLevelSave");

            // Load level with save
            LoadLevel(globalLevelSaveData.CurrentLevelID);
        }

        private void RecalculatePickUpSpeed()
        {
            PickUpSpeedUpgrade.PickUpSpeedStage pickUpSpeedStage = pickUpSpeedUpgrade.GetCurrentStage();

            itemPickUpDuration = pickUpSpeedStage.ItemPickUpDuration;
            animalPickUpDuration = pickUpSpeedStage.AnimalPickUpDuration;
        }

        private void RecalculateNurseSpeed()
        {
            NurseMovementSpeedUpgrade.NurseMovementSpeedStage nurseSpeedStage = nurseSpeedUpgrade.GetCurrentStage();

            nurseMovementSpeed = nurseSpeedStage.NurseMovementSpeed;
            nurseAcceleration = nurseSpeedStage.NurseAcceleration;
            nurseAngularSpeed = nurseSpeedStage.NurseAngularSpeed;
            nurseBlendTreeMultiplier = nurseSpeedStage.NurseBlendTreeMultiplier;
        }

        private void OnPickUpTimeUpgraded()
        {
            RecalculatePickUpSpeed();
        }

        private void OnNurseSpeedUpgraded()
        {
            RecalculateNurseSpeed();

            currentLevel.RecalculateNurses();
        }

        public void UnloadLevel(Tween.TweenCallback onSceneUnloaded = null)
        {
            purchaseZonePool.ReturnToPoolEverything(true);
            tablePurchaseZonePool.ReturnToPoolEverything(true);
            tableAreaPurchaseZonePool.ReturnToPoolEverything(true);
            tableAreaAdButtonPool.ReturnToPoolEverything(true);
            purchaseAreaAdTriggerPool.ReturnToPoolEverything(true);
            waitingIndicatorPool.ReturnToPoolEverything(true);
            visitorPool.ReturnToPoolEverything(true);
            nursePool.ReturnToPoolEverything(true);

            for(int i = 0; i < sicknesses.Length; i++)
            {
                sicknesses[i].Unload();
            }

            for (int i = 0; i < animals.Length; i++)
            {
                animals[i].Pool.Clear();
            }

            NavMeshController.Reset();

            playerBehavior.Unload();
            currentLevel.Unload();

            SceneManager.UnloadSceneAsync(levelsDatabase.GetLevelByIndex(loadedLevelIndex).LevelName, UnloadSceneOptions.None).OnCompleted(onSceneUnloaded);
        }

        public static void ActivateLevel(int index)
        {
            globalLevelSaveData.CurrentLevelID = index;

            levelController.LoadLevel(index);
        }

        private void LoadLevel(int levelID)
        {
            loadedLevelIndex = levelID;

            levelSaveData = SaveController.GetSaveObject<LevelSave>(string.Format("level_{0}", levelID));

            NavMeshController.Initialise(levelController.navMeshSurface);

            SceneManager.LoadScene(levelsDatabase.GetLevelByIndex(levelID).LevelName, LoadSceneMode.Additive);
        }

        public static void OnLevelCreated(Level level)
        {
            // Get level component from created object
            currentLevel = level;
            currentLevel.OnLevelLoaded(levelSaveData);

            // Spawn player
            GameObject playerObject = Instantiate(levelController.playerPrefabObject);
            playerObject.transform.ResetLocal();

            playerBehavior = playerObject.GetComponent<PlayerBehavior>();
            playerBehavior.Initialise();

            // Initialise player on level
            currentLevel.InitialisePlayer(playerBehavior);

            // Recalculate nurses speed
            currentLevel.RecalculateNurses();

            playerObject.transform.position = currentLevel.GetSpawnPoint();

            // Link main camera to target
            CameraController.SetMainTarget(playerBehavior.CameraTarget);

            NavMeshController.CalculateNavMesh(delegate
            {
                // Invoke level loaded callback
                OnLevelLoaded?.Invoke(currentLevel);
            });
        }

        public void OnGameLoaded()
        {

        }

        #region Animals
        public static Animal GetAnimal(Animal.Type animalType)
        {
            return animals[animalsLink[animalType]];
        }

        public static VisitorBehaviour SpawnVisitor()
        {
            GameObject visitorObject = visitorPool.GetPooledObject(false);
            visitorObject.transform.ResetGlobal();
            visitorObject.transform.rotation = Quaternion.Euler(0, 180, 0);
            visitorObject.transform.localScale = Vector3.one;
            visitorObject.SetActive(true);

            return visitorObject.GetComponent<VisitorBehaviour>();
        }
        #endregion

        #region Zones
        public static PurchaseAreaBehaviour CreatePurchaseZone()
        {
            GameObject purchaseZone = purchaseZonePool.GetPooledObject();
            purchaseZone.SetActive(true);

            return purchaseZone.GetComponent<PurchaseAreaBehaviour>();
        }

        public static PurchaseAreaBehaviour CreateTablePurchaseZone()
        {
            GameObject purchaseZone = tablePurchaseZonePool.GetPooledObject();
            purchaseZone.SetActive(true);

            return purchaseZone.GetComponent<PurchaseAreaBehaviour>();
        }

        public static PurchaseAreaBehaviour CreateTableAreaPurchaseZone()
        {
            GameObject purchaseZone = tableAreaPurchaseZonePool.GetPooledObject();
            purchaseZone.SetActive(true);

            return purchaseZone.GetComponent<PurchaseAreaBehaviour>();
        }

        public static TableZoneAdButtonBehaviour CreateTableZoneAdButton()
        {
            GameObject purchaseZoneAdButton = tableAreaAdButtonPool.GetPooledObject();
            purchaseZoneAdButton.SetActive(true);

            return purchaseZoneAdButton.GetComponent<TableZoneAdButtonBehaviour>();
        }

        public static PurchaseAreaAdTrigger CreatePurchaseAreaAdTrigger()
        {
            GameObject areaAdTriggerObject = purchaseAreaAdTriggerPool.GetPooledObject();
            areaAdTriggerObject.SetActive(true);

            return areaAdTriggerObject.GetComponent<PurchaseAreaAdTrigger>();

        }
        #endregion

        #region Waiting Indicator
        public static WaitingIndicatorBehaviour CreateWaitingIndicator(Vector3 spawnPosition)
        {
            GameObject waitingIndicatorObject = waitingIndicatorPool.GetPooledObject();
            waitingIndicatorObject.transform.position = spawnPosition;
            waitingIndicatorObject.SetActive(true);

            return waitingIndicatorObject.GetComponent<WaitingIndicatorBehaviour>();
        }
        #endregion

        #region Nurse
        public static NurseBehaviour SpawnNurse(Vector3 spawnPosition)
        {
            GameObject nurseObject = nursePool.GetPooledObject();
            nurseObject.transform.position = spawnPosition;
            nurseObject.SetActive(true);

            return nurseObject.GetComponent<NurseBehaviour>();
        }
        #endregion

        #region Sicknesses
        public static Sickness GetSickness(SicknessType sicknessType)
        {
            return sicknesses[sicknessesLink[sicknessType]];
        }
        #endregion

        public delegate void OnLevelLoadedCallback(Level level);
    }
}