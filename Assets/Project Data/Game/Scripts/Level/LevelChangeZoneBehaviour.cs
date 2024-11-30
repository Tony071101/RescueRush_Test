using UnityEngine;

namespace Watermelon
{
    public class LevelChangeZoneBehaviour : MonoBehaviour
    {
        [SerializeField] int levelIndex;

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                UIGame gameUI = UIController.GetPage<UIGame>();

                // Show fullscreen black overlay
                gameUI.ShowFadePanel(() =>
                {
                    // Save the current state of the game
                    SaveController.ForceSave();

                    // Unload the current level and all the dependencies
                    GameController.Unload(() =>
                    {
                        // Load next level
                        LevelController.ActivateLevel(levelIndex);

                        // Disable fullscreen black overlay
                        gameUI.HideFadePanel(() =>
                        {
                            GameController.OnGameLoaded();
                        });
                    });
                });
            }
        }
    }
}