using System;
using System.Collections.Generic;
using Project.Domain.Event;
using Project.Domain.Quest;
using Project.Domain.Save;
using UnityEngine;

namespace Project.Systems.Event
{
    public class EventSystem
    {
        private const string PlagueEventId = "plague";

        private int _todaySuccessCount;

        public ActiveEventData Current { get; private set; }

        public void LoadOrInit(ActiveEventData savedOrNull)
        {
            if (savedOrNull == null)
            {
                Current = CreateDefault();
                Debug.Log($"[Event] LoadOrInit — 세이브 없음, 기본값 생성: Phase={Current.Phase}, Finished={Current.IsFinished}");
                return;
            }

            Current = new ActiveEventData
            {
                EventId = string.IsNullOrWhiteSpace(savedOrNull.EventId) ? PlagueEventId : savedOrNull.EventId,
                Phase = savedOrNull.Phase,
                DaysInPhase = Math.Max(0, savedOrNull.DaysInPhase),
                TotalFailuresWindow = Math.Max(0, savedOrNull.TotalFailuresWindow),
                IsFinished = savedOrNull.Phase == EventPhase.Resolved || savedOrNull.Phase == EventPhase.Catastrophe || savedOrNull.IsFinished
            };
            Debug.Log($"[Event] LoadOrInit — 세이브 로드: Phase={Current.Phase}, DaysInPhase={Current.DaysInPhase}, Failures={Current.TotalFailuresWindow}, Finished={Current.IsFinished}");
        }

        public void OnDayStart()
        {
            EnsureCurrent();
            _todaySuccessCount = 0;
            var prevFailures = Current.TotalFailuresWindow;
            Current.TotalFailuresWindow = Math.Max(0, Current.TotalFailuresWindow - 1);

            if (Current.Phase != EventPhase.Dormant && !Current.IsFinished)
            {
                Current.DaysInPhase++;
            }

            Debug.Log($"[Event] OnDayStart — Phase={Current.Phase}, DaysInPhase={Current.DaysInPhase}, Failures={prevFailures}→{Current.TotalFailuresWindow}, Finished={Current.IsFinished}");
        }

        public void RegisterQuestResults(IReadOnlyList<QuestResult> results)
        {
            EnsureCurrent();

            if (results == null || results.Count == 0)
            {
                Debug.Log("[Event] RegisterQuestResults — 결과 없음, 스킵");
                return;
            }

            var failCount = 0;
            var successCount = 0;
            for (var i = 0; i < results.Count; i++)
            {
                if (results[i].Result == QuestResultType.Fail)
                {
                    Current.TotalFailuresWindow++;
                    failCount++;
                    continue;
                }

                if (results[i].Result == QuestResultType.Success)
                {
                    _todaySuccessCount++;
                    successCount++;
                }
            }

            Debug.Log($"[Event] RegisterQuestResults — 성공={successCount}, 실패={failCount}, TodaySuccess합계={_todaySuccessCount}, Failures누적={Current.TotalFailuresWindow}");
        }

        public void TryTriggerOrAdvance(WorldStateData world)
        {
            EnsureCurrent();
            var prevPhase = Current.Phase;

            if (Current.IsFinished)
            {
                Debug.Log($"[Event] TryTriggerOrAdvance — 이미 종료 상태 ({Current.Phase}), 스킵");
                return;
            }

            if (_todaySuccessCount >= 3)
            {
                LowerOnePhase();
                Current.DaysInPhase = 0;
                Debug.Log($"[Event] TryTriggerOrAdvance — 성공 3회 이상! Phase 하강: {prevPhase} → {Current.Phase}");
                return;
            }

            if (Current.Phase == EventPhase.Dormant && Current.TotalFailuresWindow >= 3)
            {
                Current.Phase = EventPhase.Active;
                Current.DaysInPhase = 0;
                Debug.Log($"[Event] TryTriggerOrAdvance — 실패 누적 {Current.TotalFailuresWindow}회! Phase 상승: {prevPhase} → {Current.Phase}");
                return;
            }

            if (Current.Phase == EventPhase.Active && Current.DaysInPhase >= 3)
            {
                Current.Phase = EventPhase.Escalating;
                Current.DaysInPhase = 0;
                Debug.Log($"[Event] TryTriggerOrAdvance — Active 3일 경과! Phase 상승: {prevPhase} → {Current.Phase}");
                return;
            }

            if (Current.Phase == EventPhase.Escalating && Current.DaysInPhase >= 3)
            {
                Current.Phase = EventPhase.Critical;
                Current.DaysInPhase = 0;
                Debug.Log($"[Event] TryTriggerOrAdvance — Escalating 3일 경과! Phase 상승: {prevPhase} → {Current.Phase}");
                return;
            }

            if (Current.Phase == EventPhase.Critical && Current.DaysInPhase >= 3)
            {
                Current.Phase = EventPhase.Catastrophe;
                Current.DaysInPhase = 0;
                Current.IsFinished = true;
                Debug.Log($"[Event] TryTriggerOrAdvance — Critical 3일 경과! 재앙 발생: {prevPhase} → {Current.Phase}");
                return;
            }

            Debug.Log($"[Event] TryTriggerOrAdvance — 변화 없음. Phase={Current.Phase}, DaysInPhase={Current.DaysInPhase}, Failures={Current.TotalFailuresWindow}, TodaySuccess={_todaySuccessCount}");
        }

        public WorldDelta GetDailyDelta()
        {
            EnsureCurrent();

            var delta = Current.Phase switch
            {
                EventPhase.Active => new WorldDelta { Stability = -1 },
                EventPhase.Escalating => new WorldDelta { Stability = -2, Casualties = 1 },
                EventPhase.Critical => new WorldDelta { Stability = -3, Casualties = 2, Reputation = -2 },
                _ => new WorldDelta()
            };

            Debug.Log($"[Event] GetDailyDelta — Phase={Current.Phase}, 안정={delta.Stability}, 사상자={delta.Casualties}, 평판={delta.Reputation}");
            return delta;
        }

        private void LowerOnePhase()
        {
            Current.Phase = Current.Phase switch
            {
                EventPhase.Critical => EventPhase.Escalating,
                EventPhase.Escalating => EventPhase.Active,
                EventPhase.Active => EventPhase.Dormant,
                _ => Current.Phase
            };
        }

        private void EnsureCurrent()
        {
            if (Current == null)
            {
                Current = CreateDefault();
            }
        }

        private static ActiveEventData CreateDefault()
        {
            return new ActiveEventData
            {
                EventId = PlagueEventId,
                Phase = EventPhase.Dormant,
                DaysInPhase = 0,
                TotalFailuresWindow = 0,
                IsFinished = false
            };
        }
    }
}

namespace Project.Domain.Event
{
    public enum EventPhase
    {
        Dormant,
        Active,
        Escalating,
        Critical,
        Resolved,
        Catastrophe
    }

    [Serializable]
    public class ActiveEventData
    {
        public string EventId = "plague";
        public EventPhase Phase = EventPhase.Dormant;
        public int DaysInPhase;
        public int TotalFailuresWindow;
        public bool IsFinished;
    }
}
