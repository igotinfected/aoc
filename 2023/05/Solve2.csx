var input = File.ReadAllLines(@"input.txt")
    .Where(line => !string.IsNullOrWhiteSpace(line))
    .ToArray();
var listOfAllMaps = new List<List<RangeMapping>>
{
    new List<RangeMapping>(),
    new List<RangeMapping>(),
    new List<RangeMapping>(),
    new List<RangeMapping>(),
    new List<RangeMapping>(),
    new List<RangeMapping>(),
    new List<RangeMapping>()
};
var seedPairs = input[0]
    .Split(':')[1]
    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
    .Select(long.Parse)
    .ToList();

// prepare seeds
var seedRanges = new List<Range>();
for (var i = 0; i < seedPairs.Count; i += 2)
{
    seedRanges.Add(new Range(seedPairs[i], seedPairs[i] + seedPairs[i + 1] - 1));
}

// prepare data maps
var index = 2;
for (var i = 0; i < listOfAllMaps.Count; i++)
{
    var indexOfNextMap = Array.FindIndex(input, index, line => char.IsDigit(line[0]) is false);
    indexOfNextMap = indexOfNextMap == -1 ? input.Length : indexOfNextMap;
    var mapValues = input[index..indexOfNextMap].Select(line =>
    {
        var split = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var sourceStart = long.Parse(split[1]);
        var destinationStart = long.Parse(split[0]);
        var length = long.Parse(split[2]);
        return new RangeMapping(new Range(sourceStart, sourceStart + length - 1), new Range(destinationStart, destinationStart + length - 1));
    }).ToList();
    listOfAllMaps[i].AddRange(mapValues);
    index = indexOfNextMap + 1;
}

var lowestSeedLocation = seedRanges.Select(GetLowestSeedLocationForSeedRange).Min();
Console.WriteLine($"Lowest seed location: {lowestSeedLocation}");


long GetLowestSeedLocationForSeedRange(Range range)
{
  var transformedRanges = new HashSet<Range> { range };
  var index = 0;
  foreach (var map in listOfAllMaps)
  {
    transformedRanges = TransformRangesAndKeepUnmatched(transformedRanges, map);
    index++;
  }

  return transformedRanges.Select(r => r.Start).Min();
}

record Range(long Start, long End);
record RangeMapping(Range SourceRange, Range DesinationRange);

HashSet<Range> TransformRangesAndKeepUnmatched(HashSet<Range> ranges, List<RangeMapping> map)
{
  var result = new List<Range>();
  foreach (var range in ranges)
  {
    result.AddRange(TransformRangeAndKeepUnmatched(range, map));
  }
  return result.ToHashSet();
}

HashSet<Range> TransformRangeAndKeepUnmatched(Range range, List<RangeMapping> map)
{
  var transformedRanges = new HashSet<Range>();
  var transformsToApply = new List<(Range range, RangeMapping mapping)>();
  foreach (var mapping in map)
  {
    if (mapping.SourceRange.Start > range.End || mapping.SourceRange.End < range.Start)
    {
      continue;
    }

    var matchedRange = new Range(Math.Max(mapping.SourceRange.Start, range.Start), Math.Min(mapping.SourceRange.End, range.End));
    transformsToApply.Add((matchedRange, mapping));
  }

  // apply transforms
  foreach (var (matchedRange, mapping) in transformsToApply)
  {
    var transformedRange = new Range(matchedRange.Start - mapping.SourceRange.Start + mapping.DesinationRange.Start, matchedRange.End - mapping.SourceRange.Start + mapping.DesinationRange.Start);
    transformedRanges.Add(transformedRange);
  }
  // add unmatched ranges
  var unmatchedRanges = GenerateUnmatchedRanges(range, transformsToApply.Select(t => t.range).ToHashSet());
  transformedRanges.UnionWith(unmatchedRanges);
  if (transformedRanges.Count == 0)
  {
    transformedRanges.Add(range);
  }

  return transformedRanges;
}

// regenerate unmatched ranges based on matched ranges
HashSet<Range> GenerateUnmatchedRanges(Range range, HashSet<Range> matchedRanges)
{
  var unmatchedRanges = new HashSet<Range>();
  var start = range.Start;
 
  foreach (var matchedRange in matchedRanges.OrderBy(r => r.Start))
  {
    if (matchedRange.Start > start)
    {
      unmatchedRanges.Add(new Range(start, Math.Min(matchedRange.Start - 1, range.End)));
    }
    start = matchedRange.End + 1;
  }

  return unmatchedRanges;
}
