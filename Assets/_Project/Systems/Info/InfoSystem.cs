using System;
using System.Collections.Generic;
using Project.Core.Random;
using Project.Domain.Info;

namespace Project.Systems.Info
{
    public class InfoSystem
    {
        private static readonly string[] Regions =
        {
            "북문 지구", "상업 거리", "구항구", "성벽 외곽", "서부 농지", "마도 연구구"
        };

        private static readonly string[] Subjects =
        {
            "괴수", "실종 사건", "호위 요청", "보급 수송", "오염 정화", "외교 사절"
        };

        private static readonly string[] SummaryTemplates =
        {
            "목격담이 늘고 있어 확인이 필요하다.",
            "주민들이 불안을 호소하고 있어 빠른 대응이 요구된다.",
            "소규모 단서가 모이고 있어 추가 조사가 필요하다.",
            "길드 평판에 영향을 줄 수 있어 신중한 판단이 필요하다."
        };

        private readonly List<InfoData> _todayInfos = new List<InfoData>();

        public IReadOnlyList<InfoData> TodayInfos => _todayInfos;

        /// <summary>
        /// Creates six deterministic infos for the given day using the provided RNG.
        /// </summary>
        public void StartDay(IRandomService rng, int dayIndex)
        {
            if (rng == null)
            {
                throw new ArgumentNullException(nameof(rng));
            }

            if (dayIndex <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dayIndex));
            }

            _todayInfos.Clear();

            for (var i = 0; i < 6; i++)
            {
                var region = Regions[rng.Range(0, Regions.Length)];
                var type = (InfoType)rng.Range(0, Enum.GetValues(typeof(InfoType)).Length);
                var subject = Subjects[(int)type];
                var credibility = rng.Range(20, 86);
                var template = SummaryTemplates[rng.Range(0, SummaryTemplates.Length)];
                var idToken = rng.Range(1000, 9999);

                _todayInfos.Add(new InfoData
                {
                    Id = $"D{dayIndex}_I{i + 1}_{idToken}",
                    Title = $"{region}에서 {subject} 관련 소문",
                    Region = region,
                    Type = type,
                    Credibility = credibility,
                    Summary = $"{region}의 {subject} 제보가 들어왔다. {template}",
                    IsArchived = false,
                    IsDiscarded = false
                });
            }
        }

        /// <summary>
        /// Archives an info by ID. Archived/discarded infos cannot be changed again.
        /// </summary>
        public bool TryArchive(string infoId)
        {
            var info = FindMutableInfo(infoId);
            if (info == null)
            {
                return false;
            }

            info.IsArchived = true;
            return true;
        }

        /// <summary>
        /// Discards an info by ID. Archived/discarded infos cannot be changed again.
        /// </summary>
        public bool TryDiscard(string infoId)
        {
            var info = FindMutableInfo(infoId);
            if (info == null)
            {
                return false;
            }

            info.IsDiscarded = true;
            return true;
        }

        /// <summary>
        /// Investigates an info by ID and increases credibility by 15 up to 100.
        /// </summary>
        public bool TryInvestigate(string infoId)
        {
            var info = FindMutableInfo(infoId);
            if (info == null)
            {
                return false;
            }

            info.Credibility = Math.Min(100, info.Credibility + 15);
            return true;
        }

        private InfoData FindMutableInfo(string infoId)
        {
            if (string.IsNullOrWhiteSpace(infoId))
            {
                return null;
            }

            for (var i = 0; i < _todayInfos.Count; i++)
            {
                var info = _todayInfos[i];
                if (!string.Equals(info.Id, infoId, StringComparison.Ordinal))
                {
                    continue;
                }

                if (info.IsArchived || info.IsDiscarded)
                {
                    return null;
                }

                return info;
            }

            return null;
        }
    }
}
