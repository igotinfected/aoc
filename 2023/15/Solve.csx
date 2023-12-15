Solve();

void Solve()
{
    var input = File.ReadAllText(@"input.txt").Replace("\r", "").Replace("\n", "");

    SolvePartOne(input);
    SolvePartTwo(input);
}

void SolvePartOne(string input)
{
    var result = input.Split(',').Select(Hash).Sum();
    Console.WriteLine($"Part one: {result}");
}

void SolvePartTwo(string input)
{
    var hashMaps = Enumerable.Range(0, 256)
        .Select(i => new Dictionary<string, (int, int)>())
        .ToList();
    var listOfHashMaps = new List<Dictionary<string, (int, int)>>(hashMaps);

    input.Split(',')
        .ToList()
        .ForEach(operation => ApplyOperation(operation, listOfHashMaps));

    var focusingPower = listOfHashMaps
        .Select((hashmap, index) =>
            hashmap.Values.Select(map => GetFocusingPower((index, map.Item1, map.Item2))).Sum())
        .Sum();
    Console.WriteLine($"Part two: {focusingPower}");
}

int Hash(string input)
{
    var currentValue = 0;

    foreach (var c in input)
    {
        if (c == ',')
        {
            continue;
        }
        currentValue += (int)c;
        currentValue *= 17;
        currentValue %= 256;
    }

    return currentValue;
}

void ApplyOperation(string input, List<Dictionary<string, (int, int)>> hashMap)
{
    var indexOfOperation = input.IndexOfAny(new char[] { '-', '=' });
    var hash = Hash(input[..indexOfOperation]);
    var operation = input[indexOfOperation];


    var exists = hashMap[hash].TryGetValue(input[..indexOfOperation], out var item);
    if (operation == '-')
    {
        if (exists is false)
        {
            return;
        }

        hashMap[hash].Remove(input[..indexOfOperation]);
        hashMap[hash]
            .Where(x => x.Value.Item1 > item.Item1)
            .Select(x => (x.Key, index: x.Value.Item1 - 1, focalLength: x.Value.Item2))
            .ToList()
            .ForEach(newItem => hashMap[hash][newItem.Key] = (newItem.index, newItem.focalLength));
    }
    else if (operation == '=')
    {
        if (exists)
        {
            hashMap[hash][input[..indexOfOperation]] = (
                item.Item1,
                int.Parse(input[^1].ToString())
            );
        }
        else
        {
            var newIndex = hashMap[hash].Any()
                ? hashMap[hash].Values.Max(item => item.Item1) + 1
                : 0;
            var newFocalLength = int.Parse(input[^1].ToString());
            hashMap[hash][input[..indexOfOperation]] = (newIndex, newFocalLength);
        }
    }
}

long GetFocusingPower((int, int, int) item)
    => (1 + item.Item1) * (1 + item.Item2) * item.Item3;
