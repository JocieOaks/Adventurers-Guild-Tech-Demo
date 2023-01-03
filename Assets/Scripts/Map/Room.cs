using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Profiling;

/// <summary>
/// Base class for locations on a <see cref="Map"/>.
/// </summary>
public interface INode
{
    /// <summary>
    /// Determines if the <see cref="INode"/> can be passed through by a navigating <see cref="Pawn"/>.
    /// </summary>
    public bool Traversible{ get; set;}

    public Vector3Int WorldPosition { get; }
}

/// <summary>
/// Class <c>Room</c> models a closed space within a <see cref="Map"/>.
/// </summary>
public class Room
{

    protected List<Pawn> _occupants { get; } = new List<Pawn>();

    public void EnterRoom(Pawn pawn)
    {
        _occupants.Add(pawn);
        pawn.Social.EnterRoom(this);
    }

    public void ExitRoom(Pawn pawn)
    {
        _occupants.Remove(pawn);
    }

    public bool IsInRoom(Pawn pawn)
    {
        return _occupants.Any(x => x == pawn);
    }

    public IEnumerable Occupants => _occupants;


    protected RoomNode[,] _nodes;


    public int Height { get; } = 6;

    /// <summary>
    /// Initializes a new <see cref="Room"/> object.
    /// </summary>
    /// <param name="x">The width of the <see cref="Room"/>.</param>
    /// <param name="y">The length of the <see cref="Room"/>.</param>
    /// <param name="originPosition">The position of the origin in the <see cref="Room"/>'s coordinate grid within a <see cref="Map"/>.</param>
    public Room(int x, int y, Vector3Int originPosition)
    {
        _nodes = new RoomNode[x, y];
        Origin = originPosition;
        Doors = new List<ConnectionNode>();
    }

    public Room(RoomNode[,] nodes, Vector3Int originPosition)
    {
        _nodes = nodes;
        Origin = originPosition;
        Doors = new List<ConnectionNode>();
    }

    ///<value>Property <c>Doors</c> gets the <see cref="List{T}"/> of all <see cref="ConnectionNode"/>s accessible from a <see cref="Room"/>.</value>
    public List<ConnectionNode> Doors { get; set; }

    ///<value>Property <c>Length</c> represents the length of the room.</value>
    public int Length => _nodes.GetLength(1);

    ///<value>Property <c>Width</c> represents the width of the room.</value>
    public int Width => _nodes.GetLength(0);

    ///<value>Property <c>MaxPoint</c> represents the coordinates of the maximum x and y value in the <see cref="Room"/>s array.
    ///The <see cref="RoomNode"/> corresponding to the MaxPoint is not necessarily inside of the Room.</value>
    public (int x,int y) MaxPoint => (Origin.x + Width, Origin.y + Length);

    ///<value>Property <c>MinPoint</c> represents the coordinates of the minimum x and y value in the <see cref="Room"/>s array.
    ///The <see cref="RoomNode"/> corresponding to the OriginPoint is not necessarily inside of the Room.</value>
    public (int x,int y) MinPoint => (Origin.x, Origin.y);

    ///<value>Property <c>Origin</c> gives the Vector3Int origin of the <see cref="Room"/>.</value>
    public Vector3Int Origin { get; protected set; }


    ///<value>Gets the <see cref="RoomNode"/> at a specific location within the <see cref="Room"/> grid. Returns null if there is no <see cref="RoomNode"/> at that location.</value>
    public virtual RoomNode this[int x, int y]
    {
        get
        {
            if (x < 0 || y < 0 || x >= Width || y >= Length)
                return RoomNode.Invalid;
            else
                return _nodes[x, y];
        }
    }
    
    public void AddConnection(ConnectionNode connection)
    {
        if (Doors.Contains(connection))
            return;

        List<RoomNode> path = new List<RoomNode>();
        foreach (ConnectionNode door in Doors)
        {
            path = new List<RoomNode>();
            IEnumerator navigationIter = Navigate(door, connection);
            navigationIter.MoveNext();
            float distance = (float)navigationIter.Current;
            if (distance < float.PositiveInfinity)
            {
                while (navigationIter.MoveNext())
                {
                    path.Add(navigationIter.Current as RoomNode);
                }

                door.AddAdjoiningConnection(connection, distance, path);
            }

            path = new List<RoomNode>();
            navigationIter = Navigate(connection, door);
            navigationIter.MoveNext();
            distance = (float)navigationIter.Current;
            if (distance < float.PositiveInfinity)
            {
                while (navigationIter.MoveNext())
                {
                    path.Add(navigationIter.Current as RoomNode);
                }

                connection.AddAdjoiningConnection(door, distance, path);
            }

        }
        Doors.Add(connection);
    }

