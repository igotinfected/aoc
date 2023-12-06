using System;
using System.Linq;

var maxCubesPerColorMapping = new Dictionary<string, int>()
{
    { "blue", 14 },
    { "green", 13 },
    { "red", 12 }
};

Console.WriteLine(
    File.ReadLines("input.txt")
        .Sum(game =>
        {
            var gameId = int.Parse(game.Split(':')[0].Split(' ')[^1]);
            var minimumNumberOfCubesPerColor = new Dictionary<string, int>()
            {
                { "blue", 0 },
                { "green", 0 },
                { "red", 0 }
            };
            game.Split(':')[1]
                .Split(';')
                .ToList()
                .ForEach(
                    round =>
                        round
                            .Split(',')
                            .ToList()
                            .ForEach(part =>
                            {
                                var numberOfCubes = int.Parse(part.Trim(' ').Split(' ')[0]);
                                var color = part.Trim(' ').Split(' ')[1];
                                minimumNumberOfCubesPerColor[color] = Math.Max(
                                    minimumNumberOfCubesPerColor[color],
                                    numberOfCubes
                                );
                            })
                );
            return minimumNumberOfCubesPerColor.Values.Aggregate((x, y) => x * y);
        })
);
