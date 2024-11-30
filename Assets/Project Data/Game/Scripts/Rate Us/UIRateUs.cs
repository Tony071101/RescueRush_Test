using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIRateUs : UIPage
    {
        private readonly Vector2 DEFAULT_HAND_POSITION = new Vector2(160, -60);
        private readonly Vector2 ACTIVE_HAND_POSITION = new Vector3(-160, 240);

        [SerializeField] Image fadeImage;
        [SerializeField] RectTransform panel;

        [Space]
        [SerializeField] RectTransform[] starTransforms;

        [Space]
        [SerializeField] RectTransform hand;

        public override void Initialise()
        {

        }

        public override void PlayHideAnimation()
        {
            fadeImage.DOFade(0.0f, 0.5f);

            panel.anchoredPosition = Vector2.zero;
            panel.DOAnchoredPosition(Vector2.down * 2000, 0.5f, unscaledTime: true).SetEasing(Ease.Type.SineIn).OnComplete(() =>
            {

                UIController.OnPageClosed(this);
            });
        }

        public override void PlayShowAnimation()
        {
            // Disable joystick
            UIController.GetPage<UIGame>().Joystick.ResetControl();

            // Reset positions
            panel.anchoredPosition = Vector2.down * 1500;

            for (int i = 0; i < starTransforms.Length; i++)
            {
                starTransforms[i].localScale = Vector3.zero;
            }

            hand.anchoredPosition = DEFAULT_HAND_POSITION;

            // Do animation
            fadeImage.DOFade(0.6f, 0.4f);

            panel.DOAnchoredPosition(Vector2.zero, 0.4f).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {

                StartCoroutine(StarsSpawnCoroutine());

                hand.DOAnchoredPosition(ACTIVE_HAND_POSITION, 0.6f).SetEasing(Ease.Type.QuadOut).OnComplete(delegate
                {
                    UIController.OnPageOpened(this);
                });
            });
        }

        private IEnumerator StarsSpawnCoroutine()
        {
            for (int i = 0; i < starTransforms.Length; i++)
            {
                starTransforms[i].DOScale(Vector3.one, 0.2f).SetEasing(Ease.Type.BackOut);

                yield return new WaitForSeconds(0.1f);
            }
        }

        #region Buttons
        public void RateButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            RateUsController.OpenStore();
            RateUsController.Rate();

            UIController.HidePage<UIRateUs>();
        }

        public void CloseButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            UIController.HidePage<UIRateUs>();
        }

        public void BigCloseButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            UIController.HidePage<UIRateUs>();
        }
        #endregion
    }
}