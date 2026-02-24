using Project.Systems.Day;
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
