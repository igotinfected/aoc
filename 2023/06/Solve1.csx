var input = File.ReadAllLines(@"input.txt");

var timeList = input[0].Split(':')[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
var distanceList = input[1].Split(':')[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);

var races = timeList
    .Zip(distanceList, (time, distance) => new Race(int.Parse(time), int.Parse(distance)))
    .ToList();

Console.WriteLine(
    $"Winning options: {races.Select(race => race.CalculateWinningOptions()).Aggregate((x, y) => x * y)}"
);

record Race(int Time, int DistanceToBeat)
{
    const int AccelerationPerSecond = 1;

    public int CalculateWinningOptions() =>
        Enumerable
            .Range(0, Time)
            .Select(timeSpentAccelerating => CalculateDistance(timeSpentAccelerating))
            .Count(distance => distance > DistanceToBeat);

    private int CalculateDistance(int timeSpentAccelerating) =>
        timeSpentAccelerating * AccelerationPerSecond * (Time - timeSpentAccelerating);
};
