using System;
using System.Linq;

var maxCubesPerColorMapping = new Dictionary<string, int>()
{
    { "blue", 14 },
    { "green", 13 },
    { "red", 12 }
};

Console.WriteLine(
    File.ReadLines("test1.txt")
        .Sum(game =>
        {
            var gameId = int.Parse(game.Split(':')[0].Split(' ')[^1]);
            return game.Split(':')[1]
                .Split(';')
                .Select(
                    round =>
                        round
                            .Split(',')
                            .All(
                                part =>
                                    int.Parse(part.Trim(' ').Split(' ')[0])
                                    <= maxCubesPerColorMapping[part.Trim(' ').Split(' ')[1]]
                            )
                )
                .All(isRoundPossible => isRoundPossible)
                ? gameId
                : 0;
        })
);
