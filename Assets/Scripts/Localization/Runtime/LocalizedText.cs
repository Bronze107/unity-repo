using UnityEngine;
using UnityEngine.UI;

namespace Game.Localization
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Text))]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField] private int keyId;

        private Text _text;
        private string _lastValue;

        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        private void OnEnable()
        {
            LocalizationManager.Instance.LanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            LocalizationManager.Instance.LanguageChanged -= Refresh;
        }

        public void SetKey(int id)
        {
            keyId = id;
            Refresh();
        }

        public void Refresh()
        {
            if (_text == null)
            {
                _text = GetComponent<Text>();
            }

            var value = LocalizationManager.Instance.Get(keyId);
            if (_lastValue == value)
            {
                return;
            }

            _lastValue = value;
            _text.text = value;
        }
    }
}
