var input = File.ReadAllLines(@"input.txt");

var directions = input[0].ToCharArray();
var nodeDictionary = input[2..].Select(node =>
{
    var nodeName = node[..3];
    var leftNodeName = node[7..10];
    var rightNodeName = node[12..15];
    return new Node(nodeName, leftNodeName, rightNodeName);
}).ToDictionary(node => node.Name);

var startNodes = nodeDictionary.Values
  .Where(node => node.Name[^1] == 'A')
  .ToArray();

// today's AOC requires you to make multiple assumptions like knowing every start node only has 1 possible end node
// and that they are cyclic, and probably other mathematical assumptions that I am too dumb for...
// so we only need to compute the steps to reach the end node once for each start node
var stepsToEndNodeForEachNode = startNodes.Select(node => FindStepsToReachEndNodeOnce(node, directions));

// and then we compute the least common multiple of al the steps, which is the number of steps required to reach
// the end node for all nodes simultaneously
var leastCommonMultiple = LeastCommonMultiple(stepsToEndNodeForEachNode);

Console.WriteLine($"Result: {leastCommonMultiple}");

record Node(string Name, string LeftName, string RightName);

long FindStepsToReachEndNodeOnce(Node node, char[] directions)
{
    var index = 0;
    var steps = 0L;
    while (node.Name[^1] != 'Z')
    {
      node = nodeDictionary[directions[index] == 'L' ? node.LeftName : node.RightName];
      index = (index + 1) % directions.Length;
      steps++;
    }
 
    return steps;
}

// to compute the least common multiple of multiple numbers we simply loop through them
// and compute the lcm of the current lcm and the next number
long LeastCommonMultiple(IEnumerable<long> numbers)
{
    var lcm = numbers.First();
    foreach (var number in numbers.Skip(1))
    {
        lcm = LeastCommonMultiple(lcm, number);
    }
    return lcm;
}

// the least common multiple of two numbers is the product of the two numbers divided by
// their greatest common divisor: |a * b| / gcd(a, b)
// https://en.wikipedia.org/wiki/Least_common_multiple
long LeastCommonMultiple(long a, long b)
  => a * b / GreatestCommonDivisor(a, b);

// to calculate the greatest common divisor of two numbers we use the euclidean algorithm
// while b is not 0 we swap a and b and set b to the remainder of a / b
// https://en.wikipedia.org/wiki/Euclidean_algorithm
long GreatestCommonDivisor(long a, long b)
{
    while (b != 0)
    {
      (a, b) = (b, a % b);
    }

    return a;
}
