using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

namespace TestProject.Utils
{
    public static class NativeUtils
    {
        public static bool CreateAndroidNotificationChannel(string channelID, string channelDescription, int importance)
        {
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var notificationManager = activity.Call<AndroidJavaObject>("getSystemService", "notification");
                if (new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT") >= 26)
                {
                    var channel = new AndroidJavaObject("android.app.NotificationChannel",
                        channelID,
                        channelDescription,
                        Mathf.Clamp(importance, 0, 4));

                    channel.Call("setDescription", "Main notifications");
                    channel.Call("setShowBadge", true);
                    notificationManager.Call("createNotificationChannel", channel);
                }

                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static Dictionary<string, string> GetAllNotificationDataFromIntent()
        {
            var notificationData = new Dictionary<string, string>();

            try
            {
                using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getIntent");
                using AndroidJavaObject extras = intent.Call<AndroidJavaObject>("getExtras");

                if (extras == null)
                    return notificationData;

                using AndroidJavaObject keySet = extras.Call<AndroidJavaObject>("keySet");
                using AndroidJavaObject iterator = keySet.Call<AndroidJavaObject>("iterator");
                while (iterator.Call<bool>("hasNext"))
                {
                    string key = iterator.Call<string>("next");
                    string value = extras.Call<string>("getString", key);
                    notificationData[key] = value;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return notificationData;
        }

        public static bool CanRequestAndroidNotificationPermission()
        {
            try
            {
                using var version = new AndroidJavaClass("android.os.Build$VERSION");
                if (version.GetStatic<int>("SDK_INT") >= 33)
                {
                    using var context = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    var activity = context.GetStatic<AndroidJavaObject>("currentActivity");
                    var permission = "android.permission.POST_NOTIFICATIONS";

                    var permissionStatus = activity.Call<int>("checkSelfPermission", permission);

                    return permissionStatus != 0;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return false;
        }
    }
}
