using Firebase.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestProject.Utils;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif
#if UNITY_IOS
using Unity.Notifications.iOS;
#endif
using UnityEngine;

namespace TestProject
{
    public class PushNotificationService
    {
        private const string REMIND_PREFS_KEY = "RemindNotifications";
        private const string FCM_CHANNEL_ID = "fcm_default_channel";
        private const string FCM_CHANNEL_DESCRIPTION = "Notifications";
        private const int FCM_CHANNEL_IMPROTANCE = 4;
        private const float REMIND_NOTIFICATIONS_DAYS = 3;

        public IDictionary<string, string> LastIntent { get; set; }

        public async Task<string> GetPushNotificationTokenAsync()
        {
            return await FirebaseMessaging.GetTokenAsync();
        }

        public async Task InitializeAsync()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            NativeUtils.CreateAndroidNotificationChannel(FCM_CHANNEL_ID, FCM_CHANNEL_DESCRIPTION, FCM_CHANNEL_IMPROTANCE);
#endif

            if (NativeUtils.GetAllNotificationDataFromIntent() is Dictionary<string, string> intentData)
            {
                LastIntent = intentData;
            }

            FirebaseMessaging.MessageReceived += OnMessageRecieved;

            await Task.Yield();
        }

        private void OnMessageRecieved(object sender, MessageReceivedEventArgs e) { }

        public bool CanRequestPermissions()
        {
#if UNITY_ANDROID
            return NativeUtils.CanRequestAndroidNotificationPermission() && ShouldRemindRequest() && 
                AndroidNotificationCenter.UserPermissionToPost != PermissionStatus.Allowed;
#elif UNITY_IOS
            return iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus != AuthorizationStatus.Authorized &&
                ShouldRemindRequest();
#endif
        }

        public async Task<bool> RequestPermissionsAsync()
        {
#if UNITY_ANDROID
            if (AndroidNotificationCenter.UserPermissionToPost == PermissionStatus.NotRequested || 
                AndroidNotificationCenter.UserPermissionToPost == PermissionStatus.Denied)
            {
                PermissionRequest request = new ();
                while(request.Status == PermissionStatus.RequestPending)
                {
                    await Task.Yield();
                }

                if (request.Status == PermissionStatus.Denied)
                    RemindRequest(TimeSpan.FromDays(REMIND_NOTIFICATIONS_DAYS));

                return request.Status == PermissionStatus.Allowed;
            }

            
            return AndroidNotificationCenter.UserPermissionToPost == PermissionStatus.Allowed;
#elif UNITY_IOS
            if (iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus == AuthorizationStatus.Authorized ||
                iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus == AuthorizationStatus.Denied) 
            {
                var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
                using var request = new AuthorizationRequest(authorizationOption, true);
                while (!request.IsFinished)
                {
                    await Task.Yield();
                };

                if (!request.Granted)
                    RemindNotificationsRequest();

                return request.Granted;
            }

            return iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus == AuthorizationStatus.Authorized;
#else
            return false;
#endif
        }

        public void RemindNotificationsRequest()
        {
            RemindRequest(TimeSpan.FromDays(REMIND_NOTIFICATIONS_DAYS));
        }

        private void RemindRequest(TimeSpan delay)
        {
            PlayerPrefs.SetString(REMIND_PREFS_KEY, DateTime.Now.Add(delay).ToString());
            PlayerPrefs.Save();
        }

        private bool ShouldRemindRequest()
        {
            if (PlayerPrefs.HasKey(REMIND_PREFS_KEY))
            {
                return DateTime.Parse(PlayerPrefs.GetString(REMIND_PREFS_KEY)) < DateTime.Now;
            }

            return true;
        }
    }
}
