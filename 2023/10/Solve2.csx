#nullable enable

const float INACCURACY = 0.000001f;
var input = File.ReadAllLines(@"input.txt");
var map = new Dictionary<Coordinate, Tile>();

var result = Solve();
Console.WriteLine($"# of coordinates in loop: {result}");

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
    var (firstDirection, _) = DetermineDirectionsForTile(map[startTile.Key]);
    var pathMap = MoveUntilBackAtStart(startTile.Key, firstDirection.Item1, firstDirection.Item2);

    // find all coordinates inside of loop
    var candidateCoordinates = map.Keys.Except(pathMap.Keys).ToList();
    return candidateCoordinates.Count(coordinate => IsCoordinateInPolygon(coordinate, pathMap.Keys.ToList()));
}

bool IsCoordinateInPolygon(Coordinate coordinate, List<Coordinate> polygon)
{
    var crossings = CalculateNumberOfCrossings(coordinate, polygon);
    return crossings % 2 == 1;
}

/// <summary>
/// <para>
/// Based on the Jordan Curve Theorem that states that a simple closed curve
/// divides the plane into exactly two regions, the inside and the outside.
/// </para>
/// <para>
/// The implication is that any ray cast from a point inside of the polygon in any direction
/// will intersect the polygon an odd number of times.
/// </para>
/// <para>
/// In this case we are casting a ray from the coordinate in the negative Y direction,
/// i.e. a ray pointing straight up.
/// </para>
/// </summary>
/// <param name="coordinate">The coordinate from which a ray will be cast.</param>
/// <param name="polygon">The coordinates that make up the polygon we are testing against.</param>
/// <returns>The number of times a ray cast from the coordinate intersects with the polygon.</returns>
int CalculateNumberOfCrossings(Coordinate coordinate, List<Coordinate> polygon)
{
    var crossings = 0;
    for (var i = 0; i < polygon.Count; i++)
    {
        // to ensure we get the same result if the X coordinates were to be inversed we force coordinate direction
        var (normalisedXFrom, normalisedXTo) =
            polygon[i].X < polygon[(i + 1) % polygon.Count].X
                ? (polygon[i].X, polygon[(i + 1) % polygon.Count].X)
                : (polygon[(i + 1) % polygon.Count].X, polygon[i].X);
        var isCoordinatePotentiallyInPolygon =
            coordinate.X > normalisedXFrom
            && coordinate.X <= normalisedXTo
            && (coordinate.Y < polygon[i].Y || coordinate.Y <= polygon[(i + 1) % polygon.Count].Y);

        if (isCoordinatePotentiallyInPolygon is false)
        {
            continue;
        }

        // straight line equation: y = kx + m
        (float dx, float dy) = (
            polygon[(i + 1) % polygon.Count].X - polygon[i].X,
            polygon[(i + 1) % polygon.Count].Y - polygon[i].Y
        );
        // if dx approaches 0 we use a very large number to emulate infinity
        float k = Math.Abs(dx) < INACCURACY ? float.MaxValue : dy / dx;
        float m = polygon[i].Y - k * polygon[i].X;
        var y = k * coordinate.X + m;

        // if y is greater than or equal to the coordinate's Y value then the ray crosses the line
        if (y >= coordinate.Y)
        {
            crossings++;
        }
    }

    return crossings;
}

Coordinate? Move(
    Coordinate coordinate,
    Tile tile,
    int verticalDirection,
    int horizontalDirection)
    => tile switch
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

Dictionary<Coordinate, Tile> MoveUntilBackAtStart(
    Coordinate startCoordinate,
    int initialVerticalDirection,
    int initialHorizontalDirection)
{
    var pathMap = new Dictionary<Coordinate, Tile>();
    var currentCoordinate = startCoordinate;
    var currentTile = map[currentCoordinate];
    var verticalDirection = initialVerticalDirection;
    var horizontalDirection = initialHorizontalDirection;

    do
    {
        pathMap.Add(currentCoordinate, currentTile);

        var previousCoordinate = currentCoordinate;
        currentCoordinate =
            Move(currentCoordinate, currentTile, verticalDirection, horizontalDirection)
            ?? throw new Exception("Cannot move");
        currentTile = map[currentCoordinate];

        verticalDirection = currentCoordinate.Y - previousCoordinate.Y;
        horizontalDirection = currentCoordinate.X - previousCoordinate.X;
    } while (currentCoordinate.X != startCoordinate.X || currentCoordinate.Y != startCoordinate.Y);

    return pathMap;
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
record Edge(Coordinate From, Coordinate To);
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
