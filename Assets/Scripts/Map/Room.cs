using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Class <see cref="Room"/> models a closed space within a <see cref="Map"/>.
/// </summary>
public class Room
{

    protected RoomNode[,] _nodes;
    protected List<Pawn> _occupants = new();
    const float RAD2 = 1.41421356237f;
    readonly List<ConnectingNode> _connections;
    bool _updating = false;

    /// <summary>
    /// Initializes a new empty <see cref="Room"/> object.
    /// </summary>
    /// <param name="x">The width of the <see cref="Room"/>.</param>
    /// <param name="y">The length of the <see cref="Room"/>.</param>
    /// <param name="originPosition">The position of the origin in the <see cref="Room"/>'s coordinate grid within a <see cref="Map"/>.</param>
    public Room(int x, int y, Vector3Int originPosition)
    {
        _nodes = new RoomNode[x, y];
        Origin = originPosition;
        _connections = new List<ConnectingNode>();
    }

    /// <summary>
    /// Initializes a new <see cref="Room"/> object based on an array of <see cref="RoomNode"/>s.
    /// </summary>
    /// <param name="nodes">An array of <see cref="RoomNode"/>s that the <see cref="Room"/> contains.</param>
    /// <param name="originPosition">The <see cref="Map"/> position of the lower left corner of the room.</param>
    public Room(RoomNode[,] nodes, Vector3Int originPosition)
    {
        _nodes = nodes;
        Origin = originPosition;
        _connections = new List<ConnectingNode>();
    }

    /// <value>Returns an <see cref="IEnumerable"/> of all the <see cref="ConnectingNode"/>s that border this <see cref="Room"/>.</value>
    public IEnumerable<ConnectingNode> Connections => _connections;

    /// <value>The vertical height of the <see cref="Room"/>.</value>
    public int Height { get; } = 6;

    ///<value>The length of the <see cref="Room"/> in the y-coordinates.</value>
    public int Length => _nodes.GetLength(1);

    ///<value>The coordinates of the maximum x and y value in the <see cref="Room"/>s array.
    ///The <see cref="RoomNode"/> corresponding to the MaxPoint is not necessarily inside of the Room.</value>
    public (int x, int y) MaxPoint => (Origin.x + Width, Origin.y + Length);

    ///<value>The coordinates of the minimum x and y value in the <see cref="Room"/>s array.
    ///The <see cref="RoomNode"/> corresponding to the MinPoint is not necessarily inside of the Room.</value>
    public (int x, int y) MinPoint => (Origin.x, Origin.y);

