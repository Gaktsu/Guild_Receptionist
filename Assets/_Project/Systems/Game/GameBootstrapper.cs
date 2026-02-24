using System.Collections.Generic;
using Project.Domain.Quest;
using Project.Systems.Day;
using Project.Systems.Info;
using Project.Systems.Quest;
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

            // // ──────────────────────────────────────────────
            // // [임시 테스트] InfoSystem.StartDay() 동작 확인용
            // // 추후 정식 흐름이 완성되면 제거할 수 있음
            // // ──────────────────────────────────────────────
            // var dayRng = Session.CreateDayRng();
            // var infoSystem = new InfoSystem(Session.ActionPointSystem);
            // infoSystem.StartDay(dayRng, Session.CurrentDay);

            // Debug.Log($"[임시 테스트] Day {Session.CurrentDay} — TodayInfos ({infoSystem.TodayInfos.Count}개)");
            // for (var i = 0; i < infoSystem.TodayInfos.Count; i++)
            // {
            //     var info = infoSystem.TodayInfos[i];
            //     Debug.Log($"  [{i + 1}] {info.Id} | {info.Title} | 신뢰도: {info.Credibility} | {info.Summary}");
            // }
            // // ──────────────────────────────────────────────

            // // ──────────────────────────────────────────────
            // // [임시 테스트] AP 시스템 흐름 확인용
            // // 추후 정식 흐름이 완성되면 제거할 수 있음
            // // ──────────────────────────────────────────────

            // // 1) Day 시작 → AP=5 확인
            // Debug.Log($"[임시 AP 테스트] Day 시작 후 AP = {Session.ActionPointSystem.CurrentAP} (기대값: {Session.ActionPointSystem.MaxAP})");

            // // 2) Info 1개 조사 → AP=4
            // var firstInfoId = infoSystem.TodayInfos[0].Id;
            // var investigateResult = infoSystem.TryInvestigate(firstInfoId);
            // Debug.Log($"[임시 AP 테스트] 조사 시도 (Id: {firstInfoId}) → 성공: {investigateResult}, AP = {Session.ActionPointSystem.CurrentAP} (기대값: 4)");

            // // 3) AP 0까지 감소 확인
            // for (var apIdx = 1; apIdx < infoSystem.TodayInfos.Count && Session.ActionPointSystem.CurrentAP > 0; apIdx++)
            // {
            //     var id = infoSystem.TodayInfos[apIdx].Id;
            //     var result = infoSystem.TryInvestigate(id);
            //     Debug.Log($"[임시 AP 테스트] 조사 (Id: {id}) → 성공: {result}, AP = {Session.ActionPointSystem.CurrentAP}");
            // }
            // Debug.Log($"[임시 AP 테스트] AP 소진 완료 → AP = {Session.ActionPointSystem.CurrentAP} (기대값: 0)");

            // // 4) 저장 후 재로드 → AP 유지 확인
            // Session.Save();
            // Debug.Log("[임시 AP 테스트] 저장 완료. 재로드 시뮬레이션...");

            // var reloadedSave = saveSystem.Load();
            // Debug.Log($"[임시 AP 테스트] 재로드된 세이브 AP = {reloadedSave.CurrentAP} (기대값: 0, 저장된 값과 일치해야 함)");

            // // ──────────────────────────────────────────────

            // // ──────────────────────────────────────────────
            // // [임시 테스트] QuestSystem 드래프트 → 제출 → 판정 흐름 확인용
            // // 추후 정식 흐름이 완성되면 제거할 수 있음
            // // ──────────────────────────────────────────────
            // var questSystem = new QuestSystem();
            // var resolveRng = Session.CreateDayRng();

            // // 1) TodayInfos 중 1~2개를 소스로 드래프트 4개 생성
            // var draft1 = questSystem.CreateDraft(
            //     QuestTemplateType.Combat,
            //     new List<string> { infoSystem.TodayInfos[0].Id },
            //     risk: 3, reward: 200, deadlineDays: 3);

            // var draft2 = questSystem.CreateDraft(
            //     QuestTemplateType.Investigation,
            //     new List<string> { infoSystem.TodayInfos[1].Id, infoSystem.TodayInfos[2].Id },
            //     risk: 2, reward: 150, deadlineDays: 4);

            // var draft3 = questSystem.CreateDraft(
            //     QuestTemplateType.Escort,
            //     new List<string> { infoSystem.TodayInfos[3].Id },
            //     risk: 4, reward: 300, deadlineDays: 2);

            // var draft4 = questSystem.CreateDraft(
            //     QuestTemplateType.Delivery,
            //     new List<string> { infoSystem.TodayInfos[4].Id, infoSystem.TodayInfos[5].Id },
            //     risk: 1, reward: 100, deadlineDays: 5);

            // Debug.Log($"[임시 Quest 테스트] 드래프트 {questSystem.Drafts.Count}개 생성 완료");
            // for (var qi = 0; qi < questSystem.Drafts.Count; qi++)
            // {
            //     var d = questSystem.Drafts[qi];
            //     Debug.Log($"  드래프트[{qi + 1}] {d.Id} | 유형: {d.Type} | 위험: {d.Risk} | 보상: {d.Reward} | 마감: {d.DeadlineDays}일 | 소스Info: {string.Join(", ", d.SourceInfoIds)}");
            // }

            // // 2) 최대 4개 submit 확인
            // var submitResults = new[]
            // {
            //     questSystem.TrySubmit(draft1.Id, Session.CurrentDay),
            //     questSystem.TrySubmit(draft2.Id, Session.CurrentDay),
            //     questSystem.TrySubmit(draft3.Id, Session.CurrentDay),
            //     questSystem.TrySubmit(draft4.Id, Session.CurrentDay)
            // };
            // Debug.Log($"[임시 Quest 테스트] 제출 결과: [{string.Join(", ", submitResults)}] — 제출됨: {questSystem.SubmittedToday.Count}개 (최대: {questSystem.MaxSubmissionsPerDay})");

            // // 3) ResolveSubmitted 호출 → 결과/월드 델타 출력
            // var questResults = questSystem.ResolveSubmitted(
            //     Session.CurrentDay,
            //     resolveRng,
            //     infoId =>
            //     {
            //         for (var si = 0; si < infoSystem.TodayInfos.Count; si++)
            //         {
            //             if (infoSystem.TodayInfos[si].Id == infoId)
            //                 return infoSystem.TodayInfos[si].Credibility;
            //         }
            //         return 50;
            //     });

            // Debug.Log($"[임시 Quest 테스트] 판정 완료 — 결과 {questResults.Count}개");
            // for (var ri = 0; ri < questResults.Count; ri++)
            // {
            //     var qr = questResults[ri];
            //     Debug.Log($"  결과[{ri + 1}] {qr.QuestId} | {qr.Result} | 성공확률: {qr.FinalSuccessChance}% | 사유: {string.Join(", ", qr.TopReasons)}");
            //     Debug.Log($"    월드 델타 → 평판: {qr.Delta.Reputation:+#;-#;0}, 안정: {qr.Delta.Stability:+#;-#;0}, 예산: {qr.Delta.Budget:+#;-#;0}, 영향력: {qr.Delta.Influence:+#;-#;0}, 사상자: {qr.Delta.Casualties:+#;-#;0}");
            // }
            // // ──────────────────────────────────────────────
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
