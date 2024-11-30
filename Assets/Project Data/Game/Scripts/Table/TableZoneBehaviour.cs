using UnityEngine;
using Watermelon.LevelSystem;

namespace Watermelon
{
    public class TableZoneBehaviour : MonoBehaviour, IPurchaseObject
    {
        [Header("Settings")]
        [SerializeField] int tableZoneID;
        public int TableZoneID => tableZoneID;

        [SerializeField] bool isPermanentOpened = false;

        [Space]
        [SerializeField] CurrencyType priceCurrencyType;
        [SerializeField] int priceAmount;

        [Header("Refferences")]
        [SerializeField] TableBehaviour[] tableBehaviours;
        [SerializeField] GameObject[] environmentObjects;

        [Space]
        [SerializeField] GameObject walkableZone;
        [SerializeField] Transform purchaseZoneTransform;

        [Header("Lock")]
        [SerializeField] GameObject lockObject;
        [SerializeField] GameObject solidLockContainer;
        [SerializeField] GameObject purchaseLockContainer;

        [Space]
        [SerializeField] Transform purchaseLeftSideTransform;
        [SerializeField] Transform purchaseRightSideTransform;

        [Space]
        [SerializeField] float scaleZSize = 1.0f;

        [Header("Ad")]
        [SerializeField] bool isAllowedAdOpening = true;

        [Header("Next Table Zone")]
        [SerializeField] TableZoneBehaviour nextTableZoneBehaviour;

        private PurchaseAreaBehaviour purchaseAreaBehaviour;

        public bool IsAllowedAdOpening => isAllowedAdOpening;

        private Zone zone;
        public Zone Zone => zone;

        private bool isOpened;
        public bool IsOpened => isOpened;

        private bool isUnlocked;
        public bool IsUnlocked => isUnlocked;

        private int placedCurrencyAmount;
        public int PlacedCurrencyAmount => placedCurrencyAmount;

        public Transform Transform => transform;

        public int PriceAmount => priceAmount;
        public CurrencyType PriceCurrencyType => priceCurrencyType;

        public TableBehaviour[] TableBehaviours => tableBehaviours;

        public void Initialise(Zone zone)
        {
            this.zone = zone;

            isUnlocked = true;

            if (isOpened || isPermanentOpened)
            {
                lockObject.SetActive(false);

                walkableZone.SetActive(true);

                for (int i = 0; i < tableBehaviours.Length; i++)
                {
                    tableBehaviours[i].Initialise(zone, this, false);
                }

                if (nextTableZoneBehaviour != null)
                {
                    nextTableZoneBehaviour.ActivatePurchase();
                }

                for (int i = 0; i < environmentObjects.Length; i++)
                {
                    environmentObjects[i].SetActive(true);
                }
            }
        }

        public void OnTableOpened(TableBehaviour tableBehaviour)
        {

        }

        public void Unlock()
        {
            isUnlocked = true;

            if (purchaseAreaBehaviour != null)
                purchaseAreaBehaviour.SetBlockState(false);
        }

        public void Lock()
        {
            isUnlocked = false;

            if (purchaseAreaBehaviour != null)
                purchaseAreaBehaviour.SetBlockState(true);
        }

        #region Purchase
        public void PlaceCurrency(int amount)
        {
            if (!isOpened)
            {
                // Request game save
                SaveController.MarkAsSaveIsRequired();

                // Adjust place amount
                placedCurrencyAmount += amount;

                // Redraw purchase area amount
                purchaseAreaBehaviour.SetAmount(priceAmount - placedCurrencyAmount);
            }
        }

        public void OnPurchaseCompleted()
        {
            if (isOpened)
                return;

            isOpened = true;

            if (isAllowedAdOpening)
            {
                UIController.GetPage<UIGame>().DisableZoneAdButton();
            }

            SaveController.MarkAsSaveIsRequired();

            if (nextTableZoneBehaviour != null)
            {
                nextTableZoneBehaviour.ActivatePurchaseWithAnimation();
            }

            // Unlock zone with animation
            purchaseLockContainer.transform.localScale = new Vector3(1, 1, 0.7f);

            purchaseLeftSideTransform.transform.localScale = Vector3.one;
            purchaseRightSideTransform.transform.localScale = new Vector3(-1, 1, 1);


            purchaseLeftSideTransform.DOScaleX(0, 0.14f).SetEasing(Ease.Type.CubicIn);
            purchaseRightSideTransform.DOScaleX(0, 0.14f).SetEasing(Ease.Type.CubicIn);

            purchaseLockContainer.transform.DOScaleZ(0, 0.2f).SetEasing(Ease.Type.QuadOut).OnComplete(delegate
            {
                lockObject.SetActive(false);

                walkableZone.SetActive(true);

                for (int i = 0; i < tableBehaviours.Length; i++)
                {
                    int index = i;

                    Tween.DelayedCall((index + 1) * 0.06f, delegate
                    {
                        tableBehaviours[index].Initialise(zone, this, true);
                    });
                }

                for (int i = 0; i < environmentObjects.Length; i++)
                {
                    Vector3 defaultScale = environmentObjects[i].transform.localScale;

                    environmentObjects[i].SetActive(true);
                    environmentObjects[i].transform.localScale = Vector3.zero;
                    environmentObjects[i].transform.DOScale(defaultScale, 0.4f).SetEasing(Ease.Type.BackOut);
                }

                NavMeshController.RecalculateNavMesh(delegate { });
            });

            purchaseAreaBehaviour.DisableWithAnimation();

            TutorialController.OnTableZoneUnlocked(this);

            if (AudioController.IsVibrationEnabled())
                Vibration.Vibrate(AudioController.Vibrations.shortVibration);
        }

