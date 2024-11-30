using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class ZoneSave
    {
        [SerializeField] int id;
        public int ID => id;

        [SerializeField] int placedCurrencyAmount;
        public int PlacedCurrencyAmount => placedCurrencyAmount;

        [SerializeField] bool isOpened;
        public bool IsOpened => isOpened;

        [SerializeField] TableZoneBehaviour.SaveData[] tableZones;
        public TableZoneBehaviour.SaveData[] TableZones => tableZones;

        [SerializeField] NurseZoneSettings.SaveData nurseSettings;
        public NurseZoneSettings.SaveData NurseSettings => nurseSettings;

        public ZoneSave(int id, int placedCurrencyAmount, bool isOpened, TableZoneBehaviour.SaveData[] tableZones, NurseZoneSettings.SaveData nurseSettings)
        {
            this.id = id;
            this.placedCurrencyAmount = placedCurrencyAmount;
            this.isOpened = isOpened;
            this.tableZones = tableZones;
            this.nurseSettings = nurseSettings;
        }
    }
}