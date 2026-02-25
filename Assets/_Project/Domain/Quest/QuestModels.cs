using System;
using System.Collections.Generic;

namespace Project.Domain.Quest
{
    public enum QuestTemplateType
    {
        Combat,
        Investigation,
        Escort,
        Delivery,
        Purification,
        Diplomacy
    }

    [Serializable]
    public class QuestDraft
    {
        public string Id;
        public QuestTemplateType Type;
        public List<string> SourceInfoIds = new List<string>();
        public int Risk;
        public int Reward;
        public int DeadlineDays;
    }

    [Serializable]
    public class QuestIssued
    {
        public QuestDraft Draft;
        public int SubmittedDay;
    }

    public enum QuestResultType
    {
        Success,
        Fail
    }

    [Serializable]
    public class WorldDelta
    {
        public int Reputation;
        public int Stability;
        public int Budget;
        public int Influence;
        public int Casualties;
    }

    [Serializable]
    public class QuestResult
    {
        public string QuestId;
        public QuestResultType Result;
        public int FinalSuccessChance;
        public List<string> TopReasons = new List<string>();
        public WorldDelta Delta = new WorldDelta();
    }
}
