var input = File.ReadAllLines("input.txt");

var scratchCardCopies = new Dictionary<int, int>();

// generate scratch card copies
var index = 0;
input
    .ToList()
    .ForEach(line =>
    {
        scratchCardCopies.TryGetValue(index, out var value);
        scratchCardCopies[index] = value + 1;

        var scratchCard = line.Split(':', StringSplitOptions.RemoveEmptyEntries)[1].Split(
            '|',
            StringSplitOptions.RemoveEmptyEntries
        );
        var winningNumbers = scratchCard[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var playedNumbers = scratchCard[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var matchedNumbers = winningNumbers.Where(w => playedNumbers.Contains(w)).Count();

        var scratchCardValue = matchedNumbers switch
        {
            0 => 0,
            1 => 1,
            _ => Math.Pow(2, matchedNumbers - 1)
        };

        for (var i = 1; i <= matchedNumbers && i < input.Length; i++)
        {
            scratchCardCopies.TryGetValue(index + i, out var nextValue);
            scratchCardCopies[index + i] = nextValue + scratchCardCopies[index];
        }

        index++;
    });

var result = scratchCardCopies.Sum(s => s.Value);
Console.WriteLine($"Result: {result}");
