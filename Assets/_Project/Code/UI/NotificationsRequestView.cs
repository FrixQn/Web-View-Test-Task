using System;
using UnityEngine;
using UnityEngine.UI;

namespace TestProject.UI
{
    public class NotificationsRequestView : MonoBehaviour
    {
        [SerializeField] private Button _accept;
        [SerializeField] private Button _skip;

        public event Action Accepted;
        public event Action Skipped;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            _accept.onClick.AddListener(OnAccepted);
            _skip.onClick.AddListener(OnSkipped);
        }

        private void OnSkipped()
        {
            Skipped?.Invoke();
        }

        private void OnAccepted()
        {
            Accepted?.Invoke();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            _accept.onClick.RemoveAllListeners();
            _skip.onClick.RemoveAllListeners();
        }
    }
}
