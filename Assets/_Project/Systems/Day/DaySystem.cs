using System;

namespace Project.Systems.Day
{
    public class DaySystem
    {
        public event Action<DayState> OnStateChanged;

        public DayState CurrentState { get; private set; } = DayState.DayStart;

        /// <summary>
        /// Attempts to transition to the next state when valid.
        /// </summary>
        public bool TrySetState(DayState nextState)
        {
            if (!CanTransition(nextState))
            {
                return false;
            }

            SetStateInternal(nextState);
            return true;
        }

        /// <summary>
        /// Forces state change without transition validation. Intended for debug use.
        /// </summary>
        public void ForceSetState(DayState state)
        {
            SetStateInternal(state);
        }

        /// <summary>
        /// Returns true when the requested transition follows the defined forward cycle.
        /// </summary>
        public bool CanTransition(DayState nextState)
        {
            return nextState == GetNextState(CurrentState);
        }

        private static DayState GetNextState(DayState currentState)
        {
            return currentState switch
            {
                DayState.DayStart => DayState.InfoPhase,
                DayState.InfoPhase => DayState.QuestDraftPhase,
                DayState.QuestDraftPhase => DayState.SubmissionPhase,
                DayState.SubmissionPhase => DayState.ResolutionPhase,
                DayState.ResolutionPhase => DayState.DayEnd,
                DayState.DayEnd => DayState.DayStart,
                _ => DayState.DayStart
            };
        }

        private void SetStateInternal(DayState nextState)
        {
            if (CurrentState == nextState)
            {
                return;
            }

            CurrentState = nextState;
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}
