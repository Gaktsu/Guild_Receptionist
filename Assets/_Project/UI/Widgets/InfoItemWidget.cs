using Project.Domain.Info;
using Project.Systems.Info;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Project.UI.Widgets
{
    public class InfoItemWidget : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _regionText;
        [SerializeField] private TextMeshProUGUI _credibilityText;
        [SerializeField] private Button _investigateButton;
        [SerializeField] private Button _archiveButton;
        [SerializeField] private Button _discardButton;

        private InfoData _info;
        private InfoSystem _infoSystem;
        private System.Action _onChanged;

        public void Init(InfoData info, InfoSystem infoSystem, System.Action onChanged)
        {
            _info = info;
            _infoSystem = infoSystem;
            _onChanged = onChanged;

            _investigateButton.onClick.RemoveAllListeners();
            _archiveButton.onClick.RemoveAllListeners();
            _discardButton.onClick.RemoveAllListeners();

            _investigateButton.onClick.AddListener(OnClickInvestigate);
            _archiveButton.onClick.AddListener(OnClickArchive);
            _discardButton.onClick.AddListener(OnClickDiscard);

            Refresh();
        }

        public void Refresh()
        {
            if (_info == null)
            {
                return;
            }

            _titleText.text = _info.Title;
            _regionText.text = _info.Region;
            _credibilityText.text = $"Credibility: {_info.Credibility}";

            var isLocked = _info.IsArchived || _info.IsDiscarded;
            _investigateButton.interactable = !isLocked && _info.Credibility < 100;
            _archiveButton.interactable = !isLocked;
            _discardButton.interactable = !isLocked;
        }

        private void OnClickInvestigate()
        {
            if (_infoSystem == null || _info == null)
            {
                return;
            }

            if (_infoSystem.TryInvestigate(_info.Id))
            {
                _onChanged?.Invoke();
            }

            Refresh();
        }

        private void OnClickArchive()
        {
            if (_infoSystem == null || _info == null)
            {
                return;
            }

            if (_infoSystem.TryArchive(_info.Id))
            {
                _onChanged?.Invoke();
            }

            Refresh();
        }

        private void OnClickDiscard()
        {
            if (_infoSystem == null || _info == null)
            {
                return;
            }

            if (_infoSystem.TryDiscard(_info.Id))
            {
                _onChanged?.Invoke();
            }

            Refresh();
        }
    }
}
