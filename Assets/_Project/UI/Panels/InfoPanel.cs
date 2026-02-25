using System.Collections.Generic;
using Project.Core;
using Project.Domain.Info;
using Project.Systems.Day;
using Project.Systems.Info;
using Project.Systems.Player;
using Project.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Project.UI.Panels
{
    public class InfoPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _apText;
        [SerializeField] private Transform _contentRoot;
        [SerializeField] private InfoItemWidget _itemWidgetPrefab;
        [SerializeField] private Button _nextPhaseButton;


        private InfoSystem _infoSystem;
        private ActionPointSystem _apSystem;
        private DaySystem _daySystem;

        public void Init(InfoSystem infoSystem, ActionPointSystem apSystem, DaySystem daySystem)
        {
            _infoSystem = infoSystem;
            _apSystem = apSystem;

            if (_daySystem != null)
            {
                _daySystem.OnStateChanged -= HandleStateChanged;
            }

            _daySystem = daySystem;
            if (_daySystem != null)
            {
                _daySystem.OnStateChanged += HandleStateChanged;
                HandleStateChanged(_daySystem.CurrentState);
            }

            if (_nextPhaseButton != null)
            {
                _nextPhaseButton.onClick.RemoveAllListeners();
                _nextPhaseButton.onClick.AddListener(HandleNextPhaseClicked);
            }
        }

        private void OnDestroy()
        {
            if (_daySystem != null)
            {
                _daySystem.OnStateChanged -= HandleStateChanged;
            }

            if (_nextPhaseButton != null)
            {
                _nextPhaseButton.onClick.RemoveListener(HandleNextPhaseClicked);
            }
        }

        private void HandleStateChanged(DayState state)
        {
            var isInfoPhase = state == DayState.InfoPhase;
            _panelRoot.SetActive(isInfoPhase);

            if (!isInfoPhase)
            {
                ClearSpawnedWidgets();
                return;
            }

            RefreshAll();
        }

        private void RefreshAll()
        {
            RefreshAp();
            RebuildList(_infoSystem.TodayInfos);
        }

        private void RefreshAp()
        {
            _apText.text = $"AP: {_apSystem.CurrentAP}/{_apSystem.MaxAP}";
        }

        private void RebuildList(IReadOnlyList<InfoData> infos)
        {
            UIHelper.DestroyAllChildren(_contentRoot);

            for (var i = 0; i < infos.Count; i++)
            {
                var widget = Instantiate(_itemWidgetPrefab, _contentRoot);
                widget.Init(infos[i], _infoSystem, RefreshAll);
            }
        }

        private void ClearSpawnedWidgets()
        {
            UIHelper.DestroyAllChildren(_contentRoot);
        }

        private void HandleNextPhaseClicked()
        {
            if (_daySystem == null)
            {
                return;
            }

            _daySystem.TrySetState(DayState.QuestDraftPhase);
        }
    }
}
