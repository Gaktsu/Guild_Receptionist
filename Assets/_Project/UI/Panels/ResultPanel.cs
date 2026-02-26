using System.Collections.Generic;
using Project.Core;
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
        [SerializeField] private Transform _resultListRoot;
        [SerializeField] private ResultItemWidget _itemPrefab;
        [SerializeField] private TextMeshProUGUI _worldStateText;

        /// <summary> DayEnd 상태에서 표시. 클릭 시 DayEnd → DayStart (다음 날로 넘김) </summary>
        [SerializeField] private Button _nextDayButton;

        /// <summary> ResolutionPhase 상태에서 표시. 클릭 시 Resolution → DayEnd </summary>
        [SerializeField] private Button _toDayEndButton;


        private DaySystem _daySystem;
        private GameSession _session;
        private DayFlowOrchestrator _orchestrator;

        public void Init(GameSession session)
        {
            _session = session;

            if (_daySystem != null)
            {
                _daySystem.OnStateChanged -= HandleStateChanged;
            }

            _daySystem = _session != null ? _session.DaySystem : null;
            if (_daySystem != null)
            {
                _daySystem.OnStateChanged += HandleStateChanged;
            }

            if (_nextDayButton != null)
            {
                _nextDayButton.onClick.RemoveAllListeners();
                _nextDayButton.onClick.AddListener(HandleNextDayClicked);
            }

            if (_toDayEndButton != null)
            {
                _toDayEndButton.onClick.RemoveAllListeners();
                _toDayEndButton.onClick.AddListener(HandleToDayEndClicked);
            }

            HandleStateChanged(_daySystem != null ? _daySystem.CurrentState : DayState.DayStart);
        }

        public void SetOrchestrator(DayFlowOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
            if (_daySystem != null)
            {
                HandleStateChanged(_daySystem.CurrentState);
            }
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

            if (_toDayEndButton != null)
            {
                _toDayEndButton.onClick.RemoveListener(HandleToDayEndClicked);
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
                UIHelper.DestroyAllChildren(_resultListRoot);
                return;
            }

            if (_resolutionSectionRoot != null)
            {
                _resolutionSectionRoot.SetActive(isResolution);
            }

            if (_toDayEndButton != null)
            {
                _toDayEndButton.gameObject.SetActive(isResolution);
            }

            if (_nextDayButton != null)
            {
                _nextDayButton.gameObject.SetActive(isDayEnd);
            }

            RefreshResults();
            RefreshWorldState();
        }

        /// <summary>
        /// Called externally (e.g. by DayFlowOrchestrator) after resolution results
        /// are ready, so the list rebuilds with up-to-date data regardless of
        /// event subscription order.
        /// </summary>
        public void Refresh()
        {
            RefreshResults();
            RefreshWorldState();
        }

        private void RefreshResults()
        {
            RebuildList(_orchestrator != null ? _orchestrator.LastResults : null);
        }

        private void RebuildList(IReadOnlyList<QuestResult> results)
        {
            UIHelper.DestroyAllChildren(_resultListRoot);

            if (results == null || _itemPrefab == null || _resultListRoot == null)
            {
                return;
            }

            for (var i = 0; i < results.Count; i++)
            {
                var item = Instantiate(_itemPrefab, _resultListRoot);
                item.Init(results[i]);
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

        private void HandleToDayEndClicked()
        {
            if (_daySystem == null)
            {
                return;
            }

            _daySystem.TrySetState(DayState.DayEnd);
        }
    }
}