        public void ActivatePurchase()
        {
            if (isOpened)
            {
                return;
            }

            if (isPermanentOpened)
            {
                lockObject.SetActive(false);

                walkableZone.SetActive(true);

                NavMeshController.RecalculateNavMesh(delegate { });

                return;
            }

            // Create purchase zone
            purchaseAreaBehaviour = LevelController.CreateTableAreaPurchaseZone();
            purchaseAreaBehaviour.TransformInitialise(purchaseZoneTransform.position, Quaternion.identity, new Vector2(115, 90), 0.06f, 4.5f);
            purchaseAreaBehaviour.Initialise(this, false, isAllowedAdOpening);
            purchaseAreaBehaviour.Enable();

            // Enable lock object
            lockObject.SetActive(true);

            solidLockContainer.SetActive(false);
            purchaseLockContainer.SetActive(true);

            // Reset transforms
            purchaseLockContainer.transform.localScale = new Vector3(1, 1, 0.7f);
            purchaseLeftSideTransform.transform.localScale = Vector3.one;
            purchaseRightSideTransform.transform.localScale = new Vector3(-1, 1, 1);
        }

        public void ActivatePurchaseWithAnimation()
        {
            if (isOpened)
                return;

            // Create purchase zone
            purchaseAreaBehaviour = LevelController.CreateTableAreaPurchaseZone();
            purchaseAreaBehaviour.TransformInitialise(purchaseZoneTransform.position, Quaternion.identity, new Vector2(115, 90), 0.06f, 4.5f);
            purchaseAreaBehaviour.Initialise(this, false, isAllowedAdOpening);
            purchaseAreaBehaviour.EnableWithAnimation();

            // Enable lock object
            lockObject.SetActive(true);

            solidLockContainer.SetActive(false);
            purchaseLockContainer.SetActive(true);

            // Reset transforms
            purchaseLockContainer.transform.localScale = new Vector3(1, 1, scaleZSize);
            purchaseLeftSideTransform.transform.localScale = Vector3.one;
            purchaseRightSideTransform.transform.localScale = new Vector3(-1, 1, 1);

            // Play container animation
            Tween.DelayedCall(0.2f, delegate
            {
                purchaseLockContainer.transform.DOScaleZ(0.7f, 0.1f);
            });
        }
        #endregion

        #region Load/Save
        public void Load(SaveData save)
        {
            isOpened = save.IsOpened;
            placedCurrencyAmount = save.PlacedCurrencyAmount;

            if (save.TablesSaveData != null)
            {
                for (int i = 0; i < save.TablesSaveData.Length; i++)
                {
                    if (tableBehaviours.IsInRange(i))
                    {
                        tableBehaviours[i].Load(save.TablesSaveData[i]);
                    }
                }
            }
        }

        public SaveData Save()
        {
            TableSaveData[] tablesSaveData = new TableSaveData[tableBehaviours.Length];
            for (int i = 0; i < tablesSaveData.Length; i++)
            {
                tablesSaveData[i] = tableBehaviours[i].Save();
            }

            return new SaveData(tableZoneID, placedCurrencyAmount, isOpened, tablesSaveData);
        }
        #endregion

        public void OnPlayerEntered(PlayerBehavior playerBehavior)
        {

        }

        public void OnPlayerExited(PlayerBehavior playerBehavior)
        {

        }

        [System.Serializable]
        public class SaveData
        {
            [SerializeField] int id;
            public int ID => id;

            [SerializeField] int placedCurrencyAmount;
            public int PlacedCurrencyAmount => placedCurrencyAmount;

            [SerializeField] bool isOpened;
            public bool IsOpened => isOpened;

            [SerializeField] TableSaveData[] tablesSaveData;
            public TableSaveData[] TablesSaveData => tablesSaveData;

            public SaveData(int id, int placedCurrencyAmount, bool isOpened, TableSaveData[] tablesSaveData)
            {
                this.id = id;
                this.placedCurrencyAmount = placedCurrencyAmount;
                this.isOpened = isOpened;
                this.tablesSaveData = tablesSaveData;
            }
        }
    }
}