    public void ReplaceNode(int x, int y, RoomNode node)
    {
        _nodes[x, y] = node;
    }

    /// <summary>
    /// Determines if two <see cref="RoomNode"/>s are directly accessible to one another and thus should be part of the same <see cref="Room"/>.
    /// </summary>
    /// <param name="node1">First <see cref="RoomNode"/> being evaluated.</param>
    /// <param name="node2">Second <see cref="RoomNode"/> being eveluated.</param>
    /// <returns>Returns true if the two <see cref="RoomNode"/>s should be part of the same <see cref="Room"/>.</returns>
    public void CheckContiguous(RoomNode node1, RoomNode node2)
    {
        PriorityQueue<RoomNode, float> nodeQueue = new PriorityQueue<RoomNode, float>(false);
        RegisterForUpdate();

        RoomNode[,] immediatePredecessor = new RoomNode[Width, Length];
        int[,] g_score = new int[Width, Length];

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Length; j++)
            {
                g_score[i, j] = int.MaxValue;
            }
        }

        RoomNode current = node1;
        (int x, int y) = node1.Coords;
        (int endx, int endy) = node2.Coords;
        g_score[x, y] = 0;
        int currentScore = 0;

        while (current != node2)
        {
            foreach (INode node in current.AdjacentNodes)
            {
                if (node != null && node is RoomNode next && node != RoomNode.Undefined)
                {
                    (int nextX, int nextY) = next.Coords;

                    if (g_score[nextX, nextY] > currentScore + 1)
                    {
                        g_score[nextX, nextY] = currentScore + 1;

                        nodeQueue.Push(next, currentScore + 1 + Mathf.Sqrt(Mathf.Pow(nextX - endx, 2) + Mathf.Pow(nextY - endy, 2)));

                        immediatePredecessor[nextX, nextY] = current;
                    }
                }
            }


            if (nodeQueue.Empty)
            {
                Map.Instance.AddRooms(SplitOffRooms(node1, node2));
                return;
            }
            current = nodeQueue.Pop();
            (x, y) = current.Coords;
            currentScore = g_score[x, y];
        }
    }

    public void EnvelopRoom(Room otherRoom)
    {
        bool reconfigure = false;
        int width = Width;
        int length = Length;
        Vector3Int newOrigin = Origin;

        if (otherRoom.MaxPoint.x > MaxPoint.x)
        {
            reconfigure = true;
            width = otherRoom.MaxPoint.x - MinPoint.x;
        }
        if (otherRoom.MinPoint.x < MinPoint.x)
        {
            reconfigure = true;
            width += MinPoint.x - otherRoom.MinPoint.x;
            newOrigin.x = otherRoom.MinPoint.x;
        }
        if (otherRoom.MaxPoint.y > MaxPoint.y)
        {
            reconfigure = true;
            length = otherRoom.MaxPoint.y - MinPoint.y;
        }
        if (otherRoom.MinPoint.y < MinPoint.y)
        {
            reconfigure = true;
            length += MinPoint.y - otherRoom.MinPoint.y;
            newOrigin.y = otherRoom.MinPoint.y;
        }
        int xOffset, yOffset;
        if (reconfigure)
        {
            RoomNode[,] nodes = new RoomNode[width, length];
            xOffset = Origin.x - newOrigin.x;
            yOffset = Origin.y - newOrigin.y;
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    if (_nodes[i, j] != null)
                    {
                        nodes[i + xOffset, j + yOffset] = _nodes[i, j];
                        _nodes[i, j].Reassign(this, i + xOffset, j + yOffset);
                    }
                }
            }
            _nodes = nodes;
        }
        xOffset = otherRoom.Origin.x - newOrigin.x;
        yOffset = otherRoom.Origin.y - newOrigin.y;
        for (int i = 0; i < otherRoom.Width; i++)
        {
            for (int j = 0; j < otherRoom.Length; j++)
            {
                if (otherRoom._nodes[i, j] != null)
                {
                    _nodes[i + xOffset, j + yOffset] = otherRoom._nodes[i, j];
                    otherRoom._nodes[i, j].Reassign(this, i + xOffset, j + yOffset);
                }
            }
        }
        Doors.AddRange(otherRoom.Doors.Except(Doors));

        RegisterForUpdate();
    }


    /// <summary>
    /// Finds the world position within a <see cref="Map"/> of a <see cref="RoomNode"/> that is in a <see cref="Room"/>.
    /// </summary>
    /// <param name="node">The <see cref="RoomNode"/> whose world position is being found.</param>
    /// <returns>The world position of the given <see cref="RoomNode"/>.</returns>
    public Vector3Int GetWorldPosition(RoomNode node)
    {
        return Origin + node.RoomPosition;
    }


    const float RAD2 = 1.41421356237f;
    static readonly ProfilerMarker s_NavigateMarker = new ProfilerMarker("Room.Navigate");

    /// <summary>
    /// Evaluates the shortest route from start to end.
    /// </summary>
    /// <param name="start">The starting position to navigate from.</param>
    /// <param name="end">The ending position to navigate to.</param>
    /// <returns>The shortest path to the end as a list of steps, or default if no path exists.</returns>
    public IEnumerator Navigate<T>(INode start, T end)
    {
        using (s_NavigateMarker.Auto())
        {
            PriorityQueue<RoomNode, float> _navigation_NodeQueue = new PriorityQueue<RoomNode, float>(false);
            RoomNode[,] _navigation_ImmediatePredecessor = new RoomNode[Map.Instance.MapWidth, Map.Instance.MapLength];
            float[,] _navigation_GScore = new float[Map.Instance.MapWidth, Map.Instance.MapLength];

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    _navigation_GScore[i, j] = float.PositiveInfinity;
                }
            }

            RoomNode startNode, otherStart;

            if (start is ConnectionNode startConnection)
            {
                if (startConnection.SingleRoom)
                {
                    (startNode, otherStart) = startConnection.Nodes;
                    if (!otherStart.Traversible && !startNode.Traversible)
                        yield return float.PositiveInfinity;
                    _navigation_NodeQueue.Push(otherStart, 0);
                    (int xOther, int yOther) = otherStart.Coords;
                    _navigation_GScore[xOther, yOther] = 0;
                }
                else
                {
                    startNode = startConnection.GetRoomNode(this);
                    if (!startNode.Traversible)
                        yield return float.PositiveInfinity;
                }
            }
            else
                startNode = (start as RoomNode);

            List<RoomNode> endNodes;

            if (end is ConnectionNode endConnection)
            {
                if (endConnection.SingleRoom)
                {
                    (RoomNode a, RoomNode b) = endConnection.Nodes;
                    endNodes = new List<RoomNode>() { a, b};
                }
                else
                {
                    endNodes = new List<RoomNode>() { endConnection.GetRoomNode(this) };
                }
            }
            else if (end is RoomNode roomNode)
                endNodes = new List<RoomNode>() { roomNode };
            else if (end is IInteractable interactable)
                endNodes = interactable.GetInteractionPoints();
            else
                throw new System.ArgumentException();

            if (!endNodes.Any(x => x.Traversible))
                yield return float.PositiveInfinity;

            RoomNode current = startNode;
            (int x, int y) = startNode.Coords;
            (int endx, int endy) = endNodes[0].Coords;
            _navigation_GScore[x, y] = 0;
            _navigation_ImmediatePredecessor[x, y] = null;
            float currentScore = 0;

            while (!endNodes.Any(x => x == current))
            {

                try
                {
                    foreach ((RoomNode node, float distance) node in current.NextNodes)
                    {
                        AddNode(node.node, node.distance);
                    }
                }
                catch (System.InvalidOperationException)
                {

                    throw;
                }
                


                if (_navigation_NodeQueue.Empty || _navigation_NodeQueue.Count > 500)
                {
                    yield return float.PositiveInfinity;
                }
                current = _navigation_NodeQueue.Pop();
                (x, y) = current.Coords;
                currentScore = _navigation_GScore[x, y];
            }

            yield return currentScore;

            do
            {
                yield return current;
                (x, y) = current.Coords;
                current = _navigation_ImmediatePredecessor[x, y];
            } while (current != null);

            void AddNode(RoomNode node, float stepLength)
            {
                try
                {
                    (int nextX, int nextY) = node.Coords;
                    float prev = _navigation_GScore[nextX, nextY];
                    float newScore = currentScore + stepLength;

                    if (_navigation_GScore[nextX, nextY] > currentScore + stepLength)
                    {
                        int xDiff = Mathf.Abs(nextX - endx);
                        int yDiff = Mathf.Abs(nextY - endy);
                        float h_score = xDiff < yDiff ? yDiff + xDiff * (RAD2 - 1) : xDiff + yDiff * (RAD2 - 1);

                        if (_navigation_GScore[nextX, nextY] == float.PositiveInfinity)
                            _navigation_NodeQueue.Push(node, currentScore + stepLength + h_score);
                        else
                            _navigation_NodeQueue.Push(node, currentScore + stepLength + h_score, true);

                        _navigation_GScore[nextX, nextY] = currentScore + stepLength;

                        _navigation_ImmediatePredecessor[nextX, nextY] = current;
                    }
                }
                catch (System.NullReferenceException)
                {
                    Debug.Log("Null Reference in AddNode");
                    return;
                }
            }
        }
    }

    public void RemoveConnection(ConnectionNode connection, bool removeFromList = true)
    {
        foreach (ConnectionNode door in Doors)
        {
            door.RemoveAdjoiningConnection(connection);
            connection.RemoveAdjoiningConnection(door);
        }
        if(removeFromList)
            Doors.Remove(connection);
    }

    /// <summary>
    /// Sets whether a <see cref="INode"/> can or cannot be traversed.
    /// </summary>
    /// <param name="x">x-coordinate of the <see cref="INode"/>.</param>
    /// <param name="y">y-coordinate of the <see cref="INode"/>.</param>
    /// <param name="traversible">Whether the <see cref="INode"/> is traversible</param>
    public void SetTraversible(int x, int y, bool traversible)
    {
        _nodes[x, y].Traversible = traversible;
        RegisterForUpdate();
    }

    protected virtual Room CutRoom(int[,] roomDesignation, int flag, int originX, int originY, int endX, int endY)
    {
        RoomNode[,] nodes = new RoomNode[endX - originX, endY - originY];

        Room newRoom = new Room(nodes, Origin + new Vector3Int(originX, originY));

        for (int i = originX; i < endX; i++)
            for (int j = originY; j < endY; j++)
            {
                if (roomDesignation[i, j] == flag)
                {
                    nodes[i - originX, j - originY] = _nodes[i, j];
                    _nodes[i, j].Reassign(newRoom, i - originX, j - originY);
                    _nodes[i, j] = null;
                }
            }

        return newRoom;
    }

    public IEnumerable<RoomNode> RoomNodeIterator()
    {
        for(int i = 0; i < Width; i++)
            for(int j = 0; j < Length; j++)
            {
                if (_nodes[i,j] != null && _nodes[i,j] != RoomNode.Undefined)
                    yield return _nodes[i,j];
            }
    }

    protected virtual Room SplitOffRooms(RoomNode a, RoomNode b)
    {
        int[,] roomDesignation = new int[Width, Length];

        void DoNext(Queue<RoomNode> queue, int flag)
        {
            RoomNode current = queue.Dequeue();

            foreach (INode node in current.AdjacentNodes)
            {
                if (node != null && node is RoomNode next)
                {
                    (int x, int y) = next.Coords;
                    if (roomDesignation[x, y] == 0)
                    {
                        roomDesignation[x, y] = flag;
                        queue.Enqueue(next);
                    }
                    else if (roomDesignation[x, y] != flag)
                    {
                        int newFlag = roomDesignation[x, y];
                        for (int i = 0; i < Width; i++)
                            for (int j = 0; j < Length; j++)
                            {
                                if (roomDesignation[i, j] == flag)
                                    roomDesignation[i, j] = newFlag;
                            }
                        (x, y) = current.Coords;
                        roomDesignation[x, y] = newFlag;
                        while (queue.Count > 0)
                        {
                            current = queue.Dequeue();
                            (x, y) = current.Coords;
                            roomDesignation[x, y] = newFlag;
                        }
                        return;
                    }
                }
            }
        }

        int flag1 = 1, flag2 = 2;
        int size1 = 0, size2 = 0;

        Queue<RoomNode> queue1 = new Queue<RoomNode>(), queue2 = new Queue<RoomNode>();

        queue1.Enqueue(a);
        (int x, int y) = a.Coords;
        roomDesignation[x, y] = flag1;

        queue2.Enqueue(b);
        (x, y) = b.Coords;
        roomDesignation[x, y] = flag2;

        while (queue1.Count > 0 && queue2.Count > 0)
        {
            if (size1 <= size2)
            {
                size1++;
                DoNext(queue1, flag1);
            }

            if (size2 <= size1)
            {
                size2++;
                DoNext(queue2, flag2);
            }

        }

        int newRoomFlag;

        if (queue1.Count == 0)
            newRoomFlag = flag1;
        else
            newRoomFlag = flag2;

        (int x1, int y1, int x2, int y2) coordinates = default;

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Length; j++)
            {
                if (roomDesignation[i, j] == newRoomFlag)
                {
                    if (coordinates == default)
                    {
                        coordinates = (i, j, i, j);
                    }
                    else
                    {
                        coordinates = (Mathf.Min(i, coordinates.x1), Mathf.Min(j, coordinates.y1), Mathf.Max(i, coordinates.x2), Mathf.Max(j, coordinates.y2));
                    }
                }
            }
        }

        Room newRoom = CutRoom(roomDesignation, newRoomFlag, coordinates.x1, coordinates.y1, coordinates.x2 + 1, coordinates.y2 + 1);

        RegisterForUpdate();

        return newRoom;
    }


    /// <summary>
    /// Creates the paths between two <see cref="ConnectionNode"/>s. Because the pathways are made to be natural to the way people walk
    /// reversing them looks odd, so the reverse path is evaluated separately.
    /// </summary>
    /// <param name="connection1">The first <see cref="ConnectionNode"/></param>
    /// <param name="connection2">The second <see cref="ConnectionNode"/></param>
    void ConstructPaths(ConnectionNode connection1, ConnectionNode connection2)
    {
        List<RoomNode> path = new List<RoomNode>();
        IEnumerator navigationIter = Navigate(connection1, connection2);
        navigationIter.MoveNext();
        float distance = (float)navigationIter.Current;
        if (distance < float.PositiveInfinity)
        {
            while (navigationIter.MoveNext())
            {
                path.Add(navigationIter.Current as RoomNode);
            }

            connection1.AddAdjoiningConnection(connection2, distance, path);

            path = new List<RoomNode>();
            navigationIter = Navigate(connection2, connection1);
            navigationIter.MoveNext();
            distance = (float)navigationIter.Current;
            while (navigationIter.MoveNext())
            {
                path.Add(navigationIter.Current as RoomNode);
            }
            connection2.AddAdjoiningConnection(connection1, distance, path);
        }
        else
        {
            connection1.RemoveAdjoiningConnection(connection2);
            connection2.RemoveAdjoiningConnection(connection1);
        }
    }

    bool _updating = false;

    protected void RegisterForUpdate()
    {
        if (!_updating)
        {
            GameManager.MapChanging += UpdatePaths;
            _updating = true;
        }
    }

    void UpdatePaths()
    {
        Doors.RemoveAll(x =>
        {
            if (!x.ConnectedToRoom(this))
            {
                RemoveConnection(x, false);
                x.RegisterRooms();
                return true;
            }
            return false;
        });

        List<ConnectionNode> doorsToBeEvaluated = new List<ConnectionNode>(Doors);

        foreach (ConnectionNode door in Doors)
        {
            doorsToBeEvaluated.Remove(door);
            foreach (ConnectionNode door2 in doorsToBeEvaluated)
            {
                ConstructPaths(door, door2);
            }
        }

        _updating = false;
        GameManager.MapChanging -= UpdatePaths;
    }
}

