using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.VisualScripting.Member;

/// <summary>
/// Helper class for <see cref="Sector"/> to store data for each <see cref="INode"/> in the <see cref="Sector"/>.
/// </summary>
public class GraphNode
{
    /// <summary>
    /// Initialize a new instance of <see cref="GraphNode"/>.
    /// </summary>
    /// <param name="node">The corresponding <see cref="INode"/>.</param>
    /// <param name="prev">The previous <see cref="GraphNode"/> traversed to reach the <see cref="INode"/>.</param>
    public GraphNode(INode node, GraphNode prev)
    {
        Node = node;
        Sources = new List<INode>(prev.Sources.Count + 1) { node };
        Sources.AddRange(prev.Sources);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="GraphNode"/> with no predecessor.
    /// Used to construct the origin for a sector.
    /// </summary>
    /// <param name="node"></param>
    public GraphNode(INode node)
    {
        Node = node;
        Sources = new List<INode>();
    }

    /// <value>Returns true if the <see cref="GraphNode"/> has not been iterated over by <see cref="Sector.DivideIntoSectors(Map, ref List{Sector})"/>.</value>
    public bool IsNew { get; set; } = true;

    /// <value>The <see cref="INode"/> the <see cref="GraphNode"/> corresponds to.</value>
    public INode Node {get;}
    /// <value>A <see cref="List{T}"/> of <see cref="INode"/>s that must be traversed to reach <see cref="GraphNode.Node"/>.</value>
    public List<INode> Sources { get; }
    /// <summary>
    /// Compares the <see cref="GraphNode.Sources"/> list for two <see cref="GraphNode"/>s. Eliminates elements from the two lists that are not shared between them or
    /// their corresponding <see cref="GraphNode.Node"/>. Any <see cref="INode"/> that is not on both lists is not required to be traversed in order to reach their
    /// <see cref="GraphNode.Node"/>.
    /// </summary>
    /// <param name="a">First <see cref="GraphNode"/> being compared.</param>
    /// <param name="b">First <see cref="GraphNode"/> being compared.</param>
    /// <param name="aChanged">Returns true if <see cref="GraphNode"/> a was altered.</param>
    /// <param name="bChanged">Returns true if <see cref="GraphNode"/> b was altered.</param>
    public static void IntersectSources(GraphNode a, GraphNode b, out bool aChanged, out bool bChanged)
    {
        aChanged = a.Sources.RemoveAll(x => b.Sources.All(y => y != x) && x != a.Node) > 0;
        bChanged = b.Sources.RemoveAll(x => a.Sources.All(y => y != x) && x != b.Node) > 0;
    }
}


/// <summary>
/// The <see cref="Sector"/> class divides the <see cref="Map"/> into sections where each <see cref="RoomNode"/> in a <see cref="Sector"/> is accessible from any other 
/// <see cref="RoomNode"/> in a <see cref="Sector"/>. However, <see cref="RoomNode"/>s in two different <see cref="Sector"/>s will be closed off to eachother.
/// Used to ensure that <see cref="Pawn"/>s will not try to pathfind to locations that are inaccessible to them. 
/// Also, checks for bottleneck points that should not be blocked.
/// </summary>
public class Sector
{
    List<INode> _bottlenecks = new List<INode>();

    /// <value>The list of bottleneck <see cref="INode"/>s in the <see cref="Sector"/>. If any of the <see cref="INode"/>s are blocked, 
    /// then parts of the <see cref="Sector"/> become inaccessible to one another.</value>
    public IEnumerable<INode> BottleNecks => _bottlenecks;

    /// <summary>
    /// Creates a list of <see cref="Sector"/>s from the map using a modified version of BFS to find all connected <see cref="INode"/>'s and find any bottleneck <see cref="INode"/>.
    /// </summary>
    /// <param name="map">The <see cref="Map"/> from which the <see cref="Sector"/>s are being built.</param>
    /// <param name="sectors">Reference to the previous list of <see cref="Sector"/>s constructed from the <see cref="Map"/>.</param>
    public static void DivideIntoSectors(Map map, ref List<Sector> sectors)
    {
        sectors.Clear();

        do
        {
            RoomNode currentNode = null;

            foreach(RoomNode node in Map.Instance.AllNodes)
            {
                if(node.Traversible && 
                    !sectors.Any(x => x == node.Sector) && 
                    node.NorthEast != null && 
                    node.NorthEast.Traversible && 
                    node.TryGetNodeAs(Direction.North, out RoomNode north) && 
                    north.Traversible && 
                    node.TryGetNodeAs(Direction.East, out RoomNode east)&&
                    east.Traversible)
                {
                    currentNode = node;
                    break;
                }
            }
            if (currentNode == null)
            {
                foreach (RoomNode node in Map.Instance.AllNodes)
                {
                    if (node.Traversible && !sectors.Any(x => x == node.Sector))
                    {
                        currentNode = node;
                        break;
                    }
                }
                if (currentNode == null)
                    break;
            }

            Sector sector = new Sector();
            sectors.Add(sector);

            currentNode.Sector = sector;
            Dictionary<INode, GraphNode> graphNodes = new Dictionary<INode, GraphNode>();
            Queue<GraphNode> queue = new Queue<GraphNode>();
            List<GraphNode> endPoints = new List<GraphNode>();

            GraphNode current = new GraphNode(currentNode);
            graphNodes[currentNode] = current;

            queue.Enqueue(current);

            while (queue.Count > 0)
            {
                current = queue.Dequeue();

                if (current.Node is RoomNode roomNode)
                {
                    bool isEndpoint = true;
                    foreach (INode node in roomNode.AdjacentNodes)
                    {
                        if (Next(current, node))
                            isEndpoint = false;
                    }

                    if(isEndpoint && current.IsNew)
                    {
                        endPoints.Add(current);
                    }
                } 
                else if(current.Node is ConnectionNode connection)
                {
                    foreach (INode node in connection.Nodes)
                    {
                        Next(current, node);
                    }
                }
                current.IsNew = false;
            }

            foreach(GraphNode endpoint in endPoints)
            {
                foreach (INode source in endpoint.Sources)
                {
                    if (source != endpoint.Node && !sector._bottlenecks.Contains(source))
                    {
                        sector._bottlenecks.Add(source);
                    }
                }
            }
            
            bool Next(GraphNode current, INode nextNode)
            {
                if (nextNode.Traversible)
                {
                    if (graphNodes.TryGetValue(nextNode, out GraphNode next))
                    {
                        GraphNode.IntersectSources(next, current, out bool nextChanged, out bool currentChanged);

                        if(nextChanged)
                            queue.Enqueue(next);
                        if(currentChanged)
                            queue.Enqueue(current);
                        
                    }
                    else
                    {
                        if (nextNode is RoomNode roomNode)
                            roomNode.Sector = sector;

                        GraphNode graphNode = new GraphNode(nextNode, current);
                        queue.Enqueue(graphNode);
                        graphNodes[nextNode] = graphNode;
                        return true;
                    }
                }

                return false;
            }



        }while(true);
    }
}