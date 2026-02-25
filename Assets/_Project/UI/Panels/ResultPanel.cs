using System.Collections.Generic;
using Project.Domain.Quest;
using Project.Systems.Day;
using Project.Systems.Game;
using Project.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI.Panels
{
    public class ResultPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private GameObject _resolutionSectionRoot;
        [SerializeField] private GameObject _dayEndSectionRoot;
        [SerializeField] private Transform _resultListRoot;
        [SerializeField] private ResultItemWidget _itemPrefab;
        [SerializeField] private TextMeshProUGUI _worldStateText;
        [SerializeField] private Button _nextDayButton;

        private readonly List<ResultItemWidget> _spawnedItems = new List<ResultItemWidget>();

        private DaySystem _daySystem;
        private GameSession _session;
        private DayFlowOrchestrator _orchestrator;

        public void Init(DaySystem daySystem, GameSession session, DayFlowOrchestrator orchestrator)
        {
            _session = session;
            _orchestrator = orchestrator;

            if (_daySystem != null)
            {
                _daySystem.OnStateChanged -= HandleStateChanged;
            }

            _daySystem = daySystem;
            if (_daySystem != null)
            {
                _daySystem.OnStateChanged += HandleStateChanged;
            }

            if (_nextDayButton != null)
            {
                _nextDayButton.onClick.RemoveAllListeners();
                _nextDayButton.onClick.AddListener(HandleNextDayClicked);
            }

            HandleStateChanged(_daySystem != null ? _daySystem.CurrentState : DayState.DayStart);
        }

        private void Start()
        {
            if (_daySystem != null)
            {
                return;
            }

            var bootstrapper = GameBootstrapper.Instance;
            if (bootstrapper == null)
            {
                return;
            }

            var orchestrator = bootstrapper.GetComponent<DayFlowOrchestrator>();
            if (orchestrator == null)
            {
                return;
            }

            Init(bootstrapper.Session.DaySystem, bootstrapper.Session, orchestrator);
        }

        private void OnDestroy()
        {
            if (_daySystem != null)
            {
                _daySystem.OnStateChanged -= HandleStateChanged;
            }

            if (_nextDayButton != null)
            {
                _nextDayButton.onClick.RemoveListener(HandleNextDayClicked);
            }
        }

        private void HandleStateChanged(DayState state)
        {
            var isResolution = state == DayState.ResolutionPhase;
            var isDayEnd = state == DayState.DayEnd;
            var isVisible = isResolution || isDayEnd;

            if (_panelRoot != null)
            {
                _panelRoot.SetActive(isVisible);
            }

            if (!isVisible)
            {
                return;
            }

            if (_resolutionSectionRoot != null)
            {
                _resolutionSectionRoot.SetActive(isResolution);
            }

            if (_dayEndSectionRoot != null)
            {
                _dayEndSectionRoot.SetActive(isDayEnd);
            }

            RefreshResults();
            RefreshWorldState();
        }

        private void RefreshResults()
        {
            RebuildList(_orchestrator != null ? _orchestrator.LastResults : null);
        }

        private void RebuildList(IReadOnlyList<QuestResult> results)
        {
            for (var i = 0; i < _spawnedItems.Count; i++)
            {
                if (_spawnedItems[i] != null)
                {
                    Destroy(_spawnedItems[i].gameObject);
                }
            }

            _spawnedItems.Clear();

            if (results == null || _itemPrefab == null || _resultListRoot == null)
            {
                return;
            }

            for (var i = 0; i < results.Count; i++)
            {
                var item = Instantiate(_itemPrefab, _resultListRoot);
                item.Init(results[i]);
                _spawnedItems.Add(item);
            }
        }

        private void RefreshWorldState()
        {
            if (_session == null || _worldStateText == null)
            {
                return;
            }

            var state = _session.WorldState;
            _worldStateText.text =
                $"World | Rep {state.Reputation} | Stab {state.Stability} | Bud {state.Budget} | Inf {state.Influence} | Cas {state.Casualties}";
        }

        private void HandleNextDayClicked()
        {
            if (_daySystem == null)
            {
                return;
            }

            _daySystem.TrySetState(DayState.DayStart);
        }
    }
}