/// <summary>
/// Class <c>Layer</c> is a <see cref="Room"/> that contains other Rooms inside it.
/// All Rooms within a layer are at the same z elevation, but multiple Layers might be at the same z elevation.
/// A Layer's primary purpose is to be able to hold references to every <see cref="RoomNode"/> contained in that layer.
/// </summary>
public class Layer : Room
{
    int _layerID;

    public int LayerID => _layerID;

    public override RoomNode this[int x, int y]
    {
        get
        {
            int xOffset = x - Origin.x;
            int yOffset = y - Origin.y;
            return base[xOffset, yOffset];
        }
    }

    public Layer(int x, int y, Vector3Int originPosition, int layerID) : base(x, y, originPosition)
    {
        for(int i = 0; i < x; i++)
            for(int j = 0; j < y; j++)
            {
                _nodes[i, j] = RoomNode.Undefined;
            }
        _layerID = layerID;
    }
    public Layer(RoomNode[,] nodes, Vector3Int originPosition, int layerID) : base(nodes, originPosition)
    {
        _layerID = layerID;
    }

    protected override Room SplitOffRooms(RoomNode a, RoomNode b)
    {
        int[,] roomDesignation = new int[Width, Length];
        int layerFlag = 0;

        void DoNext(Queue<RoomNode> queue, int flag)
        {
            RoomNode current = queue.Dequeue();

            foreach (INode node in current.AdjacentNodes)
            {
                if(node == RoomNode.Undefined)
                {
                    layerFlag = flag;
                    continue;
                }    
                if (node != null && node is RoomNode next)
                {
                    (int x, int y) = next.Coords;
                    if (roomDesignation[x, y] == 0)
                    {
                        roomDesignation[x, y] = flag;
                        queue.Enqueue(next);
                    }
                    else if (roomDesignation[x, y] != flag)
                    {
                        int newFlag = roomDesignation[x, y];
                        for (int i = 0; i < Width; i++)
                            for (int j = 0; j < Length; j++)
                            {
                                if (roomDesignation[i, j] == flag)
                                    roomDesignation[i, j] = newFlag;
                            }
                        (x, y) = current.Coords;
                        roomDesignation[x, y] = newFlag;
                        while (queue.Count > 0)
                        {
                            current = queue.Dequeue();
                            (x, y) = current.Coords;
                            roomDesignation[x, y] = newFlag;
                        }
                        return;
                    }
                }
            }
        }

        int flag1 = 1, flag2 = 2;
        int size1 = 0, size2 = 0;

        Queue<RoomNode> queue1 = new Queue<RoomNode>(), queue2 = new Queue<RoomNode>();

        queue1.Enqueue(a);
        (int x, int y) = a.Coords;
        roomDesignation[x, y] = flag1;

        queue2.Enqueue(b);
        (x, y) = b.Coords;
        roomDesignation[x, y] = flag2;

        while ((queue1.Count > 0 || layerFlag == flag1) && (queue2.Count > 0 || layerFlag == flag2))
        {
            if ((size1 <= size2 || queue2.Count == 0) && queue1.Count > 0)
            {
                size1++;
                DoNext(queue1, flag1);
            }

            if ((size2 <= size1 || queue1.Count == 0) && queue2.Count > 0)
            {
                size2++;
                DoNext(queue2, flag2);
            }

        }

        int newRoomFlag;
        if (layerFlag != 0)
            newRoomFlag = layerFlag == 1 ? 2 : 1;
        else if (queue1.Count == 0)
            newRoomFlag = flag1;
        else
            newRoomFlag = flag2;

        (int x1, int y1, int x2, int y2) coordinates = default;

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Length; j++)
            {
                if (roomDesignation[i, j] == newRoomFlag)
                {
                    if (coordinates == default)
                    {
                        coordinates = (i, j, i, j);
                    }
                    else
                    {
                        coordinates = (Mathf.Min(i, coordinates.x1), Mathf.Min(j, coordinates.y1), Mathf.Max(i, coordinates.x2), Mathf.Max(j, coordinates.y2));
                    }
                }
            }
        }

        Room newRoom = CutRoom(roomDesignation, newRoomFlag, coordinates.x1, coordinates.y1, coordinates.x2 + 1, coordinates.y2 + 1);

        RegisterForUpdate();

        return newRoom;
    }

    protected override Room CutRoom(int[,] roomDesignation, int flag, int originX, int originY, int endX, int endY)
    {
        RoomNode[,] nodes = new RoomNode[endX - originX, endY - originY];

        Room newRoom = new Room(nodes, Origin + new Vector3Int(originX, originY));

        for (int i = originX; i < endX; i++)
            for (int j = originY; j < endY; j++)
            {
                if (roomDesignation[i, j] == flag)
                {
                    nodes[i - originX, j - originY] = _nodes[i, j];
                    _nodes[i, j].Reassign(newRoom, i - originX, j - originY);
                }
            }

        return newRoom;
    }

    /// <summary>
    /// Creates a new <see cref="RoomNode"/> at a given position, and adds it to the _nodes array.
    /// </summary>
    /// <param name="x">X position of new RoomNode.</param>
    /// <param name="y">Y position of new RoomNode.</param>
    public void InstantiateRoomNode(int x, int y)
    {
        _nodes[x, y] = new RoomNode(this, x, y);
        _nodes[x, y].SetNode(Direction.North,_nodes[x, y + 1]);
        _nodes[x, y].SetNode(Direction.South,_nodes[x, y - 1]);
        _nodes[x, y].SetNode(Direction.East,_nodes[x + 1, y]);
        _nodes[x, y].SetNode(Direction.West,_nodes[x - 1, y]);
    }
}