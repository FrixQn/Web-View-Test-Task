using AppsFlyerSDK;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Firebase;
using TestProject.UI;
using System.Text;
using System.Net.Http;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using TestProject.Utils;
using UnityEngine.SceneManagement;
#if UNITY_IOS && !UNITY_EDITOR
using Unity.Advertisement.IosSupport;
#endif

namespace TestProject
{
    public class Bootrapper : MonoBehaviour, IAppsFlyerConversionData
    {
        private readonly PushNotificationService _notificationService = new ();
        [SerializeField] private AppSettings _settings;
        [SerializeField] private NotificationsRequestView _requestView;
        [SerializeField] private UniWebView _view;
        private bool _isConversionDataRecieved;
        private bool _isPermissionsResponded;
        private string _conversionData;

        private async void Start()
        {
            Application.targetFrameRate = 120;
            SetupFreeOrientation();
            await InitializeAsync();
        }

        private void SetupFreeOrientation()
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }

        private async Task InitializeAsync()
        {
            if (AppService.Default.IsFirstLaunch || AppService.Default.IsUserRegistered)
            {
                var conversion = await InitializeAppsFlyerAsync();
                await _notificationService.InitializeAsync();
                var pushToken = await _notificationService.GetPushNotificationTokenAsync();
                JObject json = JObject.Parse(conversion);
                JObject append = new()
                {
                    { "af_id", AppsFlyer.getAppsFlyerId()},
                    { "bundle_id", Application.identifier},
                    { "os", GetOSName()},
                    { "store_id", GetStoreID() },
                    { "locale", System.Globalization.CultureInfo.CurrentCulture.ToString() },
                    { "push_token", pushToken},
                    { "firebase_project_id", GetFirebaseProjectNumber()}
                };

                json.Merge(append);

                var client = new HttpClient();

                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.PostAsync(_settings.Link, content);

                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                string pattern = @"<meta\s[^>]*?http-equiv\s*=\s*[""']?\w+[""']?[^>]*?content\s*=\s*[""'](?:[^""'>]*?\bURL=)?([^""'>]+)[""'][^>]*>";

                string responseLink = null;
                foreach (Match m in Regex.Matches(responseString, pattern, RegexOptions.IgnoreCase).Cast<Match>())
                {
                    responseLink = m.Groups[1].Value
                        .TrimStart(';')
                        .Trim();
                    break;
                }

                if (responseLink == null)
                {
                    if (AppService.Default.IsUserRegistered)
                    {
                        responseLink = AppService.Default.UserLink;
                    }
                    else
                    {
                        LoadGame();
                        return;
                    }
                }

                if (_notificationService.CanRequestPermissions())
                {
                    _requestView.Show();
                    _requestView.Accepted += PermissionsGranted;
                    _requestView.Skipped += PermissionsSkipped;

                    while (!_isPermissionsResponded)
                    {
                        await Task.Yield();
                    }
                }

                if (_notificationService.LastIntent != null)
                {
                    if (_notificationService.LastIntent.TryGetValue("url", out string newLink) && newLink != null && newLink != string.Empty)
                        responseLink = newLink;
                }

                _view.gameObject.SetActive(true);
                UniWebView.SetAllowInlinePlay(true);
                _view.SetAcceptThirdPartyCookies(true);
                _view.RegisterShouldHandleRequest(LinksPreprocessor);
                _view.SetUserAgent("Mozilla/5.0 (Linux; Android 10; SM-G975F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Mobile Safari/537.36");
                _view.OnLoadingErrorReceived += OnError;
                _view.OnMessageReceived += OnMessageRecieved;
                _view.EmbeddedToolbar.Show();
                _view.EmbeddedToolbar.ShowNavigationButtons();
                _view.EmbeddedToolbar.SetDoneButtonText(string.Empty);
                _view.Show();
                _view.SetShowSpinnerWhileLoading(true);
                _view.Load(responseLink);
                AppService.Default.RegisterUser(responseLink);
            }
            else
            {
                LoadGame();
            }
        }

        private async Task<string> InitializeAppsFlyerAsync()
        {
            AppsFlyer.setIsDebug(_settings.IsDebugMode);

#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);

            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus()
                == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
            }
#endif

#if UNITY_ANDROID
            AppsFlyer.initSDK(_settings.AFDevKey, null, this);
#elif UNITY_IOS
            AppsFlyer.initSDK(_settings.AFDevKey, _settings.AFIOSAppID, this);
#endif
            AppsFlyer.startSDK();

            while (!_isConversionDataRecieved)
            {
                await Task.Yield();
            }

            return _conversionData;
        }

        private void LoadGame()
        {
            Screen.orientation = AppService.Default.ScreenConfiguration.Orientation;
            Screen.autorotateToPortrait = AppService.Default.ScreenConfiguration.AutoRotateToPortrait;
            Screen.autorotateToPortraitUpsideDown = AppService.Default.ScreenConfiguration.AutoRotateToPortraitUpsideDown;
            Screen.autorotateToLandscapeLeft = AppService.Default.ScreenConfiguration.AutoRotateToLandscapeLeft;
            Screen.autorotateToLandscapeRight = AppService.Default.ScreenConfiguration.AutoRotateToLandscapeRight;
            SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        }

        private bool LinksPreprocessor(UniWebViewChannelMethodHandleRequest request)
        {
            if (DeepLinkUtils.IsDeepLink(request.Url))
            {
                DeepLinkUtils.OpenDeepLink(request.Url);
                return false;
            }

            return true;
        }

        private void OnMessageRecieved(UniWebView webView, UniWebViewMessage message)
        {
            if (DeepLinkUtils.IsDeepLink(message.Path))
            {
                DeepLinkUtils.OpenDeepLink(message.Path);
            }
        }

        private void OnError(UniWebView webView, int errorCode, string errorMessage, UniWebViewNativeResultPayload payload)
        {
            if (errorCode == -9)
            {
                webView.Load(webView.Url);
            }
        }

        async void PermissionsGranted()
        {
            await _notificationService.RequestPermissionsAsync();
            _requestView.Hide();
            _isPermissionsResponded = true;
            _requestView.Accepted -= PermissionsGranted;
            _requestView.Skipped -= PermissionsSkipped;
        }
        
        void PermissionsSkipped()
        {
            _notificationService.RemindNotificationsRequest();
            _requestView.Hide();
            _isPermissionsResponded = true;
            _requestView.Accepted -= PermissionsGranted;
            _requestView.Skipped -= PermissionsSkipped;
        }

        private string GetOSName()
        {
            if (Application.platform == RuntimePlatform.Android)
                return "Android";
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                return "iOS";

            return "Undefined";
        }

        private string GetStoreID()
        {
            if (Application.platform == RuntimePlatform.Android)
                return Application.identifier;
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                return $"id{_settings.AFIOSAppID}";

            return "Undefined";
        }

        private string GetFirebaseProjectNumber()
        {
            return FirebaseApp.DefaultInstance.Options.AppId.Split(":").GetValue(1).ToString();
        }

        public void onConversionDataFail(string error)
        {
            _conversionData = null;
            _isConversionDataRecieved = true;
        }

        public void onConversionDataSuccess(string conversionData)
        {
            _conversionData = conversionData;
            _isConversionDataRecieved = true;
        }
        public void onAppOpenAttribution(string attributionData) { }
        public void onAppOpenAttributionFailure(string error) { }

    }
}
