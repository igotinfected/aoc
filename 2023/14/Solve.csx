using System.Collections.Immutable;

Solve();

void Solve()
{
    var input = File.ReadAllLines(@"input.txt");
    var (grid, roundRockCoordinates) = ParseGrid(input);

    SolvePartOne(grid, roundRockCoordinates);
    SolvePartTwo(grid, roundRockCoordinates);
}

void SolvePartOne(char[][] grid, List<Coordinate> roundRockCoordinates)
{
    var copyOfGrid = grid.Select(line => line.ToArray()).ToArray();
    (copyOfGrid, var newRoundRockCoordinates) = ShiftRoundRocks(copyOfGrid, roundRockCoordinates, ShiftDirection.North);
    Console.WriteLine($"Part one: {CalculateRoundRocksWeight(newRoundRockCoordinates, copyOfGrid.Length)}");
}

void SolvePartTwo(char[][] grid, List<Coordinate> roundRockCoordinates)
{
    Dictionary<string, int> seen = new();
    var iterations = 1_000_000_000;
    while (iterations > 0)
    {
        seen[GridToString(grid)] = 1_000_000_000 - iterations;
        (grid, roundRockCoordinates) = ShiftRoundRocks(grid, roundRockCoordinates, ShiftDirection.North);
        (grid, roundRockCoordinates) = ShiftRoundRocks(grid, roundRockCoordinates, ShiftDirection.West);
        (grid, roundRockCoordinates) = ShiftRoundRocks(grid, roundRockCoordinates, ShiftDirection.South);
        (grid, roundRockCoordinates) = ShiftRoundRocks(
            grid,
            roundRockCoordinates,
            ShiftDirection.East
        );
        iterations--;

        if (seen.Keys.Contains(GridToString(grid)) is false)
        {
            continue;
        }

        var remainingIterations = iterations % (1_000_000_000 - iterations - seen[GridToString(grid)]);
        for (int i = 0; i < remainingIterations; i++)
        {
            (grid, roundRockCoordinates) = ShiftRoundRocks(grid, roundRockCoordinates, ShiftDirection.North);
            (grid, roundRockCoordinates) = ShiftRoundRocks(grid, roundRockCoordinates, ShiftDirection.West);
            (grid, roundRockCoordinates) = ShiftRoundRocks(grid, roundRockCoordinates, ShiftDirection.South);
            (grid, roundRockCoordinates) = ShiftRoundRocks(grid, roundRockCoordinates, ShiftDirection.East);
        }

        break;
    }

    var weight = CalculateRoundRocksWeight(roundRockCoordinates, grid.Length);
    Console.WriteLine($"Part two: {weight}");
}

(char[][], List<Coordinate>) ParseGrid(string[] input)
{
    var grid = input.Select(line => line.ToCharArray()).ToArray();
    var roundRockCoordinates = new List<Coordinate>();
    for (int y = 0; y < grid.Length; y++)
    {
        for (int x = 0; x < grid[0].Length; x++)
        {
            if (grid[y][x] == 'O')
            {
                roundRockCoordinates.Add(new Coordinate(x, y));
            }
        }
    }
    return (grid, roundRockCoordinates);
}

(char[][], List<Coordinate>) ShiftRoundRocks(
    char[][] grid,
    List<Coordinate> roundRockCoordinates,
    ShiftDirection direction)
{
    var newRoundRockCoordinates = new List<Coordinate>();
    var sortedRoundedCoordinates = direction switch
    {
        ShiftDirection.North => roundRockCoordinates
            .OrderBy(coordinate => coordinate.Y)
            .ToList(),
        ShiftDirection.West => roundRockCoordinates
            .OrderBy(coordinate => coordinate.X)
            .ToList(),
        ShiftDirection.South => roundRockCoordinates
            .OrderByDescending(coordinate => coordinate.Y)
            .ToList(),
        ShiftDirection.East => roundRockCoordinates
            .OrderByDescending(coordinate => coordinate.X)
            .ToList(),
        _ => throw new Exception("Unknown direction")
    };

    foreach (var coordinate in sortedRoundedCoordinates)
    {
        var x = coordinate.X;
        var y = coordinate.Y;

        var newCoordinate = direction switch
        {
            ShiftDirection.North => ShiftRoundRockNorth(coordinate, grid),
            ShiftDirection.West => ShiftRoundRockWest(coordinate, grid),
            ShiftDirection.South => ShiftRoundRockSouth(coordinate, grid),
            ShiftDirection.East => ShiftRoundRockEast(coordinate, grid),
            _ => throw new Exception("Unknown direction")
        };

        if (newCoordinate.X != x || newCoordinate.Y != y)
        {
            grid[y][x] = '.';
            grid[newCoordinate.Y][newCoordinate.X] = 'O';
        }

        newRoundRockCoordinates.Add(newCoordinate);
    }

    return (grid, newRoundRockCoordinates);

    Coordinate ShiftRoundRockNorth(Coordinate coordinate, char[][] grid)
    {
        var x = coordinate.X;
        var y = coordinate.Y;

        while (y > 0 && grid[y - 1][x] == '.')
        {
            y--;
        }

        return y == coordinate.Y ? coordinate : new Coordinate(x, y);
    }
    Coordinate ShiftRoundRockWest(Coordinate coordinate, char[][] grid)
    {
        var x = coordinate.X;
        var y = coordinate.Y;

        while (x > 0 && grid[y][x - 1] == '.')
        {
            x--;
        }

        return x == coordinate.X ? coordinate : new Coordinate(x, y);
    }
    Coordinate ShiftRoundRockSouth(Coordinate coordinate, char[][] grid)
    {
        var x = coordinate.X;
        var y = coordinate.Y;

        while (y < grid.Length - 1 && grid[y + 1][x] == '.')
        {
            y++;
        }

        return y == coordinate.Y ? coordinate : new Coordinate(x, y);
    }
    Coordinate ShiftRoundRockEast(Coordinate coordinate, char[][] grid)
    {
        var x = coordinate.X;
        var y = coordinate.Y;

        while (x < grid[0].Length - 1 && grid[y][x + 1] == '.')
        {
            x++;
        }

        return x == coordinate.X ? coordinate : new Coordinate(x, y);
    }
}

long CalculateRoundRocksWeight(List<Coordinate> roundRockCoordinates, int heightOfGrid) =>
    roundRockCoordinates.Sum(coordinate => heightOfGrid - coordinate.Y);

string GridToString(char[][] grid) => string.Join("", grid.Select(line => string.Join("", line)));

enum ShiftDirection
{
    North,
    West,
    South,
    East
}

record Coordinate(int X, int Y);
