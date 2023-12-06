using System;

var input = File.ReadAllLines("input.txt");

record Coordinate(int X, int Y);

const char GearRatioSymbol = '*';

List<Coordinate> GenerateAdjacentAndDiagonalCoordinates(Coordinate coordinate)
{
  var result = new List<Coordinate>();
  for (int x = coordinate.X - 1; x <= coordinate.X + 1; x++)
  {
    for (int y = coordinate.Y - 1; y <= coordinate.Y + 1; y++)
    {
      if (x == coordinate.X && y == coordinate.Y)
      {
        continue;
      }
      result.Add(new Coordinate(x, y));
    }
  }
  return result;
}

List<Coordinate> GenerateCoordinatesFromStartAndEndIndex(int startX, int endX, int startY, int endY)
{
  var result = new List<Coordinate>();
  if (startY < endY)
  {
    for (int x = startX; x < input[startY].Length; x++)
    {
      result.Add(new Coordinate(x, startY));
    }
    for (int x = 0; x < endX; x++)
    {
      result.Add(new Coordinate(x, endY));
    }
  }
  else
  {
    for (int x = startX; x < endX; x++)
    {
      result.Add(new Coordinate(x, startY));
    }
  }
  return result;
}

bool DoCoordinateListsIntersect(List<Coordinate> list1, List<Coordinate> list2)
{
  return list1.Any(coordinate => list2.Any(coordinate2 => coordinate.X == coordinate2.X && coordinate.Y == coordinate2.Y));
}

(int endX, int endY) FindEndOfNumberIn2DArray(int startX, int y, string[] lines)
{
  var currentX = startX;
  var currentY = y;
  while (currentX < lines[currentY].Length && Char.IsDigit(lines[currentY][currentX]))
  {
    if (currentX == lines[currentY].Length - 1)
    {
      currentY++;
      currentX = 0;
    }
    currentX++;
  }
  return (currentX, currentY);
}
var numbers = new Dictionary<List<Coordinate>, int>();
var symbols = new Dictionary<List<Coordinate>, char>();

for (var y = 0; y < input.Length; y++)
{
  for (var x = 0; x < input[y].Length; x++)
  {
    if (input[y][x] == '.')
    {
      continue;
    }
    else if (Char.IsDigit(input[y][x]) is false)
    {
      symbols[GenerateAdjacentAndDiagonalCoordinates(new Coordinate(x, y))] = input[y][x];
      continue;
    }
    else
    {
      var (endOfNumberX, endOfNumberY) = FindEndOfNumberIn2DArray(x, y, input);
      if (endOfNumberY > y)
      {
        var substring = input[y].Substring(x) + (input[endOfNumberY].Substring(0, endOfNumberX - 1));
        numbers[GenerateCoordinatesFromStartAndEndIndex(x, endOfNumberX, y, endOfNumberY)] = int.Parse(substring);
        x = endOfNumberX - 1;
        y = endOfNumberY;
      }
      else
      {
        numbers[GenerateCoordinatesFromStartAndEndIndex(x, endOfNumberX, y, endOfNumberY)] = int.Parse(input[y].Substring(x, endOfNumberX - x));
        x = endOfNumberX - 1;
      }
    }
  }
}

var result = symbols
  .Where(symbol => symbol.Value == GearRatioSymbol)
  .Select(symbol =>
  {
    var intersectingNumbers = numbers.Where(kvp => DoCoordinateListsIntersect(kvp.Key, symbol.Key)).Select(x => x.Value).ToList();
    return intersectingNumbers.Count == 2 ? intersectingNumbers.Aggregate((x, y) => x * y) : 0;
  })
  .Sum();
Console.WriteLine($"Result: {result}");
