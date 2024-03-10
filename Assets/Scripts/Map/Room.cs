using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Navigation.Destination;
using Assets.Scripts.Map.Node;
using PriorityQueue;
using UnityEngine;

namespace Assets.Scripts.Map
{
    /// <summary>
    /// Class <see cref="Room"/> models a closed space within a <see cref="Map"/>.
    /// </summary>
    public class Room
    {
        /// <value>A 2D array containing all the <see cref="RoomNode"/>'s within the <see cref="Room"/>.
        /// Depending on the shape of the <see cref="Room"/>, some entries may be null.</value>
        protected RoomNode[,] Nodes { get; set; }
        private readonly List<Pawn> _occupants = new();
        private readonly List<ConnectingNode> _connections;
        private bool _updating;
        private readonly ListDictionary _connectionPaths = new();
        private readonly float[][,] _poiPaths = new float[Enum.GetNames(typeof(PointOfInterest)).Length][,];

        /// <summary>
        /// Initializes a new empty <see cref="Room"/> object.
        /// </summary>
        /// <param name="x">The width of the <see cref="Room"/>.</param>
        /// <param name="y">The length of the <see cref="Room"/>.</param>
        /// <param name="originPosition">The position of the origin in the <see cref="Room"/>'s coordinate grid within a <see cref="Map"/>.</param>
        public Room(int x, int y, Vector3Int originPosition)
        {
            Nodes = new RoomNode[x, y];
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
            Nodes = nodes;
            Origin = originPosition;
            _connections = new List<ConnectingNode>();
        }

        /// <value>Returns an <see cref="IEnumerable"/> of all the <see cref="ConnectingNode"/>s that border this <see cref="Room"/>.</value>
        public IEnumerable<ConnectingNode> Connections => _connections;

        /// <value>The vertical height of the <see cref="Room"/>.</value>
        public int Height { get; } = 6;

        ///<value>The length of the <see cref="Room"/> in the y-coordinates.</value>
        public int Length => Nodes.GetLength(1);

        ///<value>The coordinates of the maximum x and y value in the <see cref="Room"/>s array.
        ///The <see cref="RoomNode"/> corresponding to the MaxPoint is not necessarily inside of the Room.</value>
        public (int x, int y) MaxPoint => (Origin.x + Width, Origin.y + Length);

        ///<value>The coordinates of the minimum x and y value in the <see cref="Room"/>s array.
        ///The <see cref="RoomNode"/> corresponding to the MinPoint is not necessarily inside of the Room.</value>
        public (int x, int y) MinPoint => (Origin.x, Origin.y);

        /// <value>Iterates through all the <see cref="RoomNode"/>'s within the <see cref="Room"/>.</value>
        public IEnumerable<RoomNode> GetNodes
        {
            get
            {
                for (int i = 0; i < Width; i++)
                for (int j = 0; j < Length; j++)
                {
                    if (Nodes[i, j] != null && Nodes[i, j] != RoomNode.Undefined)
                        yield return Nodes[i, j];
                }
            }
        }

        /// <value>An <see cref="IEnumerable"/> that iterates over all of <see cref="AdventurerPawn"/>s that are currently in the <see cref="Room"/>.</value>
        public IEnumerable<Pawn> Occupants => _occupants;

        ///<value>The <see cref="Map"/> coordinates of the origin point of the <see cref="Room"/>.</value>
        public Vector3Int Origin { get; protected set; }

        ///<value>The width of the <see cref="Room"/> in the x-coordinates.</value>
        public int Width => Nodes.GetLength(0);

