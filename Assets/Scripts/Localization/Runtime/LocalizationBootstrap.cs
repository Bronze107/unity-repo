using UnityEngine;

namespace Game.Localization
{
    public sealed class LocalizationBootstrap : MonoBehaviour
    {
        [SerializeField] private string defaultLanguage = "zh-CN";
        [SerializeField] private string fallbackLanguage = "zh-CN";

        private void Awake()
        {
            LocalizationManager.Instance.Initialize(defaultLanguage, fallbackLanguage);
        }
    }
}
