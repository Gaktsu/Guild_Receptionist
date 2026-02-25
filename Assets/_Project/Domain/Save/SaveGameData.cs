using System;
using System.Collections.Generic;
using Project.Domain.Event;

namespace Project.Domain.Save
{
    [Serializable]
    public class SaveGameData
    {
        public int CurrentDay;
        public int Seed;
        public int CurrentAP;
        public WorldStateData WorldState = new WorldStateData();
        public List<string> ArchivedInfoIds = new List<string>();
        public ActiveEventData PlagueEvent = new ActiveEventData();
    }

    [Serializable]
    public class WorldStateData
    {
        public int Reputation;
        public int Stability;
        public int Budget;
        public int Influence;
        public int Casualties;
    }
}
