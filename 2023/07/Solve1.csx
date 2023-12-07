var hands = File.ReadAllLines(@"input.txt")
    .Select(line => line.Split(' '))
    .Select(parts => new Hand(parts[0], int.Parse(parts[1])));

var sortableHandsWithHandType = new Dictionary<Hand, HandType>();

foreach (var hand in hands)
{
  var characterCount = new Dictionary<char, int>();
  var newHandString = string.Empty;
  for (var i = 0; i < hand.Cards.Length; i++)
  {
    newHandString += GetCorrectedCharacterValue(hand.Cards[i]);
    if (characterCount.ContainsKey(hand.Cards[i]))
    {
      characterCount[hand.Cards[i]] += 1;
    }
    else
    {
      characterCount[hand.Cards[i]] = 1;
    }
  }

  sortableHandsWithHandType.Add(hand with { Cards = newHandString }, EvaluateHandType(characterCount));
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
    'J' => 'B',
    'Q' => 'C',
    'K' => 'D',
    'A' => 'E',
    _ => originalCharacter
  };

HandType EvaluateHandType(Dictionary<char, int> characterCount)
  => characterCount.OrderByDescending(kvp => kvp.Value).Take(2).ToArray() switch
  {
    var list when list[0].Value == 5 => HandType.FiveOfAKind,
    var list when list[0].Value == 4 => HandType.FourOfAKind,
    var list when list[0].Value == 3 && list[1].Value == 2 => HandType.FullHouse,
    var list when list[0].Value == 3 => HandType.ThreeOfAKind,
    var list when list[0].Value == 2 && list[1].Value == 2 => HandType.TwoPair,
    var list when list[0].Value == 2 => HandType.OnePair,
    _ => HandType.HighCard
  };

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
