using UnityEngine;
using Watermelon.Store;

namespace Watermelon
{
    public class GameController : MonoBehaviour
    {
        private static GameController instance;

        [SerializeField] GameSettings gameSettings;

        [Header("Refferences")]
        [SerializeField] LevelController levelController;
        [SerializeField] PoolManager poolManager;
        [SerializeField] CurrenciesController currenciesController;
        [SerializeField] ItemController itemController;
        [SerializeField] UpgradesController upgradesController;
        [SerializeField] ParticlesController particlesController;
        [SerializeField] FloatingTextController floatingTextController;
        [SerializeField] UIController uiController;
        [SerializeField] NavigationController navigationController;
        [SerializeField] TutorialController tutorialController;

        public static GameSettings Settings => instance.gameSettings;

        private void Awake()
        {
            instance = this;

            SaveController.Initialise(true);
        }

        private void Start()
        {
            InitialiseGame();
        }

        public void InitialiseGame()
        {
            uiController.Initialise();

            itemController.Initialise();

            currenciesController.Initialise();

            upgradesController.Initialise();

            navigationController.Initialise();

            StoreController.Init();

            levelController.Initialise();

            particlesController.Initialise();

            floatingTextController.Inititalise();

            tutorialController.Initialise();

            uiController.InitialisePages();

            UIController.ShowPage<UIGame>();

            OnGameLoaded();
        }

        public static void OnGameLoaded()
        {
            GameLoading.MarkAsReadyToHide();

            // Unzoom camera
            CameraController.EnableCamera(CameraType.Main);

            instance.levelController.OnGameLoaded();
        }

        public static void Unload(Tween.TweenCallback onSceneUnloaded)
        {
            Tween.RemoveAll();

            ParticlesController.ClearParticles();
            FloatingTextController.Unload();
            NavigationController.Unload();
            TutorialController.Unload();
            ItemController.Unload();

            instance.levelController.UnloadLevel(onSceneUnloaded);
        }

#region Debug
        [Button("Remove Save")]
        private void RemoveSave()
        {
            if(!Application.isPlaying)
            {
                Serializer.DeleteFileAtPDP("save");
            }
        }
#endregion
    }
}