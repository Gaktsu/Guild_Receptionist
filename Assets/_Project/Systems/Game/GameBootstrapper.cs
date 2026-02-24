using Project.Systems.Day;
using Project.Systems.Info;
using Project.Systems.Save;
using UnityEngine;

namespace Project.Systems.Game
{
    public class GameBootstrapper : MonoBehaviour
    {
        // Singleton instance to prevent duplicates across scenes
        public static GameBootstrapper Instance { get; private set; }

        public GameSession Session { get; private set; }

        private void Awake()
        {
            // If another instance exists, destroy this duplicate and do nothing
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            var daySystem = new DaySystem();
            var saveSystem = new SaveSystem();

            Session = new GameSession(daySystem, saveSystem);
            Session.StartOrLoad();

            // ──────────────────────────────────────────────
            // [임시 테스트] InfoSystem.StartDay() 동작 확인용
            // 추후 정식 흐름이 완성되면 제거할 수 있음
            // ──────────────────────────────────────────────
            var dayRng = Session.CreateDayRng();
            var infoSystem = new InfoSystem(Session.ActionPointSystem);
            infoSystem.StartDay(dayRng, Session.CurrentDay);

            Debug.Log($"[임시 테스트] Day {Session.CurrentDay} — TodayInfos ({infoSystem.TodayInfos.Count}개)");
            for (var i = 0; i < infoSystem.TodayInfos.Count; i++)
            {
                var info = infoSystem.TodayInfos[i];
                Debug.Log($"  [{i + 1}] {info.Id} | {info.Title} | 신뢰도: {info.Credibility} | {info.Summary}");
            }
            // ──────────────────────────────────────────────

            // ──────────────────────────────────────────────
            // [임시 테스트] AP 시스템 흐름 확인용
            // 추후 정식 흐름이 완성되면 제거할 수 있음
            // ──────────────────────────────────────────────

            // 1) Day 시작 → AP=5 확인
            Debug.Log($"[임시 AP 테스트] Day 시작 후 AP = {Session.ActionPointSystem.CurrentAP} (기대값: {Session.ActionPointSystem.MaxAP})");

            // 2) Info 1개 조사 → AP=4
            var firstInfoId = infoSystem.TodayInfos[0].Id;
            var investigateResult = infoSystem.TryInvestigate(firstInfoId);
            Debug.Log($"[임시 AP 테스트] 조사 시도 (Id: {firstInfoId}) → 성공: {investigateResult}, AP = {Session.ActionPointSystem.CurrentAP} (기대값: 4)");

            // 3) AP 0까지 감소 확인
            for (var apIdx = 1; apIdx < infoSystem.TodayInfos.Count && Session.ActionPointSystem.CurrentAP > 0; apIdx++)
            {
                var id = infoSystem.TodayInfos[apIdx].Id;
                var result = infoSystem.TryInvestigate(id);
                Debug.Log($"[임시 AP 테스트] 조사 (Id: {id}) → 성공: {result}, AP = {Session.ActionPointSystem.CurrentAP}");
            }
            Debug.Log($"[임시 AP 테스트] AP 소진 완료 → AP = {Session.ActionPointSystem.CurrentAP} (기대값: 0)");

            // 4) 저장 후 재로드 → AP 유지 확인
            Session.Save();
            Debug.Log("[임시 AP 테스트] 저장 완료. 재로드 시뮬레이션...");

            var reloadedSave = saveSystem.Load();
            Debug.Log($"[임시 AP 테스트] 재로드된 세이브 AP = {reloadedSave.CurrentAP} (기대값: 0, 저장된 값과 일치해야 함)");

            // ──────────────────────────────────────────────
        }

        [ContextMenu("Force New Game")]
        public void ForceNewGame()
        {
            Session?.NewGame();
        }

        [ContextMenu("Force Save")]
        public void ForceSave()
        {
            Session?.Save();
        }

        [ContextMenu("Force Next Day")]
        public void ForceNextDay()
        {
            Session?.NextDay();
        }
    }
}
