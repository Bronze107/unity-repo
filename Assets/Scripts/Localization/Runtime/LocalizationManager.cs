using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Localization
{
    public sealed class LocalizationManager
    {
        private static readonly LocalizationManager InstanceInternal = new LocalizationManager();

        private Dictionary<int, string> _currentTable = new Dictionary<int, string>(0);
        private Dictionary<int, string> _fallbackTable = new Dictionary<int, string>(0);

        public static LocalizationManager Instance => InstanceInternal;

        public event Action LanguageChanged;

        public string CurrentLanguageCode { get; private set; } = "zh-CN";

        public string FallbackLanguageCode { get; private set; } = "zh-CN";

        private LocalizationManager()
        {
        }

        public bool Initialize(string languageCode, string fallbackLanguageCode = "zh-CN")
        {
            FallbackLanguageCode = string.IsNullOrWhiteSpace(fallbackLanguageCode) ? "zh-CN" : fallbackLanguageCode;
            _fallbackTable = LocalizationTableLoader.LoadFromResources(FallbackLanguageCode);

            return SetLanguage(languageCode);
        }

        public bool SetLanguage(string languageCode)
        {
            var nextLanguageCode = string.IsNullOrWhiteSpace(languageCode) ? FallbackLanguageCode : languageCode;
            var nextTable = LocalizationTableLoader.LoadFromResources(nextLanguageCode);
            if (nextTable == null)
            {
                Debug.LogWarning($"Localization table not found for language '{nextLanguageCode}'.");
                return false;
            }

            CurrentLanguageCode = nextLanguageCode;
            _currentTable = nextTable;
            LanguageChanged?.Invoke();
            return true;
        }

        public string Get(int id)
        {
            if (_currentTable.TryGetValue(id, out var value) && !string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (_fallbackTable.TryGetValue(id, out value) && !string.IsNullOrEmpty(value))
            {
                return value;
            }

            return $"#{id}";
        }

        public string Format(int id, params object[] args)
        {
            var format = Get(id);
            if (args == null || args.Length == 0)
            {
                return format;
            }

            return string.Format(format, args);
        }
    }
}
