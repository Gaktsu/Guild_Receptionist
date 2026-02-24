using System;
using System.Collections.Generic;
using Project.Core.Random;
using Project.Domain.Quest;
using Project.Domain.Save;

namespace Project.Systems.Quest
{
    public class QuestSystem
    {
        private readonly List<QuestDraft> _drafts = new List<QuestDraft>();
        private readonly List<QuestIssued> _submittedToday = new List<QuestIssued>();

        private int _draftSequence;
        private int _submissionDay = -1;

        public IReadOnlyList<QuestDraft> Drafts => _drafts;
        public IReadOnlyList<QuestIssued> SubmittedToday => _submittedToday;
        public int MaxSubmissionsPerDay => 4;

        public QuestDraft CreateDraft(
            QuestTemplateType type,
            List<string> sourceInfoIds,
            int risk,
            int reward,
            int deadlineDays)
        {
            var clampedRisk = Clamp(risk, 1, 5);
            var clampedDeadline = Clamp(deadlineDays, 1, 5);

            var draft = new QuestDraft
            {
                Id = CreateDraftId(),
                Type = type,
                SourceInfoIds = sourceInfoIds != null ? new List<string>(sourceInfoIds) : new List<string>(),
                Risk = clampedRisk,
                Reward = reward,
                DeadlineDays = clampedDeadline
            };

            _drafts.Add(draft);
            return draft;
        }

        public bool TrySubmit(string draftId, int currentDay)
        {
            if (string.IsNullOrWhiteSpace(draftId) || currentDay <= 0)
            {
                return false;
            }

            ResetSubmissionBucketIfNeeded(currentDay);

            if (_submittedToday.Count >= MaxSubmissionsPerDay)
            {
                return false;
            }

            var draft = FindDraft(draftId);
            if (draft == null)
            {
                return false;
            }

            for (var i = 0; i < _submittedToday.Count; i++)
            {
                if (string.Equals(_submittedToday[i].Draft.Id, draftId, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            _submittedToday.Add(new QuestIssued
            {
                Draft = draft,
                SubmittedDay = currentDay
            });

            return true;
        }

        public List<QuestResult> ResolveSubmitted(int currentDay, IRandomService rng, Func<string, int> getInfoCredibility)
        {
            if (rng == null)
            {
                throw new ArgumentNullException(nameof(rng));
            }

            if (getInfoCredibility == null)
            {
                throw new ArgumentNullException(nameof(getInfoCredibility));
            }

            var results = new List<QuestResult>();

            for (var i = 0; i < _submittedToday.Count; i++)
            {
                var issued = _submittedToday[i];
                if (issued.SubmittedDay != currentDay || issued.Draft == null)
                {
                    continue;
                }

                var credibilityAvg = GetCredibilityAverage(issued.Draft.SourceInfoIds, getInfoCredibility);
                var resolveOutcome = QuestResolver.Resolve(issued.Draft, credibilityAvg, rng);

                results.Add(new QuestResult
                {
                    QuestId = issued.Draft.Id,
                    Result = resolveOutcome.ResultType,
                    FinalSuccessChance = resolveOutcome.FinalChance,
                    TopReasons = BuildTopReasons(issued.Draft, credibilityAvg),
                    Delta = BuildWorldDelta(issued.Draft, resolveOutcome.ResultType)
                });
            }

            _submittedToday.Clear();
            _submissionDay = -1;
            return results;
        }

        public static WorldStateData ApplyDelta(WorldStateData worldState, WorldDelta delta)
        {
            var source = worldState ?? new WorldStateData();
            var change = delta ?? new WorldDelta();

            return new WorldStateData
            {
                Reputation = source.Reputation + change.Reputation,
                Stability = source.Stability + change.Stability,
                Budget = source.Budget + change.Budget,
                Influence = source.Influence + change.Influence,
                Casualties = source.Casualties + change.Casualties
            };
        }

        private static int GetCredibilityAverage(List<string> sourceInfoIds, Func<string, int> getInfoCredibility)
        {
            if (sourceInfoIds == null || sourceInfoIds.Count == 0)
            {
                return 50;
            }

            var sum = 0;
            var count = 0;

            for (var i = 0; i < sourceInfoIds.Count; i++)
            {
                var infoId = sourceInfoIds[i];
                if (string.IsNullOrWhiteSpace(infoId))
                {
                    continue;
                }

                sum += getInfoCredibility(infoId);
                count++;
            }

            if (count == 0)
            {
                return 50;
            }

            return (int)Math.Round(sum / (float)count);
        }

        private static List<string> BuildTopReasons(QuestDraft draft, int credibilityAvg)
        {
            var reasons = new List<string>();

            if (draft.Risk >= 4)
            {
                AddUnique(reasons, "위험도 높음");
            }

            if (credibilityAvg < 40)
            {
                AddUnique(reasons, "신뢰도 낮음");
            }

            if (credibilityAvg >= 70)
            {
                AddUnique(reasons, "신뢰도 높음");
            }

            if (draft.DeadlineDays <= 2)
            {
                AddUnique(reasons, "마감 촉박");
            }

            AddUnique(reasons, "현장 변수");
            AddUnique(reasons, "정보 부족");
            AddUnique(reasons, "운용 제약");

            if (reasons.Count > 3)
            {
                reasons.RemoveRange(3, reasons.Count - 3);
            }

            return reasons;
        }

        private static WorldDelta BuildWorldDelta(QuestDraft draft, QuestResultType resultType)
        {
            if (resultType == QuestResultType.Success)
            {
                return new WorldDelta
                {
                    Reputation = 5 + draft.Risk * 2,
                    Budget = draft.Reward,
                    Stability = 2,
                    Influence = 1,
                    Casualties = 0
                };
            }

            return new WorldDelta
            {
                Reputation = -(4 + draft.Risk * 2),
                Budget = 0,
                Stability = -3,
                Influence = 0,
                Casualties = draft.Risk >= 4 ? 1 : 0
            };
        }

        private QuestDraft FindDraft(string draftId)
        {
            for (var i = 0; i < _drafts.Count; i++)
            {
                if (string.Equals(_drafts[i].Id, draftId, StringComparison.Ordinal))
                {
                    return _drafts[i];
                }
            }

            return null;
        }

        private string CreateDraftId()
        {
            _draftSequence++;
            return $"Q{_draftSequence:D4}";
        }

        private void ResetSubmissionBucketIfNeeded(int currentDay)
        {
            if (_submissionDay == currentDay)
            {
                return;
            }

            _submittedToday.Clear();
            _submissionDay = currentDay;
        }

        private static void AddUnique(List<string> reasons, string reason)
        {
            for (var i = 0; i < reasons.Count; i++)
            {
                if (string.Equals(reasons[i], reason, StringComparison.Ordinal))
                {
                    return;
                }
            }

            reasons.Add(reason);
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}
