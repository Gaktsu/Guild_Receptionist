using System;
using Project.Core.Random;
using Project.Domain.Save;
using Project.Systems.Day;
using Project.Systems.Save;

namespace Project.Systems.Game
{
    public class GameSession
    {
        private readonly DaySystem _daySystem;
        private readonly SaveSystem _saveSystem;

        public int CurrentDay { get; private set; }
        public int GameSeed { get; private set; }
        public WorldStateData WorldState { get; private set; }

        public DaySystem DaySystem => _daySystem;

        public GameSession(DaySystem daySystem, SaveSystem saveSystem)
        {
            _daySystem = daySystem ?? throw new ArgumentNullException(nameof(daySystem));
            _saveSystem = saveSystem ?? throw new ArgumentNullException(nameof(saveSystem));
            WorldState = CreateDefaultWorldState();
        }

        /// <summary>
        /// Starts a new game when no save exists, otherwise loads the existing save.
        /// </summary>
        public void StartOrLoad()
        {
            var saveData = _saveSystem.Load();
            if (saveData == null)
            {
                NewGame();
                return;
            }

            LoadGame(saveData);
        }

        /// <summary>
        /// Initializes a new game and immediately persists it.
        /// </summary>
        public void NewGame(int? seedOverride = null)
        {
            GameSeed = seedOverride ?? GenerateTimeSeed();
            CurrentDay = 1;
            WorldState = CreateDefaultWorldState();
            _daySystem.ForceSetState(DayState.DayStart);
            Save();
        }

        /// <summary>
        /// Advances to the next day and persists the result.
        /// </summary>
        public void NextDay()
        {
            CurrentDay++;
            _daySystem.ForceSetState(DayState.DayStart);
            Save();
        }

        /// <summary>
        /// Saves current session values into save storage.
        /// </summary>
        public void Save()
        {
            var data = new SaveGameData
            {
                CurrentDay = CurrentDay,
                Seed = GameSeed,
                WorldState = CloneWorldState(WorldState)
            };

            _saveSystem.Save(data);
        }

        /// <summary>
        /// Creates deterministic RNG for the current day from game seed and day index.
        /// </summary>
        public IRandomService CreateDayRng()
        {
            var daySeed = Hash(GameSeed, CurrentDay);
            return new SeededRandomService(daySeed);
        }

        private void LoadGame(SaveGameData saveData)
        {
            GameSeed = saveData.Seed;
            CurrentDay = saveData.CurrentDay > 0 ? saveData.CurrentDay : 1;
            WorldState = saveData.WorldState != null ? CloneWorldState(saveData.WorldState) : CreateDefaultWorldState();
            _daySystem.ForceSetState(DayState.DayStart);
        }

        private static int GenerateTimeSeed()
        {
            var ticks = DateTime.UtcNow.Ticks;
            return unchecked((int)(ticks ^ (ticks >> 32)));
        }

        private static int Hash(int seed, int day)
        {
            unchecked
            {
                var value = seed;
                value ^= day + (int)0x9e3779b9 + (value << 6) + (value >> 2);
                value ^= value << 13;
                value ^= value >> 17;
                value ^= value << 5;
                return value;
            }
        }

        private static WorldStateData CreateDefaultWorldState()
        {
            return new WorldStateData
            {
                Reputation = 50,
                Stability = 50,
                Budget = 1000,
                Influence = 0,
                Casualties = 0
            };
        }

        private static WorldStateData CloneWorldState(WorldStateData source)
        {
            if (source == null)
            {
                return CreateDefaultWorldState();
            }

            return new WorldStateData
            {
                Reputation = source.Reputation,
                Stability = source.Stability,
                Budget = source.Budget,
                Influence = source.Influence,
                Casualties = source.Casualties
            };
        }
    }
}
