using System.Collections.Immutable;

List<char> PossibleChars = new List<char>() { '.', '#' };
char Wildcard = '?';
var Cache = new Dictionary<Record, long>();

Solve();

void Solve()
{
    var input = File.ReadAllLines(@"input.txt");
    SolvePartOne(input);
    SolvePartTwo(input);
}

void SolvePartOne(string[] input)
{
    var records = GenerateRecords(input, 1);
    var validCombinations = records.Select(ProcessRecord).Sum();
    Console.WriteLine($"Part One: {validCombinations}");
}

void SolvePartTwo(string[] input)
{
    var records = GenerateRecords(input, 5);
    var validCombinations = records.Select(ProcessRecord).Sum();
    Console.WriteLine($"Part Two: {validCombinations}");
}

long ProcessRecord(Record record)
{
    if (Cache.ContainsKey(record))
    {
        return Cache[record];
    }

    Cache[record] = record.ConditionRecord.FirstOrDefault() switch
    {
        '.' => ProcessDot(record),
        '?' => ProcessWildcard(record),
        '#' => ProcessHash(record),
        _ => ProcessEndOfRecord(record)
    };
    return Cache[record];
}

long ProcessDot(Record record) =>
    ProcessRecord(record with { ConditionRecord = record.ConditionRecord[1..] });

long ProcessHash(Record record)
{
    // if there are no groupings left but we still have hashes this is not a valid solution
    if (record.Groupings.Any() is false)
    {
        return 0;
    }

    var currentGrouping = record.Groupings.Peek();
    var recordWithoutConsumedGrouping = record with { Groupings = record.Groupings.Pop() };
    var consumedSprings = recordWithoutConsumedGrouping
        .ConditionRecord
        .TakeWhile(c => c is '#' or '?')
        .Count();

    var didNotConsumeEnoughSprings = currentGrouping > consumedSprings;
    var consumedTooManyDeadSprings =
        currentGrouping < consumedSprings
        && recordWithoutConsumedGrouping.ConditionRecord[currentGrouping] == '#';
    if (didNotConsumeEnoughSprings || consumedTooManyDeadSprings)
    {
        return 0;
    }

    // if the current grouping has the same length as our search string we terminate
    if (recordWithoutConsumedGrouping.ConditionRecord.Length == currentGrouping)
    {
        return ProcessRecord(recordWithoutConsumedGrouping with { ConditionRecord = string.Empty });
    }

    // in case we consumed more wildcards at the end we continue at the first character after the current grouping
    return ProcessRecord(
        recordWithoutConsumedGrouping with
        {
            ConditionRecord = recordWithoutConsumedGrouping.ConditionRecord[(currentGrouping + 1)..]
        }
    );
}

long ProcessWildcard(Record record) =>
    ProcessRecord(record with { ConditionRecord = $".{record.ConditionRecord[1..]}" })
    + ProcessRecord(record with { ConditionRecord = $"#{record.ConditionRecord[1..]}" });

long ProcessEndOfRecord(Record record) => record.Groupings.Any() ? 0 : 1;

List<Record> GenerateRecords(string[] input, int repetitions) =>
    input
        .Select(line =>
        {
            var tokens = line.Split(' ');
            var conditionRecord = Repeat(tokens[0], repetitions, '?');
            var groupings = Repeat(tokens[1], repetitions, ',').Split(',').Select(int.Parse);
            return new Record(conditionRecord, ImmutableStack.CreateRange(groupings.Reverse()));
        })
        .ToList();

string Repeat(string input, int repetitions, char separator) =>
    string.Join(separator, Enumerable.Repeat(input, repetitions));

record Record(string ConditionRecord, ImmutableStack<int> Groupings);
