var input = File.ReadAllLines(@"input.txt");

var startNode = "AAA";
var endNode = "ZZZ";

var directions = input[0].ToCharArray();
var nodeDictionary = input[2..].Select(node =>
{
    var nodeName = node[..3];
    var leftNodeName = node[7..10];
    var rightNodeName = node[12..15];
    return new Node(nodeName, leftNodeName, rightNodeName);
}).ToDictionary(node => node.Name);

Console.WriteLine("Steps to end node: {0}", WalkTree(nodeDictionary[startNode], directions, 0));

record Node(string Name, string LeftName, string RightName);

int WalkTree(Node node, char[] directions, int index)
{
    index = index % directions.Length;

    if (node.Name == endNode)
    {
        return 0;
    }
 
    return directions[index] switch
    {
      'L' => WalkTree(nodeDictionary[node.LeftName], directions, index + 1) + 1,
      'R' => WalkTree(nodeDictionary[node.RightName], directions, index + 1) + 1,
      _ => throw new Exception("Invalid direction")
    };
}
