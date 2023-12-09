var input = File.ReadAllLines(@"input.txt");

var result = input
    .Select(line =>
    {
        var invertedPascalsTriangle = line.Split(' ')
            .Select(stringValue => long.Parse(stringValue))
            .GenerateInvertedPascalsTriangle();

        return GetExtrapolatedValue(invertedPascalsTriangle);
    })
    .Sum();

Console.WriteLine($"Result: {result}");

static List<long[]> GenerateInvertedPascalsTriangle(this IEnumerable<long> initialValues)
{
    long[] initialValueArray = [..initialValues, 0L];
    var invertedPascalsTriangle = new List<long[]> { initialValueArray };
    var areAllValuesInLastRowZero = true;

    do
    {
        var previousRow = invertedPascalsTriangle.Last();
        var currentRow = new long[previousRow.Length - 1];
        areAllValuesInLastRowZero = true;

        for (var i = 0; i < currentRow.Length - 1; i++)
        {
            currentRow[i] = previousRow[i + 1] - previousRow[i];
            areAllValuesInLastRowZero = areAllValuesInLastRowZero && currentRow[i] == 0;
        }

        invertedPascalsTriangle.Add(currentRow);
    } while (areAllValuesInLastRowZero is false);

    return invertedPascalsTriangle;
}

long GetExtrapolatedValue(List<long[]> invertedPascalsTriangle)
{
    for (
        var currentRowIndex = invertedPascalsTriangle.Count - 2;
        currentRowIndex >= 0;
        currentRowIndex--
    )
    {
        invertedPascalsTriangle[currentRowIndex][^1] =
            invertedPascalsTriangle[currentRowIndex + 1][^1]
            + invertedPascalsTriangle[currentRowIndex][^2];
    }

    return invertedPascalsTriangle[0][^1];
}
