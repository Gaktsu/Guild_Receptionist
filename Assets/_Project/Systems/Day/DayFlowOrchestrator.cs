using System;
using System.Collections.Generic;
using Project.Domain.Info;
using Project.Domain.Quest;
using Project.Systems.Game;
using Project.Systems.Info;
using Project.Systems.Player;
using Project.Systems.Quest;
using UnityEngine;

namespace Project.Systems.Day
{
    public class DayFlowOrchestrator : MonoBehaviour
    {
        private GameSession _session;
        private DaySystem _daySystem;
        private InfoSystem _infoSystem;
        private ActionPointSystem _apSystem;
        private QuestSystem _questSystem;

        public void Init(
            GameSession session,
            DaySystem daySystem,
            InfoSystem infoSystem,
            ActionPointSystem apSystem,
            QuestSystem questSystem)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _daySystem = daySystem ?? throw new ArgumentNullException(nameof(daySystem));
            _infoSystem = infoSystem ?? throw new ArgumentNullException(nameof(infoSystem));
            _apSystem = apSystem ?? throw new ArgumentNullException(nameof(apSystem));
            _questSystem = questSystem ?? throw new ArgumentNullException(nameof(questSystem));

            _daySystem.OnStateChanged -= HandleStateChanged;
            _daySystem.OnStateChanged += HandleStateChanged;

            HandleStateChanged(_daySystem.CurrentState);
        }

        private void OnDestroy()
        {
            if (_daySystem != null)
            {
                _daySystem.OnStateChanged -= HandleStateChanged;
            }
        }

        [ContextMenu("Next State")]
        public void NextState()
        {
            if (_daySystem == null)
            {
                return;
            }

            _daySystem.TrySetState(GetNextState(_daySystem.CurrentState));
        }

        [ContextMenu("Jump To DayStart")]
        public void JumpToDayStart() => JumpTo(DayState.DayStart);

        [ContextMenu("Jump To InfoPhase")]
        public void JumpToInfoPhase() => JumpTo(DayState.InfoPhase);

        [ContextMenu("Jump To QuestDraftPhase")]
        public void JumpToQuestDraftPhase() => JumpTo(DayState.QuestDraftPhase);

        [ContextMenu("Jump To SubmissionPhase")]
        public void JumpToSubmissionPhase() => JumpTo(DayState.SubmissionPhase);

        [ContextMenu("Jump To ResolutionPhase")]
        public void JumpToResolutionPhase() => JumpTo(DayState.ResolutionPhase);

        [ContextMenu("Jump To DayEnd")]
        public void JumpToDayEnd() => JumpTo(DayState.DayEnd);

        public void JumpTo(DayState state)
        {
            if (_daySystem == null)
            {
                return;
            }

            var current = _daySystem.CurrentState;
            _daySystem.ForceSetState(state);
            if (current == state)
            {
                HandleStateChanged(state);
            }
        }

        private void HandleStateChanged(DayState state)
        {
            switch (state)
            {
                case DayState.DayStart:
                    _infoSystem.StartDay(_session.CreateDayRng(), _session.CurrentDay);
                    _daySystem.TrySetState(DayState.InfoPhase);
                    break;
                case DayState.InfoPhase:
                    break;
                case DayState.QuestDraftPhase:
                    break;
                case DayState.SubmissionPhase:
                    break;
                case DayState.ResolutionPhase:
                    ResolvePhase();
                    break;
                case DayState.DayEnd:
                    _session.NextDay();
                    break;
            }
        }

        private void ResolvePhase()
        {
            var results = _questSystem.ResolveSubmitted(
                _session.CurrentDay,
                _session.CreateDayRng(),
                GetInfoCredibility);

            for (var i = 0; i < results.Count; i++)
            {
                _session.ApplyWorldDelta(results[i].Delta);
            }

            _session.Save();
        }

        private int GetInfoCredibility(string infoId)
        {
            IReadOnlyList<InfoData> infos = _infoSystem.TodayInfos;
            for (var i = 0; i < infos.Count; i++)
            {
                if (string.Equals(infos[i].Id, infoId, StringComparison.Ordinal))
                {
                    return infos[i].Credibility;
                }
            }

            return 50;
        }

        private static DayState GetNextState(DayState current)
        {
            return current switch
            {
                DayState.DayStart => DayState.InfoPhase,
                DayState.InfoPhase => DayState.QuestDraftPhase,
                DayState.QuestDraftPhase => DayState.SubmissionPhase,
                DayState.SubmissionPhase => DayState.ResolutionPhase,
                DayState.ResolutionPhase => DayState.DayEnd,
                DayState.DayEnd => DayState.DayStart,
                _ => DayState.DayStart
            };
        }
    }
}
