using Cinemachine;
using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon
{
    public class LevelTutorialBehaviour : MonoBehaviour
    {
        private const float ARROW_DISABLE_DISTANCE = 7f;

        private const Animal.Type FIRST_ANIMAL_TYPE = Animal.Type.Cat_01;
        private const SicknessType FIRST_SICKNESS_TYPE = SicknessType.Dirt;
        private const Item.Type FIRST_ITEM_TYPE = Item.Type.Soap;

        private const Animal.Type SECOND_ANIMAL_TYPE = Animal.Type.Cat_02;
        private const SicknessType SECOND_SICKNESS_TYPE = SicknessType.Infection;
        private const Item.Type SECOND_ITEM_TYPE = Item.Type.Injection;

        [SerializeField] Color arrowColor = Color.white;

        [SerializeField] Zone firstZone;

        [SerializeField] AnimalSpawner animalSpawner;
        [SerializeField] Transform animalTableTransform;
        [SerializeField] Transform hireZoneTransform;

        [Space]
        [SerializeField] DispenserBuilding soapDispenser;
        [SerializeField] DispenserBuilding injectionDispenser;

        [Space]
        [SerializeField] MoneyStackBehaviour moneyStackBehaviour;

        [Space]
        [SerializeField] SecretaryInteractionZone secretaryInteractionZone;

        [Space]
        [SerializeField] TableBehaviour secondTableBehaviour;
        [SerializeField] TableBehaviour thirdTableBehaviour;
        [SerializeField] TableBehaviour fourthTableBehaviour;
        [SerializeField] TableZoneBehaviour tableZoneBehaviour;
        [SerializeField] TableZoneBehaviour secondTableZoneBehaviour;

        [Space]
        [SerializeField] Zone secondZone;
        [SerializeField] Zone thirdZone;
        [SerializeField] Transform secondZonePurchaseTransform;

        // Stage 1
        private VisitorBehaviour spawnedVisitor;
        private AnimalBehaviour spawnedAnimal;

        private NavigationController.ArrowCase navigationArrow;

        private TutorialArrowBehaviour tutorialArrowBehaviour;

        private Level level;

        private CinemachineVirtualCamera tutorialVirtualCamera;

        private bool isItemPicked;
        private bool isAnimalPicked;

        private UIGame uiGame;

        public void Initialise(Level level)
        {
            this.level = level;

            // Get UI Game Page
            uiGame = UIController.GetPage<UIGame>();

            // Get tutorial virtual camera
            tutorialVirtualCamera = CameraController.GetCamera(CameraType.Tutorial).VirtualCamera;

            // Subscribe to tutorial callback
            TutorialController.OnTutorialStageCompleted += OnTutorialStageCompleted;

            TutorialController.OnZoneOpenedEvent += OnZoneOpened;
            TutorialController.OnAnimalPickedEvent += OnAnimalPicked;
            TutorialController.OnAnimalCuredEvent += OnAnimalCured;
            TutorialController.OnAnimalPlacedOnTableEvent += OnAnimalPlacedOnTable;
            TutorialController.OnItemPickedEvent += OnItemPicked;
            TutorialController.OnMoneyPickedEvent += OnMoneyPicked;
            TutorialController.OnTableUnlockedEvent += OnTableUnlocked;
            TutorialController.OnTableZoneUnlockedEvent += OnTableZoneUnlocked;
            TutorialController.OnNurseHiredEvent += OnNurseHired;

            // Activate stage
            ActivateStage(TutorialController.CurrentTutorialStage);
        }

        private void OnDisable()
        {
            TutorialController.OnTutorialStageCompleted -= OnTutorialStageCompleted;

            TutorialController.OnZoneOpenedEvent -= OnZoneOpened;
            TutorialController.OnAnimalPickedEvent -= OnAnimalPicked;
            TutorialController.OnAnimalCuredEvent -= OnAnimalCured;
            TutorialController.OnAnimalPlacedOnTableEvent -= OnAnimalPlacedOnTable;
            TutorialController.OnItemPickedEvent -= OnItemPicked;
            TutorialController.OnMoneyPickedEvent -= OnMoneyPicked;
            TutorialController.OnTableUnlockedEvent -= OnTableUnlocked;
            TutorialController.OnTableZoneUnlockedEvent -= OnTableZoneUnlocked;
            TutorialController.OnNurseHiredEvent -= OnNurseHired;
        }

        private void OnNurseHired()
        {
            if (TutorialController.CurrentTutorialStage == TutorialStage.HireNurse)
            {
                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                TutorialController.EnableTutorial(TutorialStage.UnlockSecondTableZone);
            }
        }

        private void OnTableZoneUnlocked(TableZoneBehaviour tableZoneBehaviour)
        {
            if (TutorialController.CurrentTutorialStage == TutorialStage.UnlockTableZone && tableZoneBehaviour.TableZoneID == 1)
            {
                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                TutorialController.EnableTutorial(TutorialStage.HireNurse);
            }
            else if (TutorialController.CurrentTutorialStage == TutorialStage.UnlockSecondTableZone && tableZoneBehaviour.TableZoneID == 2)
            {
                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                TutorialController.EnableTutorial(TutorialStage.UnlockNextLocation);

                // Disable joystick
                uiGame.Joystick.DisableControl();

                tutorialVirtualCamera.LookAt = secondZonePurchaseTransform;
                tutorialVirtualCamera.Follow = secondZonePurchaseTransform;

                CameraController.EnableCamera(CameraType.Tutorial);

                Tween.DelayedCall(2.0f, delegate
                {
                    CameraController.EnableCamera(CameraType.Main);

                    // Enable joystick
                    uiGame.Joystick.EnableControl();

                    // Check if game is rated
                    if (!RateUsController.IsRated())
                    {
                        Tween.DelayedCall(0.5f, delegate
                        {
                            // Open rate us page
                            UIController.ShowPage<UIRateUs>();
                        });
                    }
                });
            }
        }

        private void OnMoneyPicked()
        {
            if (TutorialController.CurrentTutorialStage == TutorialStage.PickMoney)
            {
                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                TutorialController.EnableTutorial(TutorialStage.UnlockFirstTable);
            }
        }

        private void OnZoneOpened(Zone zone)
        {
            if (TutorialController.CurrentTutorialStage == TutorialStage.UnlockNextLocation)
            {
                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                TutorialController.EnableTutorial(TutorialStage.Done);
            }
        }

        private void OnItemPicked(Item item)
        {
            if (isItemPicked)
                return;

            // Get player behaviour
            PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();

            if (TutorialController.CurrentTutorialStage == TutorialStage.PickFirstAnimal && item.ItemType == FIRST_ITEM_TYPE)
            {
                isItemPicked = true;

                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, animalTableTransform, arrowColor, ARROW_DISABLE_DISTANCE, true);
            }
            else if (TutorialController.CurrentTutorialStage == TutorialStage.PickSecondAnimal && item.ItemType == SECOND_ITEM_TYPE)
            {
                isItemPicked = true;

                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, animalTableTransform, arrowColor, ARROW_DISABLE_DISTANCE, true);
            }
        }

        private void OnAnimalPlacedOnTable(AnimalBehaviour animalBehaviour, TableBehaviour tableBehaviour)
        {
            // Get player behaviour
            PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();

            if (TutorialController.CurrentTutorialStage == TutorialStage.PickFirstAnimal)
            {
                isAnimalPicked = false;

                // Block dispensers
                injectionDispenser.SetUnlockState(false);
                soapDispenser.SetUnlockState(true);

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(soapDispenser.transform.position + new Vector3(0, 8, 0));

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, soapDispenser.transform, arrowColor, ARROW_DISABLE_DISTANCE, true);

                // Disable joystick
                uiGame.Joystick.DisableControl();

                tutorialVirtualCamera.LookAt = soapDispenser.transform;
                tutorialVirtualCamera.Follow = soapDispenser.transform;

                CameraController.EnableCamera(CameraType.Tutorial);

                Tween.DelayedCall(2.0f, delegate
                {
                    CameraController.EnableCamera(CameraType.Main);

                    // Enable joystick
                    uiGame.Joystick.EnableControl();
                });
            }
            else if (TutorialController.CurrentTutorialStage == TutorialStage.PickSecondAnimal)
            {
                isAnimalPicked = false;

                // Block dispensers
                injectionDispenser.SetUnlockState(true);
                soapDispenser.SetUnlockState(true);

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(injectionDispenser.transform.position + new Vector3(0, 8, 0));

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, injectionDispenser.transform, arrowColor, ARROW_DISABLE_DISTANCE, true);
            }
        }

        private void OnAnimalCured(AnimalBehaviour animalBehaviour)
        {
            if (TutorialController.CurrentTutorialStage == TutorialStage.PickFirstAnimal)
            {
                isItemPicked = false;

                TutorialController.EnableTutorial(TutorialStage.PickSecondAnimal);
            }
            else if (TutorialController.CurrentTutorialStage == TutorialStage.PickSecondAnimal)
            {
                isItemPicked = false;

                TutorialController.EnableTutorial(TutorialStage.PickMoney);
            }
        }

        private void OnAnimalPicked(AnimalBehaviour animalBehaviour)
        {
            if (isAnimalPicked)
                return;

            // Get player behaviour
            PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();

            if (TutorialController.CurrentTutorialStage == TutorialStage.PickFirstAnimal)
            {
                isAnimalPicked = true;

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, animalTableTransform, arrowColor, ARROW_DISABLE_DISTANCE, true);
            }
            else if (TutorialController.CurrentTutorialStage == TutorialStage.PickSecondAnimal)
            {
                isAnimalPicked = true;

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, animalTableTransform, arrowColor, ARROW_DISABLE_DISTANCE, true);
            }
        }

        private void OnTableUnlocked(TableBehaviour tableBehaviour)
        {
            if (TutorialController.CurrentTutorialStage == TutorialStage.UnlockFirstTable)
            {
                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                TutorialController.EnableTutorial(TutorialStage.UnlockSecondTable);
            }
            else if (TutorialController.CurrentTutorialStage == TutorialStage.UnlockSecondTable)
            {
                if (tutorialArrowBehaviour != null)
                    tutorialArrowBehaviour.Disable();

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                TutorialController.EnableTutorial(TutorialStage.UnlockTableZone);
            }
        }

        public void OnGameLoaded()
        {

        }

        private void ActivateStage(TutorialStage tutorialStage)
        {
            // Get player behaviour
            PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();

            if (tutorialStage == TutorialStage.PickFirstAnimal)
            {
                secondZone.Lock();
                thirdZone.Lock();

                // Block dispensers
                injectionDispenser.SetUnlockState(false);
                soapDispenser.SetUnlockState(false);

                // Disable auto spawn
                animalSpawner.DisableAutoSpawn();

                // Disable tables
                secondTableBehaviour.Lock();
                thirdTableBehaviour.Lock();
                fourthTableBehaviour.Lock();

                tableZoneBehaviour.Lock();

                // Set secretary zone block
                secretaryInteractionZone.SetBlockState(true);

                NavMeshController.InvokeOrSubscribe(delegate
                {
                    spawnedVisitor = animalSpawner.SpawnVisitorWithRandomAnimal(FIRST_ANIMAL_TYPE, FIRST_SICKNESS_TYPE);
                    spawnedAnimal = spawnedVisitor.AnimalBehaviour;

                    navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, spawnedAnimal.transform, arrowColor, ARROW_DISABLE_DISTANCE, true);

                    // Disable joystick
                    uiGame.Joystick.DisableControl();

                    tutorialVirtualCamera.LookAt = spawnedVisitor.transform;
                    tutorialVirtualCamera.Follow = spawnedVisitor.transform;

                    CameraController.EnableCamera(CameraType.Tutorial);

                    Tween.DelayedCall(2.0f, delegate
                    {
                        CameraController.EnableCamera(CameraType.Main);

                        // Enable joystick
                        uiGame.Joystick.EnableControl();
                    });
                });

                // Block interstitial
                AdsManager.SetInterstitialDelayTime(99999);
            }
            else if (tutorialStage == TutorialStage.PickSecondAnimal)
            {
                secondZone.Lock();
                thirdZone.Lock();

                // Block dispensers
                injectionDispenser.SetUnlockState(false);
                soapDispenser.SetUnlockState(true);

                // Disable auto spawn
                animalSpawner.DisableAutoSpawn();

                // Disable tables
                secondTableBehaviour.Lock();
                thirdTableBehaviour.Lock();
                fourthTableBehaviour.Lock();

                tableZoneBehaviour.Lock();

                // Set secretary zone block
                secretaryInteractionZone.SetBlockState(true);

                NavMeshController.InvokeOrSubscribe(delegate
                {
                    spawnedVisitor = animalSpawner.SpawnVisitorWithRandomAnimal(SECOND_ANIMAL_TYPE, SECOND_SICKNESS_TYPE);
                    spawnedAnimal = spawnedVisitor.AnimalBehaviour;

                    if (navigationArrow != null)
                        navigationArrow.DisableArrow();

                    navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, spawnedAnimal.transform, arrowColor, ARROW_DISABLE_DISTANCE, true);
                });

                // Block interstitial
                AdsManager.SetInterstitialDelayTime(99999);
            }
            else if (tutorialStage == TutorialStage.PickMoney)
            {
                if (moneyStackBehaviour.CollectedMoney == 0)
                {
                    int totalMoney = 40 - CurrenciesController.Get(CurrencyType.Money);
                    if (totalMoney != 0)
                    {
                        moneyStackBehaviour.AddMoney(totalMoney);
                    }
                }

                secondZone.Lock();
                thirdZone.Lock();

                // Enable auto animal spawner
                animalSpawner.StartAutoSpawn(0.0f);

                // Disable tables
                secondTableBehaviour.Lock();
                thirdTableBehaviour.Lock();
                fourthTableBehaviour.Lock();

                tableZoneBehaviour.Lock();

                // Set secretary zone block
                secretaryInteractionZone.SetBlockState(true);

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(moneyStackBehaviour.transform.position + new Vector3(0, 8, 0));

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, moneyStackBehaviour.transform, arrowColor, ARROW_DISABLE_DISTANCE, true);

                // Block interstitial
                AdsManager.SetInterstitialDelayTime(99999);
            }
            else if (tutorialStage == TutorialStage.UnlockFirstTable)
            {
                secondZone.Lock();
                thirdZone.Lock();

                // Disable tables
                secondTableBehaviour.Unlock();
                thirdTableBehaviour.Lock();
                fourthTableBehaviour.Lock();

                tableZoneBehaviour.Lock();

                // Set secretary zone block
                secretaryInteractionZone.SetBlockState(true);

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(secondTableBehaviour.transform.position + new Vector3(0, 8, 0));

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, secondTableBehaviour.transform, arrowColor, ARROW_DISABLE_DISTANCE, true);

                // Block interstitial
                AdsManager.SetInterstitialDelayTime(99999);
            }
            else if (tutorialStage == TutorialStage.UnlockSecondTable)
            {
                secondZone.Lock();
                thirdZone.Lock();

                // Disable tables
                secondTableBehaviour.Unlock();
                thirdTableBehaviour.Unlock();
                fourthTableBehaviour.Lock();

                tableZoneBehaviour.Lock();

                // Set secretary zone block
                secretaryInteractionZone.SetBlockState(true);

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(thirdTableBehaviour.transform.position + new Vector3(0, 8, 0));

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                // Block interstitial
                AdsManager.SetInterstitialDelayTime(99999);
            }
            else if (tutorialStage == TutorialStage.UnlockTableZone)
            {
                secondZone.Lock();
                thirdZone.Lock();

                // Disable tables
                secondTableBehaviour.Unlock();
                thirdTableBehaviour.Unlock();
                fourthTableBehaviour.Unlock();

                tableZoneBehaviour.Unlock();

                secondTableZoneBehaviour.Lock();

                // Set secretary zone block
                secretaryInteractionZone.SetBlockState(true);

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(tableZoneBehaviour.transform.position + new Vector3(0, 8, 0));

                // Block interstitial
                AdsManager.SetInterstitialDelayTime(99999);
            }
            else if (tutorialStage == TutorialStage.HireNurse)
            {
                secondZone.Lock();
                thirdZone.Lock();

                secondTableBehaviour.Unlock();
                thirdTableBehaviour.Unlock();
                fourthTableBehaviour.Unlock();

                tableZoneBehaviour.Unlock();
                secondTableZoneBehaviour.Lock();

                // Set secretary zone block
                secretaryInteractionZone.SetBlockState(false);

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(hireZoneTransform.position + new Vector3(0, 8, 0));

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                navigationArrow = NavigationController.RegisterArrow(playerBehavior.transform, hireZoneTransform, arrowColor, ARROW_DISABLE_DISTANCE, true);

                // Block interstitial
                AdsManager.SetInterstitialDelayTime(99999);
            }
            else if (tutorialStage == TutorialStage.UnlockSecondTableZone)
            {
                secondZone.Lock();
                thirdZone.Lock();

                secondTableBehaviour.Unlock();
                thirdTableBehaviour.Unlock();
                fourthTableBehaviour.Unlock();

                tableZoneBehaviour.Unlock();
                secondTableZoneBehaviour.Unlock();

                if (navigationArrow != null)
                    navigationArrow.DisableArrow();

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(secondTableZoneBehaviour.transform.position + new Vector3(0, 8, 0));

                // Enable interstitial
                AdsManager.SetInterstitialDelayTime(0);
            }
            else if (tutorialStage == TutorialStage.UnlockNextLocation)
            {
                secondZone.Unlock();
                thirdZone.Unlock();

                tutorialArrowBehaviour = TutorialController.CreateTutorialArrow(secondZonePurchaseTransform.position + new Vector3(0, 8, 0));
            }
        }

        private void DisableStage(TutorialStage tutorialStage)
        {

        }

        private void OnTutorialStageCompleted(TutorialStage tutorialStage, TutorialStage previousStage)
        {
            if (previousStage != TutorialStage.None)
                DisableStage(previousStage);

            ActivateStage(tutorialStage);
        }
    }
}