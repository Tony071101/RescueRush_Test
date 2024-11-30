using UnityEngine;

namespace Watermelon
{
    public class IceSicknessBehaviour : SicknessBehaviour
    {
        [SerializeField] ParticleSystem sicknessParticleSystem;
        [SerializeField] Material sicknessMaterial;

        [Space]
        [SerializeField] GameObject iceObject;

        private Vector3 defaultPosition;
        private Quaternion defaultRotation;
        private Vector3 defaultScale;

        public override void Activate(AnimalBehaviour animalBehaviour)
        {
            base.Activate(animalBehaviour);

            defaultPosition = iceObject.transform.localPosition;
            defaultRotation = iceObject.transform.localRotation;
            defaultScale = iceObject.transform.localScale;

            // Get skinned mesh renderer and activate material
            animalBehaviour.SetMaterial(sicknessMaterial);

            // Activate particle
            sicknessParticleSystem.Play();

            // Place hat
            iceObject.SetActive(true);
            iceObject.transform.SetParent(animalBehaviour.transform);
            iceObject.transform.localPosition = defaultPosition;
            iceObject.transform.localRotation = defaultRotation;
            iceObject.transform.localScale = defaultScale;

            // Disable movement
            animalBehaviour.BlockMovement();
            animalBehaviour.AnimalAnimator.enabled = false;
        }

        public override void DisableSickness()
        {
            animalBehaviour.ResetMaterial();

            animalBehaviour.AllowMovement();
            animalBehaviour.AnimalAnimator.enabled = true;

            // Stop particle
            sicknessParticleSystem.Stop();

            // Return sickness behaviour to pool
            iceObject.SetActive(false);
            iceObject.transform.SetParent(transform);
            iceObject.transform.localPosition = defaultPosition;
            iceObject.transform.localRotation = defaultRotation;
            iceObject.transform.localScale = defaultScale;

            transform.SetParent(null);
            gameObject.SetActive(false);

            animalBehaviour = null;
        }
    }
}