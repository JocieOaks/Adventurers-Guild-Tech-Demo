using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.AI.Actor;
using Assets.Scripts.AI.Step;
using Assets.Scripts.Map.Sprite_Object;
using UnityEngine;

namespace Assets.Scripts.Map.Node
{
    /// <summary>
    /// The <see cref="RoomNode"/> class is an <see cref="INode"/> that exists as a coordinate location within a <see cref="Scripts.Map.Room"/>.
    /// </summary>
    public class RoomNode : INode
    {
        private readonly List<(RoomNode, float)> _nextNodes = new();
        private bool _nextNodesKnown;

        private INode _north, _south, _east, _west;

        private IWorldPosition _occupant;

        private float? _speed;
        private float? _speedInverse;
        private bool? _traversable;

        /// <summary>
        /// Initializes a new instance of the <see cref ="RoomNode"/> class.
        /// </summary>
        /// <param name="room">The <see cref="Scripts.Map.Room"/> containing the <see cref="RoomNode"/>.</param>
        /// <param name="x">The x coordinate of the <see cref="RoomNode"/> within the <see cref="Scripts.Map.Room"/>.</param>
        /// <param name="y">The y coordinate of the <see cref="RoomNode"/> within the <see cref="Scripts.Map.Room"/>.</param>
        public RoomNode(Room room, int x, int y)
        {
            Room = room;
            RoomPosition = new Vector3Int(x, y);
            if (x < 0 || y < 0)
                Floor = new FloorSprite(Vector3Int.back);
            else
                Floor = new FloorSprite(WorldPosition);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomNode"/> class from a previously existing <see cref="RoomNode"/>. This is primarily for when a <see cref="RoomNode"/> is changed into a derived class.
        /// </summary>
        /// <param name="node"></param>
        public RoomNode(RoomNode node)
        {
            Room = node.Room;
            RoomPosition = node.RoomPosition;
            Floor = node.Floor;
            SetNode(Direction.North, node._north);
            SetNode(Direction.South, _south = node._south);
            SetNode(Direction.East, _east = node._east);
            SetNode(Direction.West, _west = node._west);

            Room.ReplaceNode(RoomPosition.x, RoomPosition.y, this);
            Map.Instance[Room.Origin.z].ReplaceNode(WorldPosition.x, WorldPosition.y, this);
        }

        /// <value>Static property that returns a special <see cref="RoomNode"/> that serves as a flag.</value>
        public static RoomNode Invalid { get; } = new(null, -1, -1);

        /// <value>Static property that returns a special <see cref="RoomNode"/> that serves as a flag.</value>
        public static RoomNode Undefined { get; } = new(null, -2, -2);

        /// <inheritdoc/>
        public IEnumerable<INode> AdjacentNodes
        {
            get
            {
                yield return _north;
                yield return _south;
                yield return _east;
                yield return _west;
            }
        }

        /// <value>A quick accessor for the x and y room positions of the <see cref="RoomNode"/> as a tuple of two int.</value>
        public (int x, int y) Coords => (RoomPosition.x, RoomPosition.y);

        public Vector3Int Dimensions => new(1, 1, Room.Height);

        /// <value>Determines if a <see cref="SpriteObject"/>is currently placed at the <see cref ="RoomNode"/>.</value>
        public bool Empty => _occupant == null;

        /// <value> Returns the <see cref="FloorSprite"/> <see cref="SpriteObject"/> associated with this <see cref="RoomNode"/>.</value>
        public FloorSprite Floor { get; }

        /// <inheritdoc/>
        public Vector3Int NearestCornerPosition => WorldPosition;

        /// <value>The <see cref="List{T}"/> of all <see cref="RoomNode"/>'s that are a single step away from this <see cref="RoomNode"/> in terms of <see cref="AdventurerPawn"/> navigation.
        /// If the list is not already made it will be created.</value>
        public IEnumerable<(RoomNode,float)> NextNodes
        {
            get
            {
                lock (_nextNodes)
                {
                    if (!_nextNodesKnown)
                    {
                        _nextNodes.Clear();

                        bool isTraversableNorth = TryGetNodeAs(Direction.North, out RoomNode north) && north.Traversable;
                        bool isTraversableSouth = TryGetNodeAs(Direction.South, out RoomNode south) && south.Traversable;
                        bool isTraversableEast = TryGetNodeAs(Direction.East, out RoomNode east) && east.Traversable;
                        bool isTraversableWest = TryGetNodeAs(Direction.West, out RoomNode west) && west.Traversable;

                        if (isTraversableNorth)
                        {
                            _nextNodes.Add((north, (SpeedInverse + north.SpeedInverse) / 2));
                        }
                        if (isTraversableSouth)
                        {
                            _nextNodes.Add((south, (SpeedInverse + south.SpeedInverse) / 2));
                        }
                        if (isTraversableEast)
                        {
                            _nextNodes.Add((east, (SpeedInverse + east.SpeedInverse) / 2));
                        }
                        if (isTraversableWest)
                        {
                            _nextNodes.Add((west, (SpeedInverse + west.SpeedInverse) / 2));
                        }

                        bool isTraversableNorthEast = NorthEast?.Traversable ?? false;
                        bool isTraversableNorthWest = NorthWest?.Traversable ?? false;
                        bool isTraversableSouthEast = SouthEast?.Traversable ?? false;
                        bool isTraversableSouthWest = SouthWest?.Traversable ?? false;

                        if (isTraversableNorthEast)
                        {
                            _nextNodes.Add((NorthEast, Utility.Utility.RAD2_2 * (SpeedInverse + NorthEast.SpeedInverse)));
                        }
                        if (isTraversableNorthWest)
                        {
                            _nextNodes.Add((NorthWest, Utility.Utility.RAD2_2 * (SpeedInverse + NorthWest.SpeedInverse)));
                        }
                        if (isTraversableSouthEast)
                        {
                            _nextNodes.Add((SouthEast, Utility.Utility.RAD2_2 * (SpeedInverse + SouthEast.SpeedInverse)));
                        }
                        if (isTraversableSouthWest)
                        {
                            _nextNodes.Add((SouthWest, Utility.Utility.RAD2_2 * (SpeedInverse + SouthWest.SpeedInverse)));
                        }

                        if (isTraversableNorth && isTraversableNorthEast)
                        {
                            if (NorthEast.TryGetNodeAs(Direction.North, out RoomNode northNorthEast) && northNorthEast.Traversable)
                                _nextNodes.Add((northNorthEast, Utility.Utility.RAD5_4 * (SpeedInverse + north.SpeedInverse + NorthEast.SpeedInverse + northNorthEast.SpeedInverse)));
                        }
                        if (isTraversableNorth && isTraversableNorthWest)
                        {
                            if (NorthWest.TryGetNodeAs(Direction.North, out RoomNode northNorthWest) && northNorthWest.Traversable)
                                _nextNodes.Add((northNorthWest, Utility.Utility.RAD5_4 * (SpeedInverse + north.SpeedInverse + NorthWest.SpeedInverse + northNorthWest.SpeedInverse)));
                        }
                        if (isTraversableSouth && isTraversableSouthEast)
                        {
                            if (SouthEast.TryGetNodeAs(Direction.South, out RoomNode southSouthEast) && southSouthEast.Traversable)
                                _nextNodes.Add((southSouthEast, Utility.Utility.RAD5_4 * (SpeedInverse + south.SpeedInverse + SouthEast.SpeedInverse + southSouthEast.SpeedInverse)));
                        }
                        if (isTraversableSouth && isTraversableSouthWest)
                        {
                            if (SouthWest.TryGetNodeAs(Direction.South, out RoomNode southSouthWest) && southSouthWest.Traversable)
                                _nextNodes.Add((southSouthWest, Utility.Utility.RAD5_4 * (SpeedInverse + south.SpeedInverse + SouthWest.SpeedInverse + southSouthWest.SpeedInverse)));
                        }
                        if (isTraversableEast && isTraversableNorthEast)
                        {
                            if (NorthEast.TryGetNodeAs(Direction.East, out RoomNode eastNorthEast) && eastNorthEast.Traversable)
                                _nextNodes.Add((eastNorthEast, Utility.Utility.RAD5_4 * (SpeedInverse + east.SpeedInverse + NorthEast.SpeedInverse + eastNorthEast.SpeedInverse)));
                        }
                        if (isTraversableWest && isTraversableNorthWest)
                        {
                            if (NorthWest.TryGetNodeAs(Direction.West, out RoomNode westNorthWest) && westNorthWest.Traversable)
                                _nextNodes.Add((westNorthWest, Utility.Utility.RAD5_4 * (SpeedInverse + west.SpeedInverse + NorthWest.SpeedInverse + westNorthWest.SpeedInverse)));
                        }
                        if (isTraversableEast && isTraversableSouthEast)
                        {
                            if (SouthEast.TryGetNodeAs(Direction.East, out RoomNode eastSouthEast) && eastSouthEast.Traversable)
                                _nextNodes.Add((eastSouthEast, Utility.Utility.RAD5_4 * (SpeedInverse + east.SpeedInverse + SouthEast.SpeedInverse + eastSouthEast.SpeedInverse)));
                        }
                        if (isTraversableWest && isTraversableSouthWest)
                        {
                            if (SouthWest.TryGetNodeAs(Direction.West, out RoomNode westSouthWest) && westSouthWest.Traversable)
                                _nextNodes.Add((westSouthWest, Utility.Utility.RAD5_4 * (SpeedInverse + west.SpeedInverse + SouthWest.SpeedInverse + westSouthWest.SpeedInverse)));
                        }
                    }
                }
                _nextNodesKnown = true;
                return _nextNodes;
            }
        }

        /// <inheritdoc/>
        public RoomNode Node => this;

        /// <value>Returns the <see cref="RoomNode"/> that is north east of this <see cref="RoomNode"/> or null if such a node does not exist or is inaccessible.</value>
        public RoomNode NorthEast => GetNodeAs<RoomNode>(Direction.North)?.GetNodeAs<RoomNode>(Direction.East) ?? GetNodeAs<RoomNode>(Direction.East)?.GetNodeAs<RoomNode>(Direction.North);

        /// <value>Returns the <see cref="RoomNode"/> that is north west of this <see cref="RoomNode"/> or null if such a node does not exist or is inaccessible.</value>
        public RoomNode NorthWest => GetNodeAs<RoomNode>(Direction.North)?.GetNodeAs<RoomNode>(Direction.West) ?? GetNodeAs<RoomNode>(Direction.West)?.GetNodeAs<RoomNode>(Direction.North);

        /// <inheritdoc/>
        public bool Obstructed => !Floor.Enabled || RoomNodeSpeed == 0;

        /// <value>Gives the <see cref="IWorldPosition"/> - typically either <see cref="SpriteObject"/> or <see cref="AdventurerPawn"/> - that is currently within this <see cref="RoomNode"/> or null if there is none.</value>
        public IWorldPosition Occupant
        {
            get => _occupant;
            set
            {
                _occupant = value;
                UpdateNearbyNodes();
                if (value is SpriteObject)
                    Room.RegisterForUpdate();
            }
        }

        /// <value>When true, this <see cref="RoomNode"/> cannot be blocked by <see cref="AdventurerPawn"/>'s performing the <see cref="WaitStep"/>, normally because this <see cref="RoomNode"/> is a major navigation path.</value>
        public bool Reserved { get; set; }

        /// <inheritdoc/>
        public Room Room { get; set; }

        /// <value>The position of this <see cref="RoomNode"/> relative to the origin of <see cref="Room"/>.</value>
        public Vector3Int RoomPosition { get; protected set; }

        /// <value>The <see cref="Scripts.Map.Sector"/> that contains this <see cref="RoomNode"/>.</value>
        public Sector Sector { get; set; }

        /// <value>Returns the <see cref="RoomNode"/> that is south east of this <see cref="RoomNode"/> or null if such a node does not exist or is inaccessible.</value>
        public RoomNode SouthEast => GetNodeAs<RoomNode>(Direction.South)?.GetNodeAs<RoomNode>(Direction.East) ?? GetNodeAs<RoomNode>(Direction.East)?.GetNodeAs<RoomNode>(Direction.South);

        /// <value>Returns the <see cref="RoomNode"/> that is south west of this <see cref="RoomNode"/> or null if such a node does not exist or is inaccessible.</value>
        public RoomNode SouthWest => GetNodeAs<RoomNode>(Direction.South)?.GetNodeAs<RoomNode>(Direction.West) ?? GetNodeAs<RoomNode>(Direction.West)?.GetNodeAs<RoomNode>(Direction.South);

        /// <value>Gives the speed multiplier to a <see cref="Pawn"/> trying to pass through this <see cref="RoomNode"/>.</value>
        public virtual float SpeedMultiplier
        {
            get
            {
                if (_speed == null)
                {
                    _speed = 0;
                    _speed += GetNodeSpeed(Direction.North);
                    _speed += GetNodeSpeed(Direction.South);
                    _speed += GetNodeSpeed(Direction.East);
                    _speed += GetNodeSpeed(Direction.West);
                    _speed += GetNodeSpeed(Direction.NorthEast);
                    _speed += GetNodeSpeed(Direction.NorthWest);
                    _speed += GetNodeSpeed(Direction.SouthEast);
                    _speed += GetNodeSpeed(Direction.SouthWest);
                    _speed *= RoomNodeSpeed / 8;
                }

                return _speed.Value;

            }
        }

        /// <value>The <see cref="Map"/> position of the walkable surface of this <see cref="RoomNode"/>. May or may not be equal to <see cref="WorldPosition"/>.</value>
        public virtual Vector3Int SurfacePosition => WorldPosition;

        //Obstructed and Traversable are typically opposites, but have subtle differences, specifically for RoomNodes.
        //Because Pawns take up a three by three area, all 9 RoomNode tiles that they are occupying must not be Obstructed in order for the Pawn to stand there.
        //Therefore, it is possible for a RoomNode to not be Obstructed, but also not be Traversable.
        /// <inheritdoc/>
        public bool Traversable
        {
            get
            {
                if (this == Undefined || this == Invalid)
                    return false;
                if (_traversable == null)
                {
                    bool test = !Obstructed;
                    test = test && (!_north?.Obstructed ?? false);
                    test = test && (!_south?.Obstructed ?? false);
                    test = test && (!_east?.Obstructed ?? false);
                    test = test && (!_west?.Obstructed ?? false);
                    test = test && (_north is ConnectingNode || _east is ConnectingNode || CornerAccessible(Direction.NorthEast));
                    test = test && (_north is ConnectingNode || _west is ConnectingNode || CornerAccessible(Direction.NorthWest));
                    test = test && (_south is ConnectingNode || _east is ConnectingNode || CornerAccessible(Direction.SouthEast));
                    test = test && (_south is ConnectingNode || _west is ConnectingNode || CornerAccessible(Direction.SouthWest));
                    _traversable = test;
                }
                return _traversable.Value;
            }
        }

        /// <inheritdoc/>
        public bool AdjacentToRoomNode(RoomNode node)
        {
            return node == this;
        }

        /// <inheritdoc/>
        public Vector3Int WorldPosition => Room.GetWorldPosition(this);

        /// <value>Gives the speed multiplier just from this <see cref="RoomNode"/>.</value>
        private float RoomNodeSpeed
        {
            get
            {
                if (!Empty)
                {
                    if (Occupant is SpriteObject spriteObject)
                        return spriteObject.SpeedMultiplier(WorldPosition);

                    return 0;
                }
                return 1;
            }
        }

        /// <value>Gives the inverse of <see cref="SpeedMultiplier"/>.</value>
        private float SpeedInverse
        {
            get
            {
                _speedInverse ??= 1 / SpeedMultiplier;
                return _speedInverse.Value;
            }
        }

        /// <inheritdoc/>
        public MapAlignment Alignment =>  MapAlignment.Center;

        /// <summary>
        /// Evaluates if a given path of <see cref="RoomNode"/>'s remains valid.
        /// </summary>
        /// <param name="path">The path composed of an <see cref="IEnumerable"/> of <see cref="RoomNode"/>s.</param>
        /// <returns>Returns true if <c>path</c> is still traversable.</returns>
        public static bool VerifyPath(IEnumerable<RoomNode> path)
        {
            IEnumerable<RoomNode> roomNodes = path.ToList();
            RoomNode previous = roomNodes.First();
            (int prevX, int prevY) = previous.Coords;
            foreach (RoomNode nextNode in roomNodes.Skip(1))
            {
                (int nextX, int nextY) = nextNode.Coords;
                switch ((nextX - prevX, nextY - prevY))
                {
                    case (0, 1):
                        if (previous.GetNode(Direction.North) != nextNode)
                            return false;
                        break;
                    case (0, -1):
                        if (previous.GetNode(Direction.South) != nextNode)
                            return false;
                        break;
                    case (1, 0):
                        if (previous.GetNode(Direction.East) != nextNode)
                            return false;
                        break;
                    case (-1, 0):
                        if (previous.GetNode(Direction.West) != nextNode)
                            return false;
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the <see cref="INode"/> in a given cardinal <see cref="Direction"/>.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public INode GetNode(Direction direction)
        {
            return direction switch
            {
                Direction.North => _north,
                Direction.South => _south,
                Direction.East => _east,
                Direction.West => _west,
                Direction.Undirected => this,
                _ => throw new System.ArgumentException("Not a valid direction for GetNode()")
            };
        }

        /// <summary>
        /// Get the <see cref="INode"/> in a given cardinal <see cref="Direction"/> as a given type.
        /// </summary>
        /// <typeparam name="T">The type to be returned. Must be a child of <see cref="INode"/>.</typeparam>
        /// <param name="direction">The <see cref="Direction"/> of the <see cref="INode"/></param>
        /// <param name="traversable">When true, will check return null if the <see cref="INode"/> cannot be traversed to from this <see cref="RoomNode"/>.
        /// Only applies when the desired type is <see cref="RoomNode"/>.</param>
        /// <returns>Returns the <see cref="INode"/> as the desired type, or null if the <see cref="INode"/> cannot be cast to that type.</returns>
        public virtual T GetNodeAs<T>(Direction direction, bool traversable = true) where T : INode
        {
            INode node = GetNode(direction);

            if (node is T tNode)
            {
                if (typeof(T) != typeof(RoomNode) || !traversable)
                    return tNode;
                else
                {
                    if (node is StairNode stairNode && stairNode.Direction != direction)
                    {
                        if (stairNode.Direction == ~direction && stairNode.WorldPosition.z == WorldPosition.z - 1)
                        {
                            return tNode;
                        }
                    }
                    else
                    {
                        RoomNode roomNode = (RoomNode)node;
                        if (roomNode != Undefined && roomNode.WorldPosition.z == WorldPosition.z)
                            return tNode;
                    }
                }
            }
            else if (node is DoorConnector connector && typeof(T) == typeof(WallBlocker))
            {
                return (T)(INode)connector.WallNode;
            }
            return default;
        }

        /// <summary>
        /// Finds the <see cref="RoomNode"/> that is directly in the given direction, ignoring any <see cref="IDividerNode"/>s that may be in between.
        /// </summary>
        /// <param name="direction">The <see cref="Direction"/> of the desired <see cref="RoomNode"/>.</param>
        /// <returns>Returns the desired <see cref="RoomNode"/>.</returns>
        /// <exception cref="System.ArgumentException">Thrown when <c>direction</c> is not a valid <see cref="Direction"/>.</exception>
        public RoomNode GetRoomNode(Direction direction)
        { 
            switch(direction)
            {
                case Direction.North:
                case Direction.South:
                case Direction.East:
                case Direction.West:
                    INode node = GetNode(direction);
                    if (node is IDividerNode divider)
                    {
                        if (divider is LandingConnector landing)
                            return landing.GetOppositeRoomNode(this);
                        return divider.GetOppositeRoomNode(this);
                    }

                    return node as RoomNode;
                default:
                    return direction switch
                    {
                        Direction.NorthEast => NorthEast,
                        Direction.SouthEast => SouthEast,
                        Direction.NorthWest => NorthWest,
                        Direction.SouthWest => SouthWest,
                        Direction.Undirected => this,
                        _ => throw new System.ArgumentException("Not a valid direction for GetRoomNode()")
                    };
            }
        }

        /// <inheritdoc/>
        public bool HasNavigatedTo(RoomNode node)
        {
            return node == this;
        }

        /// <summary>
        /// Used to change which <see cref="Scripts.Map.Room"/> this <see cref="RoomNode"/> is within.
        /// </summary>
        /// <param name="room">The <see cref="Scripts.Map.Room"/> this <see cref="RoomNode"/> is now in.</param>
        /// <param name="x">The x position of this <see cref="RoomNode"/> relative to the given <see cref="Scripts.Map.Room"/>'s origin.</param>
        /// <param name="y">The y position of this <see cref="RoomNode"/> relative to the given <see cref="Scripts.Map.Room"/>'s origin.</param>
        public void Reassign(Room room, int x, int y)
        {
            Room = room;
            RoomPosition = new Vector3Int(x, y, RoomPosition.z);
        }

        /// <summary>
        /// Reset's the given adjacent <see cref="INode"/> to the <see cref="RoomNode"/> that is directly adjacent to this <see cref="RoomNode"/>.
        /// Used whenever an <see cref="IDividerNode"/> is removed and no longer separates the adjacent <see cref="RoomNode"/>s.
        /// </summary>
        /// <param name="direction">The <see cref="Direction"/> of the <see cref="INode"/> being reset.</param>
        public void ResetConnection(Direction direction)
        {
            SetNode(direction, Map.Instance[WorldPosition + Utility.Utility.DirectionToVector(direction)]);
        }

        /// <summary>
        /// Sets the <see cref="INode"/> that is adjacent to this <see cref="RoomNode"/> in the given cardinal <see cref="Direction"/>.
        /// </summary>
        /// <param name="direction"><see cref="Direction"/> of the <see cref="INode"/> being set.</param>
        /// <param name="node">The <see cref="INode"/> being set.</param>
        /// <param name="recursive">When true, the given <see cref="INode"/> will also set this <see cref="RoomNode"/> to be its adjacent <see cref="INode"/>.</param>
        public void SetNode(Direction direction, INode node, bool recursive = true)
        {
            if (this == Undefined || this == Invalid)
                return;

            bool checkContiguous = false;
            RoomNode node2 = null;
            if (node is WallBlocker && TryGetNodeAs(direction, out node2, false) && recursive)
            {
                checkContiguous = true;
                node2.SetNode(~direction, node, false);
            }
            else if (node is RoomNode roomNode && recursive)
                roomNode.SetNode(~direction, this, false);

            switch (direction)
            {
                case Direction.North:
                    _north = node;
                    break;
                case Direction.South:
                    _south = node;
                    break;
                case Direction.East:
                    _east = node;
                    break;
                case Direction.West:
                    _west = node;
                    break;
            }

            if (checkContiguous)
            {
                Room.CheckContiguous(this, node2);
            }

            UpdateNearbyNodes();
        }

        /// <summary>
        /// Used to change the vertical position of this <see cref="RoomNode"/>.
        /// </summary>
        /// <param name="z">The position, relative to <see cref="Room"/>'s origin to set this <see cref="RoomNode"/> to.</param>
        public void SetZ(int z)
        {
            RoomPosition = new Vector3Int(RoomPosition.x, RoomPosition.y, z);
            if (z > 0)
            {
                if (Map.Instance[z] != null && Map.Instance[z].Origin.z == z)
                    Map.Instance[WorldPosition].Floor.Enabled = false;
                else
                    Map.Instance[WorldPosition, 1].Floor.Enabled = false;
            }
        }

        /// <summary>
        /// Try to get the <see cref="INode"/> in a given cardinal <see cref="Direction"/> as a given type.
        /// </summary>
        /// <typeparam name="T">The type to be returned. Must be a child of <see cref="INode"/>.</typeparam>
        /// <param name="direction">The <see cref="Direction"/> of the <see cref="INode"/></param>
        /// <param name="node">A reference to set the desired <see cref="INode"/> to.</param>
        /// <param name="traversable">When true, will check return null if the <see cref="INode"/> cannot be traversed to from this <see cref="RoomNode"/>.
        /// Only applies when the desired type is <see cref="RoomNode"/>.</param>
        /// <returns>Returns true if the desired <see cref="INode"/> can be cast to the given type.</returns>
        public bool TryGetNodeAs<T>(Direction direction, out T node, bool traversable = true) where T : INode
        {
            node = GetNodeAs<T>(direction, traversable);
            return node != null;
        }

        /// <summary>
        /// Try to get the <see cref="INode"/> in a given cardinal <see cref="Direction"/> as a given type.
        /// </summary>
        /// <typeparam name="T">The type to be returned. Must be a child of <see cref="INode"/>.</typeparam>
        /// <param name="direction">The <see cref="Direction"/> of the <see cref="INode"/></param>
        /// <param name="traversable">When true, will check return null if the <see cref="INode"/> cannot be traversed to from this <see cref="RoomNode"/>.
        /// Only applies when the desired type is <see cref="RoomNode"/>.</param>
        /// <returns>Returns true if the desired <see cref="INode"/> can be cast to the given type.</returns>
        public bool TryGetNodeAs<T>(Direction direction, bool traversable = true) where T : INode
        {
            return GetNodeAs<T>(direction, traversable) != null;
        }

        /// <summary>
        /// Determines whether a given corner is directly accessible from this <see cref="RoomNode"/> with no <see cref="WallBlocker"/> blocking the path.
        /// </summary>
        /// <param name="corner">The corner being evaluated.</param>
        /// <returns>Returns true if the corner is accessible.</returns>
        /// <exception cref="System.ArgumentException">Thrown if <c>corner</c> is not a corner <see cref="Direction"/>.</exception>
        private bool CornerAccessible(Direction corner)
        {
            switch (corner)
            {
                case Direction.NorthEast:
                    RoomNode cornerNode = NorthEast;
                    if (cornerNode == null)
                        return false;
                    return !cornerNode.Obstructed && !cornerNode._south.Obstructed && !cornerNode._west.Obstructed;
                case Direction.NorthWest:
                    cornerNode = NorthWest;
                    if (cornerNode == null)
                        return false;
                    return !cornerNode.Obstructed && !cornerNode._south.Obstructed && !cornerNode._east.Obstructed;
                case Direction.SouthEast:
                    cornerNode = SouthEast;
                    if (cornerNode == null)
                        return false;
                    return !cornerNode.Obstructed && !cornerNode._north.Obstructed && !cornerNode._west.Obstructed;
                case Direction.SouthWest:
                    cornerNode = SouthWest;
                    if (cornerNode == null)
                        return false;
                    return !cornerNode.Obstructed && !cornerNode._north.Obstructed && !cornerNode._east.Obstructed;
            }
            throw new System.ArgumentException("Invalid Direction for CornerAccessible()");
        }

        private float GetNodeSpeed(Direction direction)
        {
            Vector3Int vector = Utility.Utility.DirectionToVector(direction);
            RoomNode roomNode = this;
            if (vector.x != 0)
            {
                INode node = GetNode(vector.x > 0 ? Direction.East : Direction.West);
                if (node is IDividerNode divider)
                {
                    if (divider is LandingConnector landing)
                        roomNode = landing.GetOppositeRoomNode(this);
                    else
                        roomNode = divider.GetOppositeRoomNode(this);
                }
                else
                    roomNode = node as RoomNode;
            }

            if (vector.y != 0)
            {
                INode node = GetNode(vector.y > 0 ? Direction.North : Direction.South);
                if (node is IDividerNode divider)
                {
                    if (divider is LandingConnector landing)
                        roomNode = landing.GetOppositeRoomNode(this);
                    else
                        roomNode = divider.GetOppositeRoomNode(this);
                }
                else
                    roomNode = node as RoomNode;
            }

            return roomNode?.RoomNodeSpeed ?? 0.25f;
        }
        /// <summary>
        /// Notifies all nearby <see cref="RoomNode"/>s that they will need to update their <see cref="NextNodes"/> and <see cref="AdjacentNodes"/>lists (including itself).
        /// </summary>
        private void UpdateNearbyNodes()
        {
            for (int i = -3; i <= 3; i++)
            {
                for (int j = -3; j <= 3; j++)
                {
                    if(Map.Instance[WorldPosition + new Vector3Int(i, j)] != null)
                        Map.Instance[WorldPosition + new Vector3Int(i, j)]._nextNodesKnown = false;
                }
            }
            for(int i = -1; i <= 1; i++)
            {
                for(int j = -1; j <= 1; j++)
                {
                    if (Map.Instance[WorldPosition + new Vector3Int(i, j)] != null)
                    {
                        Map.Instance[WorldPosition + new Vector3Int(i, j)]._traversable = null;
                        Map.Instance[WorldPosition + new Vector3Int(i, j)]._speed = null;
                        Map.Instance[WorldPosition + new Vector3Int(i, j)]._speedInverse = null;
                    }
                }
            }
        }
    }
}