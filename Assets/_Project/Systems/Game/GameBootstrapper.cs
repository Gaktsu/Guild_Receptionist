using Project.Systems.Day;
using Project.Systems.Save;
using UnityEngine;

namespace Project.Systems.Game
{
    public class GameBootstrapper : MonoBehaviour
    {
        public GameSession Session { get; private set; }

        private void Awake()
        {
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
