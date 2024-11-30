using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class MoneyStackBehaviour : MonoBehaviour
    {
        private readonly Vector3 DEFAULT_MONEY_SIZE = new Vector3(1, 1.5f, 1);
        private const int MONEY_FOR_STACK = 5;

        [SerializeField] GameObject moneyPrefab;
        [SerializeField] Transform[] stackElementsHolders;

        private Pool moneyPool;

        private Zone zone;

        private int collectedMoney = 0;
        public int CollectedMoney => collectedMoney;

        private int activeMoneyElementsAmount = 0;
        private List<Transform> activeMoneyElements = new List<Transform>();

        private WaitForSeconds spawnDelay;

        private bool isActive = true;
        public bool IsActive => isActive;

        private bool isPlayerInZone = false;
        private bool moneyAnimationIsPlaying;

        public void Initialise(Zone zone)
        {
            this.zone = zone;

            // Create delays
            spawnDelay = new WaitForSeconds(0.05f);

            // Create money pool
            moneyPool = new Pool(new PoolSettings(moneyPrefab.name, moneyPrefab, 3, true, transform));
        }

        private void Update()
        {
            if (isPlayerInZone && !moneyAnimationIsPlaying)
                PickMoney();
        }

        public void AddMoney(int amount)
        {
            moneyAnimationIsPlaying = true;

            collectedMoney += amount;

            StartCoroutine(SpawnMoneyElements());
        }

        private IEnumerator SpawnMoneyElements()
        {
            int tempActiveElements = collectedMoney / MONEY_FOR_STACK;
            int amountDiff = tempActiveElements - activeMoneyElementsAmount;

            if (amountDiff > 0)
            {
                for (int i = 0; i < amountDiff; i++)
                {
                    // Check if next stack object exist
                    if (activeMoneyElementsAmount < stackElementsHolders.Length)
                    {
                        // Get object from pool and initialise transform
                        GameObject tempMoneyObject = moneyPool.GetPooledObject();
                        tempMoneyObject.transform.ResetLocal();
                        tempMoneyObject.transform.position = stackElementsHolders[activeMoneyElementsAmount].position;
                        tempMoneyObject.transform.rotation = stackElementsHolders[activeMoneyElementsAmount].rotation;
                        tempMoneyObject.transform.localScale = Vector3.zero;
                        tempMoneyObject.SetActive(true);

                        // Play scale animation
                        tempMoneyObject.transform.DOScale(DEFAULT_MONEY_SIZE, 0.3f).SetEasing(Ease.Type.BackOut);

                        // Add element to list
                        activeMoneyElementsAmount++;
                        activeMoneyElements.Add(tempMoneyObject.transform);

                        yield return spawnDelay;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            moneyAnimationIsPlaying = false;
        }

        private void DisableMoneyElements()
        {
            // Cache variables
            int moneyElementsAmount = activeMoneyElementsAmount;
            List<Transform> moneyElementsTransform = activeMoneyElements;

            // Reset global variables
            activeMoneyElementsAmount = 0;
            activeMoneyElements = new List<Transform>();

            PlayerBehavior playerBehavior = PlayerBehavior.GetBehavior();

            for (int i = moneyElementsAmount - 1; i >= 0; i--)
            {
                // Cache transform element
                Transform elementTransform = moneyElementsTransform[i];

                float time = Random.Range(0.6f, 1.4f);

                // Play scale animation
                elementTransform.DOScale(0, time).SetEasing(Ease.Type.SineIn);
                elementTransform.DOBezierFollow(playerBehavior.transform, Random.Range(5, 10), Random.Range(-1, 1), Random.Range(0.4f, 1.0f)).SetEasing(Ease.Type.SineIn).OnComplete(delegate
                {
                    // Disable object and return to pool
                    elementTransform.gameObject.SetActive(false);

                    playerBehavior.PlayMoneyPickUpParticle();
                });
            }
        }

        private void PickMoney()
        {
            if (collectedMoney > 0)
            {
                AudioController.PlaySound(AudioController.Sounds.moneyPickUpSound);

                CurrenciesController.Add(CurrencyType.Money, collectedMoney);

                FloatingTextController.SpawnFloatingText("Money", "+" + collectedMoney, transform.position + new Vector3(0, 6, 2), Quaternion.Euler(45, 0, 0), 1.0f);

                collectedMoney = 0;

                DisableMoneyElements();

                zone.OnMoneyPicked();

                TutorialController.OnMoneyPicked();

                Tween.DelayedCall(1.0f, delegate
                {
                    AdsManager.ShowInterstitial(delegate
                    {
                        // Interstitial is shown
                    });
                });
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive)
                return;

            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                isPlayerInZone = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PhysicsHelper.TAG_PLAYER))
            {
                isPlayerInZone = false;
            }
        }


        public void SetActiveState(bool state)
        {
            isActive = state;
        }
    }
}