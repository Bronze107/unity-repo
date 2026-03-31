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

        public string FormatParams(int id, params object[] args)
        {
            var format = Get(id);
            if (args == null || args.Length == 0)
            {
                return format;
            }

            return string.Format(format, args);
        }

        public string Format<T1>(int id, T1 arg1)
        {
            return string.Format(Get(id), arg1);
        }

        public string Format<T1, T2>(int id, T1 arg1, T2 arg2)
        {
            return string.Format(Get(id), arg1, arg2);
        }

        public string Format<T1, T2, T3>(int id, T1 arg1, T2 arg2, T3 arg3)
        {
            return string.Format(Get(id), arg1, arg2, arg3);
        }

        public string FastFormat<T1>(int id, T1 arg1)
        {
            return LocalizationFastFormatter.Format(Get(id), arg1);
        }

        public string FastFormat<T1, T2>(int id, T1 arg1, T2 arg2)
        {
            return LocalizationFastFormatter.Format(Get(id), arg1, arg2);
        }

        public string FastFormat<T1, T2, T3>(int id, T1 arg1, T2 arg2, T3 arg3)
        {
            return LocalizationFastFormatter.Format(Get(id), arg1, arg2, arg3);
        }

        public string FastFormat(int id, int arg1)
        {
            return LocalizationFastFormatter.Format(Get(id), arg1);
        }

        public string FastFormat(int id, int arg1, int arg2)
        {
            return LocalizationFastFormatter.Format(Get(id), arg1, arg2);
        }

        public string FastFormat(int id, int arg1, int arg2, int arg3)
        {
            return LocalizationFastFormatter.Format(Get(id), arg1, arg2, arg3);
        }

        public string FastFormat(int id, float arg1)
        {
            return LocalizationFastFormatter.Format(Get(id), arg1);
        }

        public string FastFormat(int id, float arg1, float arg2)
        {
            return LocalizationFastFormatter.Format(Get(id), arg1, arg2);
        }

        public string FastFormat(int id, string arg1)
        {
            return LocalizationFastFormatter.Format(Get(id), arg1);
        }

        public string FastFormat(int id, string arg1, string arg2)
        {
            return LocalizationFastFormatter.Format(Get(id), arg1, arg2);
        }
    }
}
