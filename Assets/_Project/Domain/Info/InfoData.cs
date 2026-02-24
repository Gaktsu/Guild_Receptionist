using System;

namespace Project.Domain.Info
{
    [Serializable]
    public class InfoData
    {
        public string Id;
        public string Title;
        public string Region;
        public InfoType Type;
        public int Credibility;
        public string Summary;
        public bool IsArchived;
        public bool IsDiscarded;
    }
}
