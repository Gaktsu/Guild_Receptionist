using Project.Systems.Day;
using UnityEngine;

public class DayFlowExample
{
    private readonly DaySystem _daySystem = new DaySystem();

    public void Init()
    {
        _daySystem.OnStateChanged += state => Debug.Log($"Day state changed: {state}");
    }

    public void Tick()
    {
        // DayStart -> InfoPhase (정상)
        bool moved = _daySystem.TrySetState(DayState.InfoPhase); // true

        // 역방향 요청 (예: InfoPhase -> DayStart) 은 false
        bool invalid = _daySystem.TrySetState(DayState.DayStart); // false

        // 디버그 강제 이동
        _daySystem.ForceSetState(DayState.DayEnd);
    }
}
