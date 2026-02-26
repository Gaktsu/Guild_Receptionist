using Project.Systems.Day;
using Project.Systems.Info;
using Project.Systems.Quest;
using Project.Systems.Save;
using Project.UI.Panels;
using UnityEngine;

namespace Project.Systems.Game
{
    public class GameBootstrapper : MonoBehaviour
    {
        // Singleton instance to prevent duplicates across scenes
        public static GameBootstrapper Instance { get; private set; }

        public GameSession Session { get; private set; }
        public InfoSystem InfoSystem { get; private set; }
        public QuestSystem QuestSystem { get; private set; }

        [SerializeField] private InfoPanel _infoPanel;
        [SerializeField] private QuestPanel _questPanel;
        [SerializeField] private ResultPanel _resultPanel;

        private DayFlowOrchestrator _dayFlowOrchestrator;

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
            InfoSystem = new InfoSystem(Session.ActionPointSystem);
            QuestSystem = new QuestSystem();

            Session.StartOrLoad();

            // Panels must subscribe BEFORE Orchestrator.Init() triggers DayStart→InfoPhase,
            // so they receive the state-change events.
            if (_infoPanel != null)
            {
                _infoPanel.Init(InfoSystem, Session.ActionPointSystem, daySystem);
            }

            if (_questPanel != null)
            {
                _questPanel.Init(QuestSystem, InfoSystem, daySystem, Session);
            }

            if (_resultPanel != null)
            {
                _resultPanel.Init(Session);
            }

            _dayFlowOrchestrator = gameObject.AddComponent<DayFlowOrchestrator>();
            _dayFlowOrchestrator.Init(
                Session,
                daySystem,
                InfoSystem,
                Session.ActionPointSystem,
                QuestSystem);

            if (_resultPanel != null)
            {
                _resultPanel.SetOrchestrator(_dayFlowOrchestrator);
                _dayFlowOrchestrator.SetResultPanel(_resultPanel);
            }

            Debug.Log($"[GameBootstrapper] 초기화 완료 — Day {Session.CurrentDay}, AP {Session.ActionPointSystem.CurrentAP}/{Session.ActionPointSystem.MaxAP}, State: {daySystem.CurrentState}");
        }

        [ContextMenu("Force New Game")]
        public void ForceNewGame()
        {
            if (Session == null)
            {
                return;
            }

            Session.NewGame();
            _dayFlowOrchestrator?.RestartFlow();
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
