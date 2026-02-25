using System;
using System.Collections.Generic;
using Project.Domain.Event;
using Project.Domain.Quest;
using Project.Domain.Save;

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
        }

        public void OnDayStart()
        {
            EnsureCurrent();
            _todaySuccessCount = 0;
            Current.TotalFailuresWindow = Math.Max(0, Current.TotalFailuresWindow - 1);

            if (Current.Phase != EventPhase.Dormant && !Current.IsFinished)
            {
                Current.DaysInPhase++;
            }
        }

        public void RegisterQuestResults(IReadOnlyList<QuestResult> results)
        {
            EnsureCurrent();

            if (results == null || results.Count == 0)
            {
                return;
            }

            for (var i = 0; i < results.Count; i++)
            {
                if (results[i].Result == QuestResultType.Fail)
                {
                    Current.TotalFailuresWindow++;
                    continue;
                }

                if (results[i].Result == QuestResultType.Success)
                {
                    _todaySuccessCount++;
                }
            }
        }

        public void TryTriggerOrAdvance(WorldStateData world)
        {
            EnsureCurrent();

            if (Current.IsFinished)
            {
                return;
            }

            if (_todaySuccessCount >= 3)
            {
                LowerOnePhase();
                Current.DaysInPhase = 0;
                return;
            }

            if (Current.Phase == EventPhase.Dormant && Current.TotalFailuresWindow >= 3)
            {
                Current.Phase = EventPhase.Active;
                Current.DaysInPhase = 0;
                return;
            }

            if (Current.Phase == EventPhase.Active && Current.DaysInPhase >= 3)
            {
                Current.Phase = EventPhase.Escalating;
                Current.DaysInPhase = 0;
                return;
            }

            if (Current.Phase == EventPhase.Escalating && Current.DaysInPhase >= 3)
            {
                Current.Phase = EventPhase.Critical;
                Current.DaysInPhase = 0;
                return;
            }

            if (Current.Phase == EventPhase.Critical && Current.DaysInPhase >= 3)
            {
                Current.Phase = EventPhase.Catastrophe;
                Current.DaysInPhase = 0;
                Current.IsFinished = true;
            }
        }

        public WorldDelta GetDailyDelta()
        {
            EnsureCurrent();

            return Current.Phase switch
            {
                EventPhase.Active => new WorldDelta { Stability = -1 },
                EventPhase.Escalating => new WorldDelta { Stability = -2, Casualties = 1 },
                EventPhase.Critical => new WorldDelta { Stability = -3, Casualties = 2, Reputation = -2 },
                _ => new WorldDelta()
            };
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