        ///<value>Gets the <see cref="RoomNode"/> at a specific location within the <see cref="Room"/> grid. Returns null if there is no <see cref="RoomNode"/> at that location.</value>
        public virtual RoomNode this[int x, int y]
        {
            get
            {
                if (x < 0 || y < 0 || x >= Width || y >= Length)
                    return RoomNode.Invalid;
                else
                    return Nodes[x, y];
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
            
            _connections.Add(connection);
            RegisterForUpdate();
        }

    /// <summary>
    /// Determines if two <see cref="RoomNode"/>s are directly accessible to one another without traversing a <see cref="ConnectingNode"/> and thus should be part of the same <see cref="Room"/>.
    /// </summary>
    /// <param name="node1">First <see cref="RoomNode"/> being evaluated.</param>
    /// <param name="node2">Second <see cref="RoomNode"/> being evaluated.</param>
    /// <returns>Returns true if the two <see cref="RoomNode"/>s should be part of the same <see cref="Room"/>.</returns>
    public void CheckContiguous(RoomNode node1, RoomNode node2)
    {
        PriorityQueue<RoomNode, float> nodeQueue = new(PriorityQueue<RoomNode, float>.Min);
        RegisterForUpdate();

            RoomNode[,] immediatePredecessor = new RoomNode[Width, Length];
            int[,] gScore = new int[Width, Length];

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    gScore[i, j] = int.MaxValue;
                }
            }

            RoomNode current = node1;
            (int x, int y) = node1.Coords;
            (int endX, int endY) = node2.Coords;
            gScore[x, y] = 0;
            int currentScore = 0;

            while (current != node2)
            {
                foreach (INode node in current.AdjacentNodes)
                {
                    if (node is RoomNode next && node != RoomNode.Undefined)
                    {
                        (int nextX, int nextY) = next.Coords;

                        if (gScore[nextX, nextY] > currentScore + 1)
                        {
                            gScore[nextX, nextY] = currentScore + 1;

                            nodeQueue.Push(next, currentScore + 1 + Mathf.Sqrt(Mathf.Pow(nextX - endX, 2) + Mathf.Pow(nextY - endY, 2)));

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
                currentScore = gScore[x, y];
            }
        }

        /// <summary>
        /// Adds a <see cref="AdventurerPawn"/> to the list of occupants in the <see cref="Room"/>.
        /// </summary>
        /// <param name="pawn">The <see cref="AdventurerPawn"/> entering the <see cref="Room"/>.</param>
        public void EnterRoom(Pawn pawn)
        {
            _occupants.Add(pawn);
            if(pawn is AdventurerPawn realPawn )
                realPawn.Social.EnterRoom(this);
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
                        if (Nodes[i, j] != null)
                        {
                            nodes[i + xOffset, j + yOffset] = Nodes[i, j];
                            Nodes[i, j].Reassign(this, i + xOffset, j + yOffset);
                        }
                    }
                }
                Nodes = nodes;
            }
            xOffset = otherRoom.Origin.x - newOrigin.x;
            yOffset = otherRoom.Origin.y - newOrigin.y;
            for (int i = 0; i < otherRoom.Width; i++)
            {
                for (int j = 0; j < otherRoom.Length; j++)
                {
                    if (otherRoom.Nodes[i, j] != null)
                    {
                        Nodes[i + xOffset, j + yOffset] = otherRoom.Nodes[i, j];
                        otherRoom.Nodes[i, j].Reassign(this, i + xOffset, j + yOffset);
                    }
                }
            }
            _connections.AddRange(otherRoom._connections.Except(_connections));

            RegisterForUpdate();
        }

        /// <summary>
        /// Removes a <see cref="AdventurerPawn"/> from the list of occupants of the <see cref="Room"/>.
        /// </summary>
        /// <param name="pawn">The <see cref="AdventurerPawn"/> exiting the <see cref="Room"/>.</param>
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
        /// Checks if a given <see cref="AdventurerPawn"/> is within this <see cref="Room"/>.
        /// </summary>
        /// <param name="pawn">The <see cref="AdventurerPawn"/> being checked.</param>
        /// <returns>Returns true if the <c>pawn</c> is in the <see cref="Room"/>.</returns>
        public bool IsInRoom(AdventurerPawn pawn)
        {
            return _occupants.Any(x => x == pawn);
        }

