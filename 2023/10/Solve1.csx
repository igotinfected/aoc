#nullable enable

var input = File.ReadAllLines(@"input.txt");
var map = new Dictionary<Coordinate, Tile>();
var finalStepsMap = new Dictionary<Coordinate, int>();

var result = Solve();
Console.WriteLine($"Farthest tile: {result}");

int Solve()
{
    // generate map
    input
        .SelectMany((row, y) => row.Select((tile, x) => (new Coordinate(x, y), tile)))
        .ToList()
        .ForEach(x => map.Add(x.Item1, (Tile)x.Item2));

    // update start tile
    var startTile = map.First(x => x.Value == Tile.Start);
    map[startTile.Key] = DetermineRealTileForStartTile(startTile.Key);

    // find actual path
    MoveUntilBackAtStartInBothDirections(startTile.Key);

    // find farthest tile
    return finalStepsMap.Max(x => x.Value);
}

void MoveUntilBackAtStartInBothDirections(Coordinate startTile)
{
    var (firstDirection, secondDirection) = DetermineDirectionsForTile(map[startTile]);
    var stepsMaps = new List<Dictionary<Coordinate, int>>
    {
        MoveUntilBackAtStart(startTile, firstDirection.Item1, firstDirection.Item2),
        MoveUntilBackAtStart(startTile, secondDirection.Item1, secondDirection.Item2)
    };

    finalStepsMap = stepsMaps
        .SelectMany(x => x)
        .GroupBy(x => x.Key)
        .ToDictionary(x => x.Key, x => x.Min(y => y.Value));
}

Coordinate? Move(
    Coordinate coordinate,
    Tile tile,
    int verticalDirection,
    int horizontalDirection
) =>
    tile switch
    {
        Tile.Vertical => coordinate with { Y = coordinate.Y + verticalDirection },
        Tile.Horizontal => coordinate with { X = coordinate.X + horizontalDirection },
        Tile.NorthEast
            => coordinate with
            {
                X = verticalDirection == 1 ? coordinate.X + 1 : coordinate.X,
                Y = horizontalDirection == -1 ? coordinate.Y - 1 : coordinate.Y
            },
        Tile.NorthWest
            => coordinate with
            {
                X = verticalDirection == 1 ? coordinate.X - 1 : coordinate.X,
                Y = horizontalDirection == 1 ? coordinate.Y - 1 : coordinate.Y
            },
        Tile.SouthWest
            => coordinate with
            {
                X = verticalDirection == -1 ? coordinate.X - 1 : coordinate.X,
                Y = horizontalDirection == 1 ? coordinate.Y + 1 : coordinate.Y
            },
        Tile.SouthEast
            => coordinate with
            {
                X = verticalDirection == -1 ? coordinate.X + 1 : coordinate.X,
                Y = horizontalDirection == -1 ? coordinate.Y + 1 : coordinate.Y
            },
        Tile.Ground => null,
        _ => throw new Exception($"Unknown tile: {tile}")
    };

Dictionary<Coordinate, int> MoveUntilBackAtStart(
    Coordinate startCoordinate,
    int initialVerticalDirection,
    int initialHorizontalDirection
)
{
    var stepsMap = new Dictionary<Coordinate, int>();
    var currentCoordinate = startCoordinate;
    var currentTile = map[currentCoordinate];
    var steps = 0;
    var verticalDirection = initialVerticalDirection;
    var horizontalDirection = initialHorizontalDirection;

    Console.WriteLine($"Start: {startCoordinate}");
    do
    {
        stepsMap.Add(currentCoordinate, steps);
        steps++;

        var previousCoordinate = currentCoordinate;
        currentCoordinate =
            Move(currentCoordinate, currentTile, verticalDirection, horizontalDirection)
            ?? throw new Exception("Cannot move");
        currentTile = map[currentCoordinate];

        // update directions
        verticalDirection = currentCoordinate.Y - previousCoordinate.Y;
        horizontalDirection = currentCoordinate.X - previousCoordinate.X;
    } while (currentCoordinate.X != startCoordinate.X || currentCoordinate.Y != startCoordinate.Y);

    return stepsMap;
}

Tile DetermineRealTileForStartTile(Coordinate startTileCoordinate)
{
    var (left, right, up, down) = (
        map.TryGetValue(
            startTileCoordinate with
            {
                X = startTileCoordinate.X - 1
            },
            out var leftTile
        )
            ? leftTile
            : Tile.Ground,
        map.TryGetValue(
            startTileCoordinate with
            {
                X = startTileCoordinate.X + 1
            },
            out var rightTile
        )
            ? rightTile
            : Tile.Ground,
        map.TryGetValue(startTileCoordinate with { Y = startTileCoordinate.Y - 1 }, out var upTile)
            ? upTile
            : Tile.Ground,
        map.TryGetValue(
            startTileCoordinate with
            {
                Y = startTileCoordinate.Y + 1
            },
            out var downTile
        )
            ? downTile
            : Tile.Ground
    );
    return (left, right, up, down) switch
    {
        (Tile.Horizontal, Tile.Horizontal, _, _) => Tile.Horizontal,
        (_, _, Tile.Vertical, Tile.Vertical) => Tile.Vertical,
        (
            Tile.Horizontal
                or Tile.SouthEast
                or Tile.NorthEast,
            _,
            Tile.Vertical
                or Tile.SouthEast
                or Tile.SouthWest,
            _
        )
            => Tile.NorthEast,
        (
            _,
            Tile.Horizontal
                or Tile.SouthWest
                or Tile.NorthWest,
            Tile.Vertical
                or Tile.SouthEast
                or Tile.SouthWest,
            _
        )
            => Tile.NorthWest,
        (
            Tile.Horizontal
                or Tile.SouthEast
                or Tile.NorthEast,
            _,
            _,
            Tile.Vertical
                or Tile.NorthEast
                or Tile.NorthWest
        )
            => Tile.SouthWest,
        (
            _,
            Tile.Horizontal
                or Tile.SouthWest
                or Tile.NorthWest,
            _,
            Tile.Vertical
                or Tile.NorthEast
                or Tile.NorthWest
        )
            => Tile.SouthEast,
        _ => throw new Exception("Cannot determine start tile")
    };
}

((int, int), (int, int)) DetermineDirectionsForTile(Tile tile)
    => tile switch
    {
        Tile.Vertical => ((0, 1), (0, -1)),
        Tile.Horizontal => ((1, 0), (-1, 0)),
        Tile.NorthEast => ((-1, 0), (0, 1)),
        Tile.NorthWest => ((0, 1), (1, 0)),
        Tile.SouthWest => ((-1, 0), (0, 1)),
        Tile.SouthEast => ((0, -1), (-1, 0)),
        _ => throw new Exception($"Unexpected tile: {tile}")
    };

record Coordinate(int X, int Y);
enum Tile
{
    Ground = '.',
    Vertical = '|',
    Horizontal = '-',
    NorthEast = 'L',
    NorthWest = 'J',
    SouthWest = '7',
    SouthEast = 'F',
    Start = 'S'
}
