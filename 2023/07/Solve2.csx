var hands = File.ReadAllLines(@"input.txt")
    .Select(line => line.Split(' '))
    .Select(parts => new Hand(parts[0], int.Parse(parts[1])));

var sortableHandsWithHandType = new Dictionary<Hand, HandType>();

foreach (var hand in hands)
{
  var characterCount = new Dictionary<char, int>();
  var jokerCount = 0;
  var newHandString = string.Empty;
  for (var i = 0; i < hand.Cards.Length; i++)
  {
    newHandString += GetCorrectedCharacterValue(hand.Cards[i]);
    if (hand.Cards[i] == 'J')
    {
      jokerCount++;
    }
    else if (characterCount.ContainsKey(hand.Cards[i]))
    {
      characterCount[hand.Cards[i]] += 1;
    }
    else
    {
      characterCount[hand.Cards[i]] = 1;
    }
  }

  sortableHandsWithHandType.Add(hand with { Cards = newHandString }, EvaluateHandType(characterCount, jokerCount));
}

var result = sortableHandsWithHandType
  .OrderByDescending(kvp => kvp.Value)
  .ThenBy(kvp => kvp.Key.Cards)
  .Select((kvp, index) => kvp.Key.Bid * (index + 1))
  .Sum();
Console.WriteLine($"Result: {result}");

record Hand(string Cards, int Bid);

char GetCorrectedCharacterValue(char originalCharacter)
  => originalCharacter switch
  {
    'T' => 'A',
    'J' => '1',
    'Q' => 'C',
    'K' => 'D',
    'A' => 'E',
    _ => originalCharacter
  };

HandType EvaluateHandType(Dictionary<char, int> characterCount, int jokerCount)
{
  var topTwoCharacters = characterCount.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Value).Take(2).ToArray();
  var firstValue = topTwoCharacters.Length > 0 ? topTwoCharacters[0] += jokerCount : jokerCount;
  var secondValue = topTwoCharacters.Length == 2 ? topTwoCharacters[1] : 0;
  return topTwoCharacters switch
  {
    _ when firstValue == 5 => HandType.FiveOfAKind,
    _ when firstValue == 4 => HandType.FourOfAKind,
    _ when firstValue == 3 && secondValue == 2 => HandType.FullHouse,
    _ when firstValue == 3 => HandType.ThreeOfAKind,
    _ when firstValue == 2 && secondValue == 2 => HandType.TwoPair,
    _ when firstValue == 2 => HandType.OnePair,
    _ => HandType.HighCard
  };
}

enum HandType
{
  FiveOfAKind,
  FourOfAKind,
  FullHouse,
  ThreeOfAKind,
  TwoPair,
  OnePair,
  HighCard
}