        /// <summary>
        /// Calculates the path lengths of all <see cref="RoomNode"/>'s in the <see cref="Room"/> to reach the specified <see cref="IDestination"/>.
        /// This is the static GScore based on the layout of the <see cref="Room"/>. The actual GScores may be different due to changing obstacles.
        /// </summary>
        /// <param name="destination">The <see cref="IDestination"/> being evaluated.</param>
        /// <returns>Returns a 2D array detailing the expected distance from each <see cref="RoomNode"/> to <paramref name="destination"/>.
        /// <see cref="RoomNode"/>'s with no viable paths to <paramref name="destination"/> will have a score of <see cref="float.PositiveInfinity"/>.</returns>
        public float[,] CreateGScoreTable(IDestination destination)
        {
            Queue<RoomNode> nodeQueue = new();
            float[,] gScore = new float[Width, Length];

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    gScore[i, j] = float.PositiveInfinity;
                }
            }

            foreach (RoomNode node in destination.Endpoints)
            {
                if (node.Traversable)
                {
                    nodeQueue.Enqueue(node);
                    (int nodeX, int nodeY) = node.Coords;
                    gScore[nodeX, nodeY] = 0;
                }
            }

            float currentScore;

            while (nodeQueue.Count > 0)
            {
                RoomNode current = nodeQueue.Dequeue();

                (int x, int y) = current.Coords;
                currentScore = gScore[x, y];

                foreach ((RoomNode node, float distance) node in current.NextNodes)
                {
                    AddNode(node.node, node.distance);
                }

            }

            return gScore;

            void AddNode(RoomNode node, float stepLength)
            {
                try
                {
                    (int nextX, int nextY) = node.Coords;
                    float prev = gScore[nextX, nextY];
                    float newScore = currentScore + stepLength;

                    if (prev > newScore)
                    {
                        gScore[nextX, nextY] = newScore;
                        nodeQueue.Enqueue(node);
                    }
                }
                catch (NullReferenceException)
                {
                    Debug.Log("Null Reference in AddNode");
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
                GameManager.MapChanging += UpdatePaths;
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
            Nodes[x, y] = node;
        }

        /// <summary>
        /// Creates a second <see cref="Room"/> from out of this <see cref="Room"/>. Used for when the <see cref="Room"/> is split by <see cref="IDividerNode"/>s.
        /// </summary>
        /// <param name="roomDesignation">An array of int that flag which <see cref="RoomNode"/>s should be kept as part of this <see cref="Room"/> and which should be split off into another <see cref="Room"/>.</param>
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
                    nodes[i - originX, j - originY] = Nodes[i, j];
                    Nodes[i, j].Reassign(newRoom, i - originX, j - originY);
                    Nodes[i, j] = null;
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
                    if (node is RoomNode next)
                    {
                        (int nextX, int nextY) = next.Coords;
                        if (roomDesignation[nextX, nextY] == 0)
                        {
                            roomDesignation[nextX, nextY] = flag;
                            queue.Enqueue(next);
                        }
                        else if (roomDesignation[nextX, nextY] != flag)
                        {
                            int newFlag = roomDesignation[nextX, nextY];
                            for (int i = 0; i < Width; i++)
                            for (int j = 0; j < Length; j++)
                            {
                                if (roomDesignation[i, j] == flag)
                                    roomDesignation[i, j] = newFlag;
                            }
                            (nextX, nextY) = current.Coords;
                            roomDesignation[nextX, nextY] = newFlag;
                            while (queue.Count > 0)
                            {
                                current = queue.Dequeue();
                                (nextX, nextY) = current.Coords;
                                roomDesignation[nextX, nextY] = newFlag;
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

            int newRoomFlag = queue1.Count == 0 ? flag1 : flag2;

            (int x1, int y1, int x2, int y2) coordinates = default;

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    if (roomDesignation[i, j] == newRoomFlag)
                    {
                        coordinates = coordinates == default
                            ? (i, j, i, j)
                            : (Mathf.Min(i,
                                coordinates.x1), Mathf.Min(j,
                                coordinates.y1), Mathf.Max(i,
                                coordinates.x2), Mathf.Max(j,
                                coordinates.y2));
                    }
                }
            }

            Room newRoom = CutRoom(roomDesignation, newRoomFlag, coordinates.x1, coordinates.y1, coordinates.x2 + 1, coordinates.y2 + 1);

            RegisterForUpdate();

            return newRoom;
        }

        /// <summary>
        /// Determines the expected path length to travel from a given <see cref="RoomNode"/> to another <see cref="ConnectingNode"/> and vice versa.
        /// This is based on the static shape of the <see cref="Room"/>, but the actual distance may be different due to changing obstacles.
        /// </summary>
        /// <param name="node">The <see cref="RoomNode"/>.</param>
        /// <param name="connection">The <see cref="ConnectingNode"/>.</param>
        /// <returns>Returns the distance from <paramref name="node"/> to <paramref name="connection"/> when traveling through this <see cref="Room"/>.
        /// Returns <see cref="float.PositiveInfinity"/> if there is no viable path.</returns>
        /// <exception cref="ArgumentException">Thrown if either <paramref name="node"/> is not within the <see cref="Room"/> or
        /// <paramref name="connection"/> is not connected to the <see cref="Room"/>.</exception>
        public float GetDistance(RoomNode node, ConnectingNode connection)
        {
            if (node.Room != this)
                throw new ArgumentException("RoomNode is not within this Room.");
            if (!_connections.Contains(connection))
                throw new ArgumentException("ConnectionNode does not connect to this Room.");

            Vector3Int position = node.RoomPosition;
            if (_connectionPaths[connection] is not float[,] path)
            {
                RoomNode roomNode = connection.GetRoomNode(this);
                path = CreateGScoreTable(new TargetDestination(roomNode));
                if (connection.IsWithinSingleRoom)
                {
                    float[,] path2 = CreateGScoreTable(new TargetDestination(connection.GetOppositeRoomNode(roomNode)));

                    for (var i = 0; i < Width; i++)
                    {
                        for (var j = 0; j < Length; j++)
                        {
                            if (path2[i, j] < path[i, j])
                            {
                                path[i, j] = path2[i, j];
                            }
                        }
                    }
                }
                _connectionPaths[connection] = path;
            }
            return path[position.x, position.y];
        }

        /// <summary>
        /// Determines the expected path length to travel from a given <see cref="RoomNode"/> to a specified <see cref="PointOfInterest"/>.
        /// This is based on the static shape of the <see cref="Room"/>, but the actual distance may be different due to changing obstacles.
        /// </summary>
        /// <param name="node">The <see cref="RoomNode"/>.</param>
        /// <param name="poi">The <see cref="PointOfInterest"/>.</param>
        /// <returns>Returns the distance from <paramref name="node"/> to <paramref name="poi"/> when traveling through this <see cref="Room"/>.
        /// Returns &lt;see cref="float.PositiveInfinity"/&gt; if there is no viable path.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="node"/> is not within the <see cref="Room"/>.</exception>
        public float GetDistance(RoomNode node, PointOfInterest poi)
        {
            if (node.Room != this)
                throw new ArgumentException("RoomNode is not within this Room.");
            Vector3Int position = node.RoomPosition;

            if (_poiPaths[(int)poi] is null)
            {
                float[,] path = CreateGScoreTable(poi switch
                {
                    PointOfInterest.Lay => new LayDestination(),
                    PointOfInterest.Food => new FoodDestination(),
                    PointOfInterest.Sit => new SitDestination(),
                    _ => throw new ArgumentOutOfRangeException(nameof(poi), poi, null)
                });

                _poiPaths[(int)poi] = path;
            }
            return _poiPaths[(int)poi][position.x, position.y];
        }
        
        /// <summary>
        /// Update's the pathways between the <see cref="ConnectingNode"/>s that border this <see cref="Room"/>.
        /// </summary>
        private void UpdatePaths()
        {


            foreach(RoomNode node in GetNodes)
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

            _connectionPaths.Clear();
            _poiPaths[0] = null;
            _poiPaths[1] = null;
            _poiPaths[2] = null;

            _updating = false;
            GameManager.MapChanging -= UpdatePaths;
        }
    }
}
