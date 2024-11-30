using UnityEngine;
using UnityEngine.UI;
using Watermelon.Store;

namespace Watermelon
{
    public class UIGame : UIPage
    {
        [SerializeField] Joystick joystick;

        [Space]
        [SerializeField] CurrenciesUIController currenciesUIController;

        [Space]
        [SerializeField] Button dropButton;
        [SerializeField] Button storeButton;

        [Space]
        [SerializeField] Image globalFadeImage;

        [Header("World Ad Button")]
        [SerializeField] TableZoneAdButtonBehaviour tableZoneAdButtonBehaviour;

        public Joystick Joystick => joystick;

        public CurrenciesUIController CurrenciesUIController => currenciesUIController;

        public override void Initialise()
        {
            joystick.Initialise(canvas);

            currenciesUIController.Initialise(CurrenciesController.Currencies);

            dropButton.onClick.AddListener(() => DropButton());
            storeButton.onClick.AddListener(() => OnStoreButtonClicked());
        }

        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);
        }

        public override void PlayShowAnimation()
        {
            UIController.OnPageOpened(this);

            storeButton.transform.localScale = Vector3.zero;
            storeButton.transform.DOScale(Vector3.one, 0.3f).SetCustomEasing(Ease.GetCustomEasingFunction("Light Bounce"));
        }

        #region Drop Button
        public void DropButton()
        {
            if (AudioController.IsVibrationEnabled())
                Vibration.Vibrate(AudioController.Vibrations.shortVibration);

            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            PlayerBehavior.DropItemsAndAnimals();

            SetDropButtonState(false);
        }

        public void SetDropButtonState(bool state)
        {
            dropButton.gameObject.SetActive(state);
        }
        #endregion

        #region Fade
        public void ShowFadePanel(Tween.TweenCallback onCompleted)
        {
            globalFadeImage.color = new Color(0, 0, 0, 0);
            globalFadeImage.gameObject.SetActive(true);
            globalFadeImage.DOFade(1.0f, 0.4f).OnComplete(onCompleted);
        }

        public void HideFadePanel(Tween.TweenCallback onCompleted)
        {
            globalFadeImage.DOFade(0.0f, 0.4f).OnComplete(() =>
            {
                globalFadeImage.gameObject.SetActive(false);

                onCompleted?.Invoke();
            });
        }
        #endregion

        public void OnStoreButtonClicked()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            StoreController.OpenStore();
        }

        public void ActivateZoneAdButton(IPurchaseObject purchaseObject)
        {
            tableZoneAdButtonBehaviour.Initialise(purchaseObject);
        }

        public void DisableZoneAdButton()
        {
            tableZoneAdButtonBehaviour.Hide();
        }
    }
}