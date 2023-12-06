var input = File.ReadAllLines(@"input.txt")
    .Where(line => !string.IsNullOrWhiteSpace(line))
    .ToArray();

var listOfAllMaps = new List<List<MapRange>>
{
    new List<MapRange>(),
    new List<MapRange>(),
    new List<MapRange>(),
    new List<MapRange>(),
    new List<MapRange>(),
    new List<MapRange>(),
    new List<MapRange>()
};

var seeds = input[0]
    .Split(':')[1]
    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
    .Select(long.Parse);

var index = 2;
for (var i = 0; i < listOfAllMaps.Count; i++)
{
    var indexOfNextMap = Array.FindIndex(input, index, line => char.IsDigit(line[0]) is false);
    indexOfNextMap = indexOfNextMap == -1 ? input.Length : indexOfNextMap;
    var mapValues = input[index..indexOfNextMap].Select(line =>
    {
        var split = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new MapRange(long.Parse(split[0]), long.Parse(split[1]), long.Parse(split[2]));
    }).ToList();
    listOfAllMaps[i].AddRange(mapValues);
    index = indexOfNextMap + 1;
}

var lowestSeedLocation = seeds.Select(GetSeedLocation).Min();

Console.WriteLine($"Lowest seed location: {lowestSeedLocation}");

record MapRange(long DestinationRangeStart, long SourceRangeStart, long Length);

bool IsInRange(MapRange range, long value) 
  => value >= range.SourceRangeStart && value <= range.SourceRangeStart + range.Length;

long GetDestinationValue(MapRange range, long value) 
  => range.DestinationRangeStart + (value - range.SourceRangeStart);


long GetSeedLocation(long seed)
{
  var currentValue = seed;
  for (var i = 0; i < listOfAllMaps.Count; i++)
  {
    var map = listOfAllMaps[i];
    var mapRange = map.FirstOrDefault(range => IsInRange(range, currentValue), null);
    currentValue = mapRange is null ? currentValue : GetDestinationValue(mapRange, currentValue);
  }
  return currentValue;
}
