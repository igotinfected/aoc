using System.Collections.Immutable;

Solve();

void Solve()
{
    var input = File.ReadAllLines(@"input.txt");
    var grid = ParseGrid(input);

    Dictionary<string, int> seen = new();
    var iterations = 1_000_000_000L;
    while (iterations > 0)
    {
        seen[GridAsString(grid)] = 1_000_000_000 - (int)iterations;
        Console.WriteLine($"Iteration: {1_000_000_000 - iterations}");
        iterations--;
        grid = ShiftRoundRocksUp(grid);
        grid = ShiftRoundRocksLeft(grid);
        grid = ShiftRoundRocksDown(grid);
        grid = ShiftRoundRocksRight(grid);
        if (seen.Keys.Contains(GridAsString(grid)))
        {
            Console.WriteLine($"Found duplicate after {1_000_000_000 - iterations} iterations");
            // cycle was detected so calculate how many iterations we have to do aft accounting for the cycle
            var cycleLength = seen[GridAsString(grid)];
            var remainingIterations = iterations % cycleLength;
            Console.WriteLine($"Remaining iterations: {remainingIterations}");

            for (int i = 0; i < remainingIterations; i++)
            {
                grid = ShiftRoundRocksUp(grid);
                grid = ShiftRoundRocksLeft(grid);
                grid = ShiftRoundRocksDown(grid);
                grid = ShiftRoundRocksRight(grid);
            }
            break;
        }
    }
    var weight = CalculateRoundRocksWeight(grid);
    Console.WriteLine($"Weight: {weight}");
}

string GridAsString(Grid grid)
{
    var coordinates = grid.Coordinates;
    var width = grid.Width;
    var height = grid.Height;
    var gridAsString = "";
    for (int y = 0; y < height; y++)
    {
        var line = "";
        for (int x = 0; x < width; x++)
        {
            var coordinate = coordinates.FirstOrDefault(c => c.X == x && c.Y == y);
            line += coordinate.Type switch
            {
                FieldType.Empty => '.',
                FieldType.RoundRock => 'O',
                FieldType.SquareRock => '#',
                _ => throw new Exception($"Unknown field type {coordinate.Type}")
            };
        }
        gridAsString += line + "\n";
    }
    return gridAsString;
}

void DrawGrid(Grid grid)
{
    var coordinates = grid.Coordinates;
    var width = grid.Width;
    var height = grid.Height;
    for (int y = 0; y < height; y++)
    {
        var line = "";
        for (int x = 0; x < width; x++)
        {
            var coordinate = coordinates.FirstOrDefault(c => c.X == x && c.Y == y);
            line += coordinate.Type switch
            {
                FieldType.Empty => '.',
                FieldType.RoundRock => 'O',
                FieldType.SquareRock => '#',
                _ => throw new Exception($"Unknown field type {coordinate.Type}")
            };
        }
        Console.WriteLine(line);
    }
    Console.WriteLine();
}

long GCD(long a, long b) => b == 0 ? a : GCD(b, a % b);

Grid ParseGrid(string[] input)
{
    var width = input[0].Length;
    var height = input.Length;
    var coordinates = new List<Coordinate>();
    for (int y = 0; y < height; y++)
    {
        var line = input[y];
        for (int x = 0; x < width; x++)
        {
            var type = line[x] switch
            {
                '.' => FieldType.Empty,
                'O' => FieldType.RoundRock,
                '#' => FieldType.SquareRock,
                _ => throw new Exception($"Unknown field type {line[x]}")
            };
            coordinates.Add(new Coordinate(x, y, type));
        }
    }
    return new Grid(width, height, coordinates);
}

Grid ShiftRoundRocksUp(Grid grid)
{
    var coordinates = grid.Coordinates;
    var width = grid.Width;
    var height = grid.Height;

    for (int i = 1; i < grid.Height; i++)
    {
        var rocksInRow = coordinates.Where(c => c.Y == i && c.Type == FieldType.RoundRock).ToList();
        foreach (var rock in rocksInRow)
        {
            var coordinatesAbove = coordinates
                .Where(c => c.X == rock.X && c.Y < rock.Y)
                .OrderByDescending(c => c.Y)
                .ToList();
            var blockingCoordinate = coordinatesAbove.FirstOrDefault(
                c => c.Type == FieldType.SquareRock || c.Type == FieldType.RoundRock
            );
            if (blockingCoordinate is not null)
            {
                var coordinateToSwapWith = coordinatesAbove.FirstOrDefault(
                    c => c.Y == blockingCoordinate.Y + 1,
                    rock
                );
                coordinates.Remove(coordinateToSwapWith);
                coordinates.Remove(rock);
                coordinates.Add(
                    new Coordinate(rock.X, coordinateToSwapWith.Y, FieldType.RoundRock)
                );
                coordinates.Add(new Coordinate(rock.X, rock.Y, FieldType.Empty));
                continue;
            }
            coordinates.Remove(coordinatesAbove.Last());
            coordinates.Remove(rock);
            coordinates.Add(new Coordinate(rock.X, coordinatesAbove.Last().Y, FieldType.RoundRock));
            coordinates.Add(new Coordinate(rock.X, rock.Y, FieldType.Empty));
        }
    }
    return new Grid(width, height, coordinates);
}

