var input = File.ReadAllLines(@"input.txt");

var time = input[0].Split(':')[1].Replace(" ", "");
var distance = input[1].Split(':')[1].Replace(" ", "");

var race = new Race(long.Parse(time), long.Parse(distance));

Console.WriteLine($"Winning options: {race.CalculateWinningOptions()}");

record Race(long Time, long DistanceToBeat)
{
    const long AccelerationPerSecond = 1;

    public long CalculateWinningOptions()
    {
        var winningOptions = 0;
        for (var i = 0; i < Time; i++)
        {
            var distance = CalculateDistance(i);
            if (distance > DistanceToBeat)
            {
                winningOptions++;
            }
        }

        return winningOptions;
    }

    private long CalculateDistance(long timeSpentAccelerating) =>
        timeSpentAccelerating * AccelerationPerSecond * (Time - timeSpentAccelerating);
};
