using System;

var digitMapping = new Dictionary<string, string>()
{
    { "one", "1" },
    { "two", "2" },
    { "three", "3" },
    { "four", "4" },
    { "five", "5" },
    { "six", "6" },
    { "seven", "7" },
    { "eight", "8" },
    { "nine", "9" }
};

Console.WriteLine(
    File.ReadLines("input.txt")
        .Select(line =>
        {
            var startIndex = 0;
            var digits = string.Empty;
            for (var endIndex = 0; endIndex < line.Length; endIndex++)
            {
                var character = line[endIndex];
                var containedDigit = digitMapping
                    .Keys
                    .Where(line.Substring(startIndex, endIndex - startIndex + 1).Contains)
                    .Select(key => digitMapping[key])
                    .FirstOrDefault();
                var digit = char.IsDigit(character) ? character.ToString() : null;

                if (containedDigit is not null)
                {
                    startIndex = endIndex - 1;
                }
                else if (digit is not null)
                {
                    startIndex = endIndex;
                }
                else
                {
                    continue;
                }
                digits += containedDigit ?? digit;
            }
            return int.Parse($"{digits[0]}{digits[^1]}");
        })
        .Sum()
);
