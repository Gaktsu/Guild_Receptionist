using System;
using System.Collections.Generic;

namespace Project.Domain.Save
{
    [Serializable]
    public class SaveGameData
    {
        public int CurrentDay;
        public int Seed;
        public WorldStateData WorldState = new WorldStateData();
        public List<string> ArchivedInfoIds = new List<string>();
    }

    [Serializable]
    public class WorldStateData
    {
        public int Reputation;
        public int Gold;
        public int Population;
        public int ThreatLevel;
        public int Prosperity;
    }
}
