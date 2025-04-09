using UnityEngine;

namespace TestProject
{
    public class AppService
    {
        public static AppService Default { get; private set; }
        private const string FIRST_LAUNCH_PREFS_KEY = "IsFirstLaunch";
        private const string USER_LINK_PREFS_KEY = "UserLink";

        public readonly bool IsFirstLaunch;
        public readonly ScreenConfiguration ScreenConfiguration;
        public bool IsUserRegistered => PlayerPrefs.HasKey(USER_LINK_PREFS_KEY) && PlayerPrefs.GetString(USER_LINK_PREFS_KEY) != null;
        public string UserLink => System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(PlayerPrefs.GetString(USER_LINK_PREFS_KEY)));

        public AppService()
        {
            IsFirstLaunch = !PlayerPrefs.HasKey(FIRST_LAUNCH_PREFS_KEY);
            ScreenConfiguration = new (ScreenOrientation.AutoRotation, Screen.autorotateToPortrait, Screen.autorotateToPortraitUpsideDown,
                Screen.autorotateToLandscapeLeft, Screen.autorotateToLandscapeRight);

            Application.focusChanged += OnApplicationFocus;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Default = new AppService();
        }

        public void RegisterUser(string url)
        {
            PlayerPrefs.SetString(USER_LINK_PREFS_KEY, System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url)));
            PlayerPrefs.Save();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
                SaveData();
        }

        private void SaveData()
        {
            PlayerPrefs.SetInt(FIRST_LAUNCH_PREFS_KEY, 0);
            PlayerPrefs.Save();
        }
    }
}
