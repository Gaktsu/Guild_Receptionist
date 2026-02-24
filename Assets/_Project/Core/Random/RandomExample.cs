using Project.Core.Random;

public class RandomExample
{
    public void Run()
    {
        IRandomService rngA = new SeededRandomService(2026);
        IRandomService rngB = new SeededRandomService(2026);

        // 같은 seed => 같은 순서 결과
        for (int i = 0; i < 10; i++)
        {
            float a = rngA.Value();
            float b = rngB.Value();
            // a == b (동일 시퀀스)
        }

        int pick = rngA.Range(0, 5); // 0~4
    }
}
