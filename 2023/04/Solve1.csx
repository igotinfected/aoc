var input = File.ReadAllLines("input.txt");

var result = input
    .Select(line =>
    {
        var scratchCard = line.Split(':', StringSplitOptions.RemoveEmptyEntries)[1].Split(
            '|',
            StringSplitOptions.RemoveEmptyEntries
        );
        var winningNumbers = scratchCard[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var playedNumbers = scratchCard[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var matchedNumbers = winningNumbers.Where(w => playedNumbers.Contains(w)).Count();
        return matchedNumbers switch
        {
            0 => 0,
            1 => 1,
            _ => Math.Pow(2, matchedNumbers - 1)
        };
    })
    .Sum();

Console.WriteLine($"Result: {result}");