Grid ShiftRoundRocksDown(Grid grid)
{
    var coordinates = grid.Coordinates;
    var width = grid.Width;
    var height = grid.Height;

    for (int i = grid.Height - 2; i >= 0; i--)
    {
        var rocksInRow = coordinates.Where(c => c.Y == i && c.Type == FieldType.RoundRock).ToList();
        foreach (var rock in rocksInRow)
        {
            var coordinatesAbove = coordinates
                .Where(c => c.X == rock.X && c.Y > rock.Y)
                .OrderBy(c => c.Y)
                .ToList();
            var blockingCoordinate = coordinatesAbove.FirstOrDefault(
                c => c.Type == FieldType.SquareRock || c.Type == FieldType.RoundRock
            );
            if (blockingCoordinate is not null)
            {
                var coordinateToSwapWith = coordinatesAbove.FirstOrDefault(
                    c => c.Y == blockingCoordinate.Y - 1,
                    rock
                );
                coordinates.Remove(coordinateToSwapWith);
                coordinates.Remove(rock);
                coordinates.Add(
                    new Coordinate(rock.X, coordinateToSwapWith.Y, FieldType.RoundRock)
                );
                coordinates.Add(new Coordinate(rock.X, rock.Y, FieldType.Empty));
                continue;
            }
            coordinates.Remove(coordinatesAbove.Last());
            coordinates.Remove(rock);
            coordinates.Add(new Coordinate(rock.X, coordinatesAbove.Last().Y, FieldType.RoundRock));
            coordinates.Add(new Coordinate(rock.X, rock.Y, FieldType.Empty));
        }
    }
    return new Grid(width, height, coordinates);
}

Grid ShiftRoundRocksLeft(Grid grid)
{
    var coordinates = grid.Coordinates;
    var width = grid.Width;
    var height = grid.Height;

    for (int i = 1; i < grid.Width; i++)
    {
        var rocksInColumn = coordinates
            .Where(c => c.X == i && c.Type == FieldType.RoundRock)
            .ToList();
        foreach (var rock in rocksInColumn)
        {
            var coordinatesLeft = coordinates
                .Where(c => c.Y == rock.Y && c.X < rock.X)
                .OrderByDescending(c => c.X)
                .ToList();
            var blockingCoordinate = coordinatesLeft.FirstOrDefault(
                c => c.Type == FieldType.SquareRock || c.Type == FieldType.RoundRock
            );
            if (blockingCoordinate is not null)
            {
                var coordinateToSwapWith = coordinatesLeft.FirstOrDefault(
                    c => c.X == blockingCoordinate.X + 1,
                    rock
                );
                coordinates.Remove(coordinateToSwapWith);
                coordinates.Remove(rock);
                coordinates.Add(
                    new Coordinate(coordinateToSwapWith.X, rock.Y, FieldType.RoundRock)
                );
                coordinates.Add(new Coordinate(rock.X, rock.Y, FieldType.Empty));
                continue;
            }
            coordinates.Remove(coordinatesLeft.Last());
            coordinates.Remove(rock);
            coordinates.Add(new Coordinate(coordinatesLeft.Last().X, rock.Y, FieldType.RoundRock));
            coordinates.Add(new Coordinate(rock.X, rock.Y, FieldType.Empty));
        }
    }
    return new Grid(width, height, coordinates);
}

Grid ShiftRoundRocksRight(Grid grid)
{
    var coordinates = grid.Coordinates;
    var width = grid.Width;
    var height = grid.Height;

    for (int i = grid.Width - 2; i >= 0; i--)
    {
        var rocksInColumn = coordinates
            .Where(c => c.X == i && c.Type == FieldType.RoundRock)
            .ToList();
        foreach (var rock in rocksInColumn)
        {
            var coordinatesRight = coordinates
                .Where(c => c.Y == rock.Y && c.X > rock.X)
                .OrderBy(c => c.X)
                .ToList();
            var blockingCoordinate = coordinatesRight.FirstOrDefault(
                c => c.Type == FieldType.SquareRock || c.Type == FieldType.RoundRock
            );
            if (blockingCoordinate is not null)
            {
                var coordinateToSwapWith = coordinatesRight.FirstOrDefault(
                    c => c.X == blockingCoordinate.X - 1,
                    rock
                );
                coordinates.Remove(coordinateToSwapWith);
                coordinates.Remove(rock);
                coordinates.Add(
                    new Coordinate(coordinateToSwapWith.X, rock.Y, FieldType.RoundRock)
                );
                coordinates.Add(new Coordinate(rock.X, rock.Y, FieldType.Empty));
                continue;
            }
            coordinates.Remove(coordinatesRight.Last());
            coordinates.Remove(rock);
            coordinates.Add(new Coordinate(coordinatesRight.Last().X, rock.Y, FieldType.RoundRock));
            coordinates.Add(new Coordinate(rock.X, rock.Y, FieldType.Empty));
        }
    }
    return new Grid(width, height, coordinates);
}

long CalculateRoundRocksWeight(Grid grid)
{
    var coordinates = grid.Coordinates;
    var width = grid.Width;
    var height = grid.Height;
    var weight = 1L;
    var totalWeight = 0L;
    for (int y = grid.Height - 1; y >= 0; y--)
    {
        var rocksInRow = coordinates.Where(c => c.Y == y && c.Type == FieldType.RoundRock).ToList();
        foreach (var rock in rocksInRow)
        {
            totalWeight += weight;
        }
        weight++;
    }
    return totalWeight;
}

record Grid(int Width, int Height, List<Coordinate> Coordinates);

record Coordinate(int X, int Y, FieldType Type);

enum FieldType
{
    Empty = '.',
    RoundRock = 'O',
    SquareRock = '#'
}
