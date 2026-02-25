using System.Collections.Generic;
using Project.Domain.Info;
using Project.Systems.Day;
using Project.Systems.Info;
using Project.Systems.Player;
using Project.UI.Widgets;
using UnityEngine;
using TMPro;

namespace Project.UI.Panels
{
    public class InfoPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TextMeshProUGUI _apText;
        [SerializeField] private Transform _contentRoot;
        [SerializeField] private InfoItemWidget _itemWidgetPrefab;

        private readonly List<InfoItemWidget> _spawnedWidgets = new List<InfoItemWidget>();

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
        }

        private void OnDestroy()
        {
            if (_daySystem != null)
            {
                _daySystem.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(DayState state)
        {
            var isInfoPhase = state == DayState.InfoPhase;
            _panelRoot.SetActive(isInfoPhase);

            if (!isInfoPhase)
            {
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
            for (var i = 0; i < _spawnedWidgets.Count; i++)
            {
                if (_spawnedWidgets[i] != null)
                {
                    Destroy(_spawnedWidgets[i].gameObject);
                }
            }

            _spawnedWidgets.Clear();

            for (var i = 0; i < infos.Count; i++)
            {
                var widget = Instantiate(_itemWidgetPrefab, _contentRoot);
                widget.Init(infos[i], _infoSystem, RefreshAll);
                _spawnedWidgets.Add(widget);
            }
        }
    }
}
