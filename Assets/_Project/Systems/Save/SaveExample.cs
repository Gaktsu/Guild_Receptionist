using Project.Domain.Save;
using Project.Systems.Save;

public class SaveExample
{
    public void Run()
    {
        var saveSystem = new SaveSystem();

        var data = new SaveGameData
        {
            CurrentDay = 3,
            Seed = 12345,
            WorldState = new WorldStateData
            {
                Reputation = 10,
                Gold = 250,
                Population = 80,
                ThreatLevel = 2,
                Prosperity = 15
            }
        };
        data.ArchivedInfoIds.Add("info_001");

        saveSystem.Save(data);

        var loaded = saveSystem.Load(); // 파일 없으면 null
        saveSystem.Clear();             // save.json 삭제
    }
}
