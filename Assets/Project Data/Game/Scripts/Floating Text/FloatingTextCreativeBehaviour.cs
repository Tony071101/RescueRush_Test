#pragma warning disable 0618

using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class FloatingTextCreativeBehaviour : FloatingTextBaseBehaviour
    {
        [SerializeField] TextMeshProUGUI floatingText;

        [Space]
        [SerializeField] Vector3 offset;
        [SerializeField] float time;
        [SerializeField] Ease.Type easing;

        [Space]
        [SerializeField] float scaleTime;
        [SerializeField] AnimationCurve scaleAnimationCurve;

        private Vector3 defaultScale;

        private void Awake()
        {
            defaultScale = transform.localScale;
        }

        public override void Activate(string text, float scale)
        {
            floatingText.text = text;

            transform.localScale = Vector3.zero;
            transform.DOScale(defaultScale * scale, scaleTime).EnableCustomEasing(scaleAnimationCurve);
            transform.DOMove(transform.position + offset, time).SetEasing(easing).OnComplete(delegate
            {
                gameObject.SetActive(false);
            });
        }
    }
}