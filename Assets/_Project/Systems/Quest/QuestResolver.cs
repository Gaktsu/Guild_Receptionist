using System;
using Project.Core.Random;
using Project.Domain.Quest;

namespace Project.Systems.Quest
{
    public static class QuestResolver
    {
        public static QuestResolveOutcome Resolve(QuestDraft draft, int leadCredibilityAvg, IRandomService rng)
        {
            if (draft == null)
            {
                throw new ArgumentNullException(nameof(draft));
            }

            if (rng == null)
            {
                throw new ArgumentNullException(nameof(rng));
            }

            var baseChance = 70 - (draft.Risk - 1) * 15;
            var credibilityBonus = (leadCredibilityAvg - 50) * 0.6f;
            var finalChance = ClampToInt(baseChance + credibilityBonus, 5, 95);
            var roll = rng.Value() * 100f;
            var result = roll < finalChance ? QuestResultType.Success : QuestResultType.Fail;

            return new QuestResolveOutcome(result, finalChance);
        }

        private static int ClampToInt(float value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return (int)Math.Round(value);
        }
    }

    public readonly struct QuestResolveOutcome
    {
        public QuestResultType ResultType { get; }
        public int FinalChance { get; }

        public QuestResolveOutcome(QuestResultType resultType, int finalChance)
        {
            ResultType = resultType;
            FinalChance = finalChance;
        }
    }
}
