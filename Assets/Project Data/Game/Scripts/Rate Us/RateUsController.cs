using UnityEngine;

namespace Watermelon
{
    public static class RateUsController
    {
        private static readonly string RATED_ALREADY_ID = "rate_us_rated_already";

        public static bool IsRated()
        {
            return PlayerPrefs.HasKey(RATED_ALREADY_ID);
        }

        public static void Rate()
        {
            PlayerPrefs.SetInt(RATED_ALREADY_ID, 1);
        }

        public static void OpenStore()
        {
#if UNITY_ANDROID
            Application.OpenURL("market://details?id=" + Application.identifier);
#elif UNITY_IPHONE
        Application.OpenURL("itms-apps://itunes.apple.com/app/id" + GameController.Settings.AppleGameID);
#endif
        }
    }
}