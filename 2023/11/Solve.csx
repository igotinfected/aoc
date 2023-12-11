using System.Text.Json;

var input = File.ReadAllLines(@"input.txt");

Solve();

void Solve()
{
    var universe = GenerateUniverse();

    SolvePartOne(universe);
    SolvePartTwo(universe);
}

void SolvePartOne(List<List<Coordinate>> universe)
{
    var expansionOffset = 1;
    var expandedUniverse = ExpandUniverse(universe, expansionOffset);
    var galaxies = GetAllGalaxies(expandedUniverse);
    var galaxyPairs = GetAllDistinctGalaxyPairs(galaxies);
    var galaxyPairDistances = galaxyPairs.Select(
        pair => DistanceBetweenPoints(pair.Item1, pair.Item2)
    );
    var sumOfDistances = galaxyPairDistances.Sum();
    Console.WriteLine($"Result for part 1: {sumOfDistances}");
}

void SolvePartTwo(List<List<Coordinate>> universe)
{
    var expansionOffset = 999_999;
    var expandedUniverse = ExpandUniverse(universe, expansionOffset);
    var galaxies = GetAllGalaxies(expandedUniverse);
    var galaxyPairs = GetAllDistinctGalaxyPairs(galaxies);
    var galaxyPairDistances = galaxyPairs.Select(
        pair => DistanceBetweenPoints(pair.Item1, pair.Item2)
    );
    var sumOfDistances = galaxyPairDistances.Sum();
    Console.WriteLine($"Result for part 2: {sumOfDistances}");
}

List<List<Coordinate>> GenerateUniverse() =>
    input
        .Select(
            (line, y) =>
                line.Select((character, x) => new Coordinate(x, y, 0, 0, character)).ToList()
        )
        .ToList();

List<List<Coordinate>> ExpandUniverse(List<List<Coordinate>> originalUniverse, int expansionOffset)
{
    var universe = JsonSerializer.Deserialize<List<List<Coordinate>>>(
        JsonSerializer.Serialize(originalUniverse)
    );

    for (var i = 0; i < universe.Count; i++)
    {
        if (universe[i].All(coordinate => coordinate.Value == '.') is false)
        {
            continue;
        }

        universe = OffsetUniverseRows(universe, i, expansionOffset);
    }

    for (var i = 0; i < universe[0].Count; i++)
    {
        if (universe.All(row => row[i].Value == '.') is false)
        {
            continue;
        }

        universe = OffsetUniverseColumns(universe, i, expansionOffset);
    }

    return universe;
}

List<List<Coordinate>> OffsetUniverseRows(
    List<List<Coordinate>> universe,
    int rowIndex,
    int expansionOffset
) =>
    universe
        .Select(
            (row, index) =>
                index > rowIndex
                    ? row.Select(
                            coordinate =>
                                coordinate with
                                {
                                    ExpansionOffsetY = coordinate.ExpansionOffsetY + expansionOffset
                                }
                        )
                        .ToList()
                    : row
        )
        .ToList();

List<List<Coordinate>> OffsetUniverseColumns(
    List<List<Coordinate>> universe,
    int columnIndex,
    int expansionOffset
) =>
    universe
        .Select(
            row =>
                row.Select(
                        (coordinate, index) =>
                            index > columnIndex
                                ? coordinate with
                                {
                                    ExpansionOffsetX = coordinate.ExpansionOffsetX + expansionOffset
                                }
                                : coordinate
                    )
                    .ToList()
        )
        .ToList();

List<Coordinate> GetAllGalaxies(List<List<Coordinate>> universe) =>
    universe.SelectMany(row => row).Where(coordinate => coordinate.Value == '#').ToList();

List<(Coordinate, Coordinate)> GetAllDistinctGalaxyPairs(List<Coordinate> galaxies) =>
    galaxies
        .SelectMany(
            (galaxy1, galaxy1Index) =>
                galaxies.Skip(galaxy1Index + 1).Select(galaxy2 => (galaxy1, galaxy2))
        )
        .ToList();

long DistanceBetweenPoints(Coordinate point1, Coordinate point2) =>
    Math.Abs(point1.X + point1.ExpansionOffsetX - point2.X - point2.ExpansionOffsetX)
    + Math.Abs(point1.Y + point1.ExpansionOffsetY - point2.Y - point2.ExpansionOffsetY);

record Coordinate(int X, int Y, int ExpansionOffsetX, int ExpansionOffsetY, char Value);