    public IEnumerable<RoomNode> Nodes
    {
        get
        {
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Length; j++)
                {
                    if (_nodes[i, j] != null && _nodes[i, j] != RoomNode.Undefined)
                        yield return _nodes[i, j];
                }
        }
    }

    /// <value>An <see cref="IEnumerable"/> that iterates over all of <see cref="Pawn"/>s that are currently in the <see cref="Room"/>.</value>
    public IEnumerable<Pawn> Occupants => _occupants;

    ///<value>The <see cref="Map"/> coordinates of the origin point of the <see cref="Room"/>.</value>
    public Vector3Int Origin { get; protected set; }

    ///<value>The width of the <see cref="Room"/> in the x-coordinates.</value>
    public int Width => _nodes.GetLength(0);

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

    /// <summary>
    /// Adds a new <see cref="ConnectingNode"/> that border's the <see cref="Room"/>.
    /// </summary>
    /// <param name="connection">The new <see cref="ConnectingNode"/> that connects to the <see cref="Room"/>.</param>
    public void AddConnection(ConnectingNode connection)
    {
        if (_connections.Contains(connection))
            return;

        foreach (ConnectingNode door in _connections)
        {
            ConstructPaths(door, connection);
        }
        _connections.Add(connection);
    }

    /// <summary>
    /// Determines if two <see cref="RoomNode"/>s are directly accessible to one another without traversing a <see cref="ConnectingNode"/> and thus should be part of the same <see cref="Room"/>.
    /// </summary>
    /// <param name="node1">First <see cref="RoomNode"/> being evaluated.</param>
    /// <param name="node2">Second <see cref="RoomNode"/> being eveluated.</param>
    /// <returns>Returns true if the two <see cref="RoomNode"/>s should be part of the same <see cref="Room"/>.</returns>
    public void CheckContiguous(RoomNode node1, RoomNode node2)
    {
        PriorityQueue<RoomNode, float> nodeQueue = new(false);
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

    /// <summary>
    /// Adds a <see cref="Pawn"/> to the list of occupants in the <see cref="Room"/>.
    /// </summary>
    /// <param name="pawn">The <see cref="Pawn"/> entering the <see cref="Room"/>.</param>
    public void EnterRoom(Pawn pawn)
    {
        _occupants.Add(pawn);
        pawn.Social.EnterRoom(this);
    }

    /// <summary>
    /// Takes another <see cref="Room"/> and combine's it with this <see cref="Room"/>, expanding the size of the <see cref="RoomNode"/> array if necessary.
    /// </summary>
    /// <param name="otherRoom">The other <see cref="Room"/> being combined with this <see cref="Room"/>.</param>
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
        _connections.AddRange(otherRoom._connections.Except(_connections));

        RegisterForUpdate();
    }

    /// <summary>
    /// Removes a <see cref="Pawn"/> from the list of occupants of the <see cref="Room"/>.
    /// </summary>
    /// <param name="pawn">The <see cref="Pawn"/> exiting the <see cref="Room"/>.</param>
    public void ExitRoom(Pawn pawn)
    {
        _occupants.Remove(pawn);
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

    /// <summary>
    /// Checks if a given <see cref="Pawn"/> is within this <see cref="Room"/>.
    /// </summary>
    /// <param name="pawn">The <see cref="Pawn"/> being checked.</param>
    /// <returns>Retunrs true if the <c>pawn</c> is in the <see cref="Room"/>.</returns>
    public bool IsInRoom(Pawn pawn)
    {
        return _occupants.Any(x => x == pawn);
    }

    /// <summary>
    /// Evaluates the shortest route from one <see cref="IWorldPosition"/> to another, where both are located within this <see cref="Room"/>.
    /// </summary>
    /// <param name="start">The starting <see cref="IWorldPosition"/>.</param>
    /// <param name="end">The ending <see cref="IWorldPosition"/>.</param>
    /// <returns>The <see cref="IEnumerator"/> first returns the distance from <c>start</c> to <c>end</c>, and then all the steps between the two, in reverse order.
    /// The initial distance may be <see cref="float.PositiveInfinity"/> if no path exists between the two <see cref="IWorldPosition"/>.</returns>
    public IEnumerator Navigate(IWorldPosition start, IWorldPosition end)
    {
        PriorityQueue<RoomNode, float> nodeQueue = new(false);
        RoomNode[,] immediatePredecessor = new RoomNode[Map.Instance.MapWidth, Map.Instance.MapLength];
        float[,] g_score = new float[Map.Instance.MapWidth, Map.Instance.MapLength];

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Length; j++)
            {
                g_score[i, j] = float.PositiveInfinity;
            }
        }

        RoomNode current;

        if (start.Node is ConnectingNode startConnection)
        {
            if (startConnection.IsWithinSingleRoom)
            {
                foreach(RoomNode node in startConnection.AdjacentNodes)
                {
                    if(node.Traversable)
                    {
                        nodeQueue.Push(node, 0);
                        (int nodex, int nodey) = node.Coords;
                        g_score[nodex, nodey] = 0;
                    }
                }

                if (nodeQueue.Empty)
                    yield return float.PositiveInfinity;

                current = nodeQueue.Pop();
            }
            else
            {
                current = startConnection.GetRoomNode(this);
                if (!current.Traversable)
                    yield return float.PositiveInfinity;
            }
        }
        else if (start.Node is RoomNode roomNode)
            current = roomNode;
        else
            throw new System.ArgumentException();

        (int x, int y) = current.Coords;
        g_score[x, y] = 0;
        immediatePredecessor[x, y] = null;
        float currentScore = 0;

        while (!end.HasNavigatedTo(current))
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



            if (nodeQueue.Empty || nodeQueue.Count > 500)
            {
                yield return float.PositiveInfinity;
            }
            current = nodeQueue.Pop();
            (x, y) = current.Coords;
            currentScore = g_score[x, y];
        }

        yield return currentScore;

        do
        {
            yield return current;
            (x, y) = current.Coords;
            current = immediatePredecessor[x, y];
        } while (current != null);

        void AddNode(RoomNode node, float stepLength)
        {
            try
            {
                (int nextX, int nextY) = node.Coords;
                float prev = g_score[nextX, nextY];
                float newScore = currentScore + stepLength;

                if (g_score[nextX, nextY] > currentScore + stepLength)
                {
                    int xDiff = Mathf.Abs(nextX - (end.WorldPosition.x - Origin.x));
                    int yDiff = Mathf.Abs(nextY - (end.WorldPosition.y - Origin.y));
                    float h_score = xDiff < yDiff ? yDiff + xDiff * (RAD2 - 1) : xDiff + yDiff * (RAD2 - 1);

                    if (g_score[nextX, nextY] == float.PositiveInfinity)
                        nodeQueue.Push(node, currentScore + stepLength + h_score);
                    else
                        nodeQueue.Push(node, currentScore + stepLength + h_score, true);

                    g_score[nextX, nextY] = currentScore + stepLength;

                    immediatePredecessor[nextX, nextY] = current;
                }
            }
            catch (System.NullReferenceException)
            {
                Debug.Log("Null Reference in AddNode");
                return;
            }
        }

    }

    /// <summary>
    /// Registers the <see cref="Room"/> to be updated by the <see cref="GameManager"/> once all changes to the <see cref="Map"/> have completed.
    /// </summary>
    public void RegisterForUpdate()
    {
        if (!_updating)
        {
            GameManager.MapChangingFirst += UpdatePaths;
            _updating = true;
        }
    }

    /// <summary>
    /// Removes a <see cref="ConnectingNode"/> from the list of connections and disconnects it from all adjoining connections.
    /// </summary>
    /// <param name="connection">The <see cref="ConnectingNode"/> being removed.</param>
    /// <param name="removeFromList">Determines whether <c>connection</c> should be actually removed from the list. This is only false when <see cref="Connections"/> is being iterated over, 
    /// and thus cannot be modified. It is expected that <c>connection</c> will be removed from the list afterwards.</param>
    public void RemoveConnection(ConnectingNode connection, bool removeFromList = true)
    {
        foreach (ConnectingNode door in _connections)
        {
            door.RemoveAdjoiningConnection(connection);
            connection.RemoveAdjoiningConnection(door);
        }
        if (removeFromList)
            _connections.Remove(connection);
    }

    /// <summary>
    /// Replaces a <see cref="RoomNode"/> with another <see cref="RoomNode"/> in the same position. This is primarily for when a <see cref="RoomNode"/> is changed into a derived class.
    /// </summary>
    /// <param name="x">The x position of the <see cref="RoomNode"/> relative to <see cref="Origin"/>.</param>
    /// <param name="y">The y position of the <see cref="RoomNode"/> relative to <see cref="Origin"/>.</param>
    /// <param name="node">The <see cref="RoomNode"/> replacing the <see cref="RoomNode"/> that was previously at this position.</param>
    public void ReplaceNode(int x, int y, RoomNode node)
    {
        _nodes[x, y] = node;
    }

    /// <summary>
    /// Creates a second <see cref="Room"/> from out of this <see cref="Room"/>. Used for when the <see cref="Room"/> is split by <see cref="IDividerNode"/>s.
    /// </summary>
    /// <param name="roomDesignation">An array of ints that flag which <see cref="RoomNode"/>s should be kept as part of this <see cref="Room"/> and which should be split off into another <see cref="Room"/>.</param>
    /// <param name="flag">The int value representing the <see cref="RoomNode"/>s being split off into another <see cref="Room"/>.</param>
    /// <param name="originX">The minimum x value of the new <see cref="Room"/> relative to <see cref="Origin"/>.</param>
    /// <param name="originY">The minimum y value of the new <see cref="Room"/> relative to <see cref="Origin"/>.</param>
    /// <param name="endX">The maximum x value of the new <see cref="Room"/> relative to <see cref="Origin"/>.</param>
    /// <param name="endY">The maximum y value of the new <see cref="Room"/> relative to <see cref="Origin"/>.</param>
    /// <returns>Returns the new <see cref="Room"/> created from this <see cref="Room"/>.</returns>
    protected virtual Room CutRoom(int[,] roomDesignation, int flag, int originX, int originY, int endX, int endY)
    {
        RoomNode[,] nodes = new RoomNode[endX - originX, endY - originY];

        Room newRoom = new(nodes, Origin + new Vector3Int(originX, originY));

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

    /// <summary>
    /// Splits the <see cref="Room"/> into two different <see cref="Room"/>s based two <see cref="RoomNode"/>s that are in different <see cref="Room"/>s.
    /// </summary>
    /// <param name="a">The first <see cref="RoomNode"/>.</param>
    /// <param name="b">The second <see cref="RoomNode"/>.</param>
    /// <returns>Returns the new <see cref="Room"/> created from this <see cref="Room"/>.</returns>
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

        Queue<RoomNode> queue1 = new(), queue2 = new();

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
    /// Creates the paths between two <see cref="ConnectingNode"/>s. Because the pathways are made to be natural to the way people walk
    /// reversing them looks odd, so the reverse path is evaluated separately.
    /// </summary>
    /// <param name="connection1">The first <see cref="ConnectingNode"/></param>
    /// <param name="connection2">The second <see cref="ConnectingNode"/></param>
    void ConstructPaths(ConnectingNode connection1, ConnectingNode connection2)
    {
        List<RoomNode> path = new();
        IEnumerator navigationIter = Navigate(connection1, connection2);
        navigationIter.MoveNext();
        float distance = (float)navigationIter.Current;
        if (distance < float.PositiveInfinity)
        {
            while (navigationIter.MoveNext())
            {
                var roomNode = (RoomNode)navigationIter.Current;
                path.Add(roomNode);
                roomNode.Reserved = true;
            }

            connection1.AddAdjoiningConnection(connection2, distance, path);

            path = new List<RoomNode>();
            navigationIter = Navigate(connection2, connection1);
            navigationIter.MoveNext();
            distance = (float)navigationIter.Current;
            while (navigationIter.MoveNext())
            {
                var roomNode = (RoomNode)navigationIter.Current;
                path.Add(roomNode);
                roomNode.Reserved = true;
            }
            connection2.AddAdjoiningConnection(connection1, distance, path);
        }
        else
        {
            connection1.RemoveAdjoiningConnection(connection2);
            connection2.RemoveAdjoiningConnection(connection1);
        }
    }

    /// <summary>
    /// Update's the pathways between the <see cref="ConnectingNode"/>s that border this <see cref="Room"/>.
    /// </summary>
    void UpdatePaths()
    {
        foreach(RoomNode node in Nodes)
        {
            node.Reserved = false;
        }

        _connections.RemoveAll(x =>
        {
            if (!x.AdjacentToRoom(this))
            {
                RemoveConnection(x, false);
                x.RegisterRooms();
                return true;
            }
            return false;
        });

        List<ConnectingNode> doorsToBeEvaluated = new(_connections);

        foreach (ConnectingNode door in _connections)
        {
            doorsToBeEvaluated.Remove(door);
            foreach (ConnectingNode door2 in doorsToBeEvaluated)
            {
                ConstructPaths(door, door2);
            }
        }

        _updating = false;
        GameManager.MapChangingFirst -= UpdatePaths;
    }
}
