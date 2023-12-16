#nullable enable
using System.Collections.Concurrent;

Solve();

void Solve()
{
    var input = File.ReadAllLines(@"input.txt");

    SolvePartOne(input);
    SolvePartTwo(input);
}

void SolvePartOne(string[] grid)
{
    var startingBeam = new Beam(new Coordinate(0, 0), 1, 0);
    var result = FindNumberOfEnergizedTiles(startingBeam, grid).Count;
    Console.WriteLine($"Part 1: {result}");
}

void SolvePartTwo(string[] grid)
{
    var startingBeams = new HashSet<Beam>();
    for (var i = 0; i < grid[0].Length; i++)
    {
        startingBeams.Add(new Beam(new Coordinate(i, 0), 0, 1));
        startingBeams.Add(new Beam(new Coordinate(i, grid.Length - 1), 0, -1));
    }
    for (var i = 0; i < grid.Length; i++)
    {
        startingBeams.Add(new Beam(new Coordinate(0, i), 1, 0));
        startingBeams.Add(new Beam(new Coordinate(grid[0].Length - 1, i), -1, 0));
    }

    var energizedTilesList = new List<HashSet<Coordinate>>();
    Parallel.ForEach(startingBeams, (startingBeam, _, index) =>
    {
        var energizedTiles = FindNumberOfEnergizedTilesV2(startingBeam, grid);

        lock (energizedTilesList)
        {
            energizedTilesList.Add(energizedTiles);
        }
    });

    var result = energizedTilesList.Max(e => e.Count);
    Debug.Assert(result == 8216, "Broke it");
    Console.WriteLine($"Part 2: {result}");
}

(Beam, Beam?) Move(Beam beam, string[] grid)
{
    var currentLocationSymbol = grid[beam.currentLocation.Y][beam.currentLocation.X];

    return currentLocationSymbol switch
    {
        '.' => (GenerateNewBeam(beam, beam.VelocityX, beam.VelocityY), null),
        '-' when beam.VelocityX is 1 or -1 => (GenerateNewBeam(beam, beam.VelocityX, beam.VelocityY), null),
        '|' when beam.VelocityY is 1 or -1 => (GenerateNewBeam(beam, beam.VelocityX, beam.VelocityY), null),
        '/' when beam.VelocityX is 1 or -1 => (GenerateNewBeam(beam, 0, -1 * beam.VelocityX), null),
        '\\' when beam.VelocityX is 1 or -1 => (GenerateNewBeam(beam, 0, beam.VelocityX), null),
        '/' when beam.VelocityY is 1 or -1 => (GenerateNewBeam(beam, -1 * beam.VelocityY, 0), null),
        '\\' when beam.VelocityY is 1 or -1 => (GenerateNewBeam(beam, beam.VelocityY, 0), null),
        '|' when beam.VelocityX is 1 or -1 => (GenerateNewBeam(beam, 0, -1), GenerateNewBeam(beam, 0, 1)),
        '-' when beam.VelocityY is 1 or -1 => (GenerateNewBeam(beam, -1, 0), GenerateNewBeam(beam, 1, 0)),
        _ => throw new InvalidOperationException($"Invalid symbol {currentLocationSymbol} or velocity {beam.VelocityX}, {beam.VelocityY}")
    };
}

Beam GenerateNewBeam(Beam beam, int vx, int vy)
    => new Beam(
            new Coordinate(
                beam.currentLocation.X + vx,
                beam.currentLocation.Y + vy
            ),
            vx,
            vy
        );

HashSet<Coordinate> FindNumberOfEnergizedTiles(Beam startingBeam, string[] grid)
{
    var energizedTiles = new HashSet<Coordinate>() { startingBeam.currentLocation };
    var beamCache = new HashSet<Beam>(new BeamComparer());
    var beams = new Queue<Beam>();
    beams.Enqueue(startingBeam);

    int gridWidth = grid[0].Length;
    int gridHeight = grid.Length;

    while (beams.Count > 0)
    {
        var beam = beams.Dequeue();

        var (newBeam, newBeam2) = Move(beam, grid);
        ProcessBeam(newBeam, gridWidth, gridHeight, energizedTiles, beamCache, beams);

        if (newBeam2 == null)
        {
            continue;
        }

        ProcessBeam(newBeam2, gridWidth, gridHeight, energizedTiles, beamCache, beams);
    }

    return energizedTiles;
}

void ProcessBeam(Beam beam, int gridWidth, int gridHeight, HashSet<Coordinate> energizedTiles, HashSet<Beam> beamCache, Queue<Beam> beams)
{
    if (beam == null
        || beam.currentLocation.X < 0
        || beam.currentLocation.X >= gridWidth
        || beam.currentLocation.Y < 0
        || beam.currentLocation.Y >= gridHeight)
    {
        return;
    }

    energizedTiles.Add(beam.currentLocation);
    if (beamCache.Contains(beam))
    {
        return;
    }

    beams.Enqueue(beam);
    beamCache.Add(beam);
}

record Beam(Coordinate CurrentLocation, int VelocityX, int VelocityY);

class BeamComparer : IEqualityComparer<Beam>
{
    public bool Equals(Beam? x, Beam? y)
        => x?.currentLocation.X == y?.currentLocation.X
            && x?.currentLocation.Y == y?.currentLocation.Y
            && x?.VelocityX == y?.VelocityX
            && x?.VelocityY == y?.VelocityY;

    public int GetHashCode(Beam obj)
        => HashCode.Combine(obj.currentLocation.X, obj.currentLocation.Y, obj.VelocityX, obj.VelocityY);
}

record Coordinate(int X, int Y);
