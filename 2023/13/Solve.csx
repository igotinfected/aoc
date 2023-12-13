Solve();

void Solve()
{
    var input = File.ReadAllText(@"input.txt")
        .Split(Environment.NewLine + Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
        .Select(
            pattern => pattern.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
        )
        .ToArray();
    var patterns = ParsePatterns(input);

    var (partOneResult, partTwoResult) = patterns
        .Select(pattern =>
        {
            var duplicateRows = FindAdjacentDuplicates<Row>(pattern.Rows, 0);
            var duplicateColumns = FindAdjacentDuplicates<Column>(pattern.Columns, 0);
            var (partOneRows, rowsCountedAsMirrored) = CountIfDuplicatesAreMirrored<Row>(
                duplicateRows,
                pattern.Rows,
                100,
                0
            );
            var (partOneColumns, columnsCountedAsMirrored) = CountIfDuplicatesAreMirrored<Column>(
                duplicateColumns,
                pattern.Columns,
                1,
                0
            );
            var duplicateRowsOffByOne = FindAdjacentDuplicates<Row>(pattern.Rows, 1)
                .Except(rowsCountedAsMirrored)
                .ToList();
            var duplicateColumnsOffByOne = FindAdjacentDuplicates<Column>(pattern.Columns, 1)
                .Except(columnsCountedAsMirrored)
                .ToList();
            var (partTwoRows, _) = CountIfDuplicatesAreMirrored<Row>(
                duplicateRowsOffByOne,
                pattern.Rows,
                100,
                1
            );
            var (partTwoColumns, _) = CountIfDuplicatesAreMirrored<Column>(
                duplicateColumnsOffByOne,
                pattern.Columns,
                1,
                1
            );
            return (partOneRows + partOneColumns, partTwoRows + partTwoColumns);
        })
        .Aggregate(
            (partOne, partTwo) => (partOne.Item1 + partTwo.Item1, partOne.Item2 + partTwo.Item2)
        );

    Console.WriteLine($"Part one: {partOneResult}");
    Console.WriteLine($"Part two: {partTwoResult}");
}

List<Pattern> ParsePatterns(string[][] arrayOfPatterns)
{
    var patterns = new List<Pattern>();

    for (var patternIndex = 0; patternIndex < arrayOfPatterns.Length; patternIndex++)
    {
        var rows = new List<Row>();
        var columns = new List<Column>();
        var rowsByValue = new Dictionary<string, List<Row>>();
        var columnsByValue = new Dictionary<string, List<Column>>();

        for (int i = 0; i < arrayOfPatterns[patternIndex].Length; i++)
        {
            var row = new Row(i, arrayOfPatterns[patternIndex][i]);
            rows.Add(row);

            var existingEntry = rowsByValue.GetValueOrDefault(row.Value, new List<Row>());
            rowsByValue[row.Value] = [..existingEntry, row];
        }

        for (var i = 0; i < arrayOfPatterns[patternIndex][0].Length; i++)
        {
            var stringBuilder = new StringBuilder();

            for (var j = 0; j < arrayOfPatterns[patternIndex].Length; j++)
            {
                stringBuilder.Append(arrayOfPatterns[patternIndex][j][i]);
            }

            var column = new Column(i, stringBuilder.ToString());
            columns.Add(column);

            var existingEntry = columnsByValue.GetValueOrDefault(column.Value, new List<Column>());
            columnsByValue[column.Value] = [..existingEntry, column];
        }

        patterns.Add(new Pattern(rows, rowsByValue, columns, columnsByValue));
    }

    return patterns;
}

List<(T, T)> FindAdjacentDuplicates<T>(List<T> entries, int maximumHammingDistance)
    where T : IGridLine =>
    entries
        .Zip(entries[1..])
        .Where(
            pair => HammingDistance(pair.Item1.Value, pair.Item2.Value) <= maximumHammingDistance
        )
        .ToList();

(int, List<(T, T)>) CountIfDuplicatesAreMirrored<T>(
    List<(T, T)> duplicates,
    List<T> entries,
    int multiplier,
    int expectedHammingDistance
)
    where T : IGridLine
{
    var count = 0;
    var countedAsMirrored = new List<(T, T)>();

    foreach (var (entry, otherEntry) in duplicates)
    {
        var indexOfPreviousValue = entry.Index - 1;
        var indexOfNextValue = otherEntry.Index + 1;
        var iterations = 0;

        // set initial hamming distance, if the mirror entries are not the same the difference starts at 1
        var hammingDistance = entry.Value == otherEntry.Value ? 0 : 1;
        while (indexOfPreviousValue >= 0 && indexOfNextValue < entries.Count)
        {
            iterations++;

            hammingDistance += HammingDistance(
                entries[indexOfPreviousValue].Value,
                entries[indexOfNextValue].Value
            );

            if (hammingDistance > expectedHammingDistance)
            {
                break;
            }

            indexOfPreviousValue--;
            indexOfNextValue++;
        }

        // if hamming distance is not equal to the expected hamming distance (i.e. if there is more or less than one smudge)
        // we discard this duplicate
        if (hammingDistance != expectedHammingDistance && iterations > 0)
        {
            continue;
        }

        count = otherEntry.Index * multiplier;
        countedAsMirrored.Add((entry, otherEntry));
        break;
    }

    return (count, countedAsMirrored);
}

int HammingDistance(string a, string b)
{
    if (a.Length != b.Length)
    {
        throw new ArgumentException("Strings must be of equal length");
    }

    var distance = 0;
    for (var i = 0; i < a.Length; i++)
    {
        if (a[i] != b[i])
        {
            distance++;
        }
    }
    return distance;
}

public interface IGridLine
{
    public int Index { get; set; }
    public string Value { get; set; }
}

record Row(int Index, string Value) : IGridLine
{
    public int Index { get; set; } = Index;
    public string Value { get; set; } = Value;
}

record Column(int Index, string Value) : IGridLine
{
    public int Index { get; set; } = Index;
    public string Value { get; set; } = Value;
}

record Pattern(
    List<Row> Rows,
    Dictionary<string, List<Row>> RowsByValue,
    List<Column> Columns,
    Dictionary<string, List<Column>> ColumnsByValue
);
