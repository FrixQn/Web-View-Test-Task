using UnityEngine;

namespace TestProject
{
    [CreateAssetMenu(fileName = nameof(AppSettings), menuName = "Project/" + nameof(AppSettings))]
    public class AppSettings : ScriptableObject
    {
        [field: SerializeField] public string AFDevKey { get; private set; }
        [field: SerializeField] public string AFIOSAppID { get; private set; }
        [field: SerializeField] public string Link { get; private set; }
        [field: SerializeField] public bool IsDebugMode { get; private set; }
    }
}
