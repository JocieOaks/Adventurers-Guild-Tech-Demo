using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The <see cref="RoomNode"/> class is an <see cref="INode"/> that exists as a coordinate location within a <see cref="Map.Room"/>.
/// </summary>
public class RoomNode : INode
{


    static readonly float RAD2_2 = Mathf.Sqrt(2) / 2;
    static readonly float RAD6_4 = Mathf.Sqrt(5) / 4;
    readonly List<INode> _adjacentNodes = new();

    readonly List<(RoomNode, float)> _nextNodes = new();
    Graphics.Corner _corner;
    bool _nextNodesKnown = false;

    INode _north, _south, _east, _west;

    IWorldPosition _occupant;

    float? _speed = null;
    float? _speedInverse = null;
    bool? _traversible;

    /// <summary>
    /// Initializes a new instance of the <see cref ="RoomNode"/> class.
    /// </summary>
    /// <param name="room">The <see cref="Map.Room"/> containing the <see cref="RoomNode"/>.</param>
    /// <param name="x">The x coordinate of the <see cref="RoomNode"/> within the <see cref="Map.Room"/>.</param>
    /// <param name="y">The y coordinate of the <see cref="RoomNode"/> within the <see cref="Map.Room"/>.</param>
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
    public static RoomNode Invalid { get; } = new RoomNode(null, -1, -1);

    /// <value>Static property that returns a special <see cref="RoomNode"/> that serves as a flag.</value>
    public static RoomNode Undefined { get; } = new RoomNode(null, -2, -2);

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

    /// <value>A quick accessor for the x and y room positions of the <see cref="RoomNode"/> as a tuple of two ints.</value>
    public (int x, int y) Coords => (RoomPosition.x, RoomPosition.y);

    /// <value>Determines if a <see cref="SpriteObject"/>is currently placed at the <see cref ="RoomNode"/>.</value>
    public bool Empty
    {
        get => _occupant == null;
    }

    /// <value> Returns the <see cref="global::FloorSprite"/> <see cref="SpriteObject"/> associated with this <see cref="RoomNode"/>.</value>
    public FloorSprite Floor { get; }

    /// <value>The <see cref="List{T}"/> of all <see cref="RoomNode"/>'s that are a single step away from this <see cref="RoomNode"/> in terms of <see cref="AdventurerPawn"/> navigation.
    /// If the list is not already made it will be created.</value>
    public IEnumerable NextNodes
    {
        get
        {
            lock (_nextNodes)
            {
                if (!_nextNodesKnown)
                {
                    _nextNodes.Clear();

                    bool isTraversibleNorth = TryGetNodeAs(Direction.North, out RoomNode north) && north.Traversable;
                    bool isTraversibleSouth = TryGetNodeAs(Direction.South, out RoomNode south) && south.Traversable;
                    bool isTraversibleEast = TryGetNodeAs(Direction.East, out RoomNode east) && east.Traversable;
                    bool isTraversibleWest = TryGetNodeAs(Direction.West, out RoomNode west) && west.Traversable;

                    if (isTraversibleNorth)
                    {
                        _nextNodes.Add((north, (SpeedInverse + north.SpeedInverse) / 2));
                    }
                    if (isTraversibleSouth)
                    {
                        _nextNodes.Add((south, (SpeedInverse + south.SpeedInverse) / 2));
                    }
                    if (isTraversibleEast)
                    {
                        _nextNodes.Add((east, (SpeedInverse + east.SpeedInverse) / 2));
                    }
                    if (isTraversibleWest)
                    {
                        _nextNodes.Add((west, (SpeedInverse + west.SpeedInverse) / 2));
                    }

                    bool isTraversibleNorthEast = NorthEast?.Traversable ?? false;
                    bool isTraversibleNorthWest = NorthWest?.Traversable ?? false;
                    bool isTraversibleSouthEast = SouthEast?.Traversable ?? false;
                    bool isTraversibleSouthWest = SouthWest?.Traversable ?? false;

                    if (isTraversibleNorthEast)
                    {
                        _nextNodes.Add((NorthEast, RAD2_2 * (SpeedInverse + NorthEast.SpeedInverse)));
                    }
                    if (isTraversibleNorthWest)
                    {
                        _nextNodes.Add((NorthWest, RAD2_2 * (SpeedInverse + NorthWest.SpeedInverse)));
                    }
                    if (isTraversibleSouthEast)
                    {
                        _nextNodes.Add((SouthEast, RAD2_2 * (SpeedInverse + SouthEast.SpeedInverse)));
                    }
                    if (isTraversibleSouthWest)
                    {
                        _nextNodes.Add((SouthWest, RAD2_2 * (SpeedInverse + SouthWest.SpeedInverse)));
                    }

                    if (isTraversibleNorth && isTraversibleNorthEast)
                    {
                        if (NorthEast.TryGetNodeAs(Direction.North, out RoomNode northNorthEast) && northNorthEast.Traversable)
                            _nextNodes.Add((northNorthEast, RAD6_4 * (SpeedInverse + north.SpeedInverse + NorthEast.SpeedInverse + northNorthEast.SpeedInverse)));
                    }
                    if (isTraversibleNorth && isTraversibleNorthWest)
                    {
                        if (NorthWest.TryGetNodeAs(Direction.North, out RoomNode northNorthWest) && northNorthWest.Traversable)
                            _nextNodes.Add((northNorthWest, RAD6_4 * (SpeedInverse + north.SpeedInverse + NorthWest.SpeedInverse + northNorthWest.SpeedInverse)));
                    }
                    if (isTraversibleSouth && isTraversibleSouthEast)
                    {
                        if (SouthEast.TryGetNodeAs(Direction.South, out RoomNode southSouthEast) && southSouthEast.Traversable)
                            _nextNodes.Add((southSouthEast, RAD6_4 * (SpeedInverse + south.SpeedInverse + SouthEast.SpeedInverse + southSouthEast.SpeedInverse)));
                    }
                    if (isTraversibleSouth && isTraversibleSouthWest)
                    {
                        if (SouthWest.TryGetNodeAs(Direction.South, out RoomNode southSouthWest) && southSouthWest.Traversable)
                            _nextNodes.Add((southSouthWest, RAD6_4 * (SpeedInverse + south.SpeedInverse + SouthWest.SpeedInverse + southSouthWest.SpeedInverse)));
                    }
                    if (isTraversibleEast && isTraversibleNorthEast)
                    {
                        if (NorthEast.TryGetNodeAs(Direction.East, out RoomNode eastNorthEast) && eastNorthEast.Traversable)
                            _nextNodes.Add((eastNorthEast, RAD6_4 * (SpeedInverse + east.SpeedInverse + NorthEast.SpeedInverse + eastNorthEast.SpeedInverse)));
                    }
                    if (isTraversibleWest && isTraversibleNorthWest)
                    {
                        if (NorthWest.TryGetNodeAs(Direction.West, out RoomNode westNorthWest) && westNorthWest.Traversable)
                            _nextNodes.Add((westNorthWest, RAD6_4 * (SpeedInverse + west.SpeedInverse + NorthWest.SpeedInverse + westNorthWest.SpeedInverse)));
                    }
                    if (isTraversibleEast && isTraversibleSouthEast)
                    {
                        if (SouthEast.TryGetNodeAs(Direction.East, out RoomNode eastSouthEast) && eastSouthEast.Traversable)
                            _nextNodes.Add((eastSouthEast, RAD6_4 * (SpeedInverse + east.SpeedInverse + SouthEast.SpeedInverse + eastSouthEast.SpeedInverse)));
                    }
                    if (isTraversibleWest && isTraversibleSouthWest)
                    {
                        if (SouthWest.TryGetNodeAs(Direction.West, out RoomNode westSouthWest) && westSouthWest.Traversable)
                            _nextNodes.Add((westSouthWest, RAD6_4 * (SpeedInverse + west.SpeedInverse + SouthWest.SpeedInverse + westSouthWest.SpeedInverse)));
                    }
                }
            }
            _nextNodesKnown = true;
            return _nextNodes;
        }
    }

    /// <inheritdoc/>
    public INode Node => this;

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

    /// <value>The <see cref="global::Sector"/> that contains this <see cref="RoomNode"/>.</value>
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
    //Because Pawns take up a three by three area, all 9 RoomNode tiles that they are ocupying must not be Obstructed in order for the Pawn to stand there.
    //Therefore, it is possible for a RoomNode to not be Obstructed, but also not be Traversible.
    /// <inheritdoc/>
    public bool Traversable
    {
        get
        {
            try
            {
                if (this == Undefined || this == Invalid)
                    return false;
                if (_traversible == null)
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
                    _traversible = test;
                }
                return _traversible.Value;
            }
            catch (System.NullReferenceException e)
            {
                throw e;
            }
        }
    }

    /// <inheritdoc/>
    public Vector3Int WorldPosition => Room.GetWorldPosition(this);

    /// <value>Gives the speed multiplier just from this <see cref="RoomNode"/>.</value>
    float RoomNodeSpeed
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
    float SpeedInverse
    {
        get
        {
            if(_speedInverse == null)
            {
                _speedInverse = 1 / SpeedMultiplier;
            }
            return _speedInverse.Value;
        }
    }

    public Vector3Int Dimensions => new Vector3Int(1,1,Room.Height);

    /// <summary>
    /// Evaluates if a given path of <see cref="RoomNode"/>'s remains valid.
    /// </summary>
    /// <param name="path">The path composed of an <see cref="IEnumerable"/> of <see cref="RoomNode"/>s.</param>
    /// <returns>Returns true if <c>path</c> is still traversible.</returns>
    public static bool VerifyPath(IEnumerable<RoomNode> path)
    {
        RoomNode previous = path.First();
        (int prevX, int prevY) = previous.Coords;
        foreach (RoomNode nextNode in path.Skip(1))
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
                    return divider.GetOppositeRoomNode(this);
                else
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

    /// <summary>
    /// Get the <see cref="INode"/> in a given cardinal <see cref="Direction"/> as a given type.
    /// </summary>
    /// <typeparam name="T">The type to be returned. Must be a child of <see cref="INode"/>.</typeparam>
    /// <param name="direction">The <see cref="Direction"/> of the <see cref="INode"/></param>
    /// <param name="traversible">When true, will check return null if the <see cref="INode"/> cannot be traversed to from this <see cref="RoomNode"/>.
    /// Only applies when the desired type is <see cref="RoomNode"/>.</param>
    /// <returns>Returns the <see cref="INode"/> as the desired type, or null if the <see cref="INode"/> cannot be cast to that type.</returns>
    public virtual T GetNodeAs<T>(Direction direction, bool traversible = true) where T : INode
    {
        INode node = GetNode(direction);

        if (node is T tNode)
        {
            if (typeof(T) != typeof(RoomNode) || !traversible)
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
        else if (node is DoorConnector && typeof(T) == typeof(WallBlocker))
        {
            return (T)(INode)(node as DoorConnector).WallNode;
        }
        return default;
    }

    /// <inheritdoc/>
    public bool HasNavigatedTo(RoomNode node)
    {
        return node == this;
    }

    /// <summary>
    /// Used to change which <see cref="global::Room"/> this <see cref="RoomNode"/> is within.
    /// </summary>
    /// <param name="room">The <see cref="global::Room"/> this <see cref="RoomNode"/> is now in.</param>
    /// <param name="x">The x position of this <see cref="RoomNode"/> relative to the given <see cref="global::Room"/>'s origin.</param>
    /// <param name="y">The y position of this <see cref="RoomNode"/> relative to the given <see cref="global::Room"/>'s origin.</param>
    public void Reassign(Room room, int x, int y)
    {
        Room = room;
        RoomPosition = new Vector3Int(x, y, RoomPosition.z);
    }

    /// <summary>
    /// Reset's the given adjacent <see cref="INode"/> to the <see cref="RoomNode"/> that is directly adjacent to this <see cref="RoomNode"/>.
    /// Useed whenever an <see cref="IDividerNode"/> is removed and no longer separates the adjacent <see cref="RoomNode"/>s.
    /// </summary>
    /// <param name="direction">The <see cref="Direction"/> of the <see cref="INode"/> being reset.</param>
    public void ResetConnection(Direction direction)
    {
        SetNode(direction, Map.Instance[WorldPosition + Utility.DirectionToVector(direction)]);
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
    /// Trye to get the <see cref="INode"/> in a given cardinal <see cref="Direction"/> as a given type.
    /// </summary>
    /// <typeparam name="T">The type to be returned. Must be a child of <see cref="INode"/>.</typeparam>
    /// <param name="direction">The <see cref="Direction"/> of the <see cref="INode"/></param>
    /// <param name="node">A reference to set the desired <see cref="INode"/> to.</param>
    /// <param name="traversible">When true, will check return null if the <see cref="INode"/> cannot be traversed to from this <see cref="RoomNode"/>.
    /// Only applies when the desired type is <see cref="RoomNode"/>.</param>
    /// <returns>Returns true if the desired <see cref="INode"/> can be cast to the given type.</returns>
    public bool TryGetNodeAs<T>(Direction direction, out T node, bool traversible = true) where T : INode
    {
        node = GetNodeAs<T>(direction, traversible);
        return node != null;
    }

    /// <summary>
    /// Trye to get the <see cref="INode"/> in a given cardinal <see cref="Direction"/> as a given type.
    /// </summary>
    /// <typeparam name="T">The type to be returned. Must be a child of <see cref="INode"/>.</typeparam>
    /// <param name="direction">The <see cref="Direction"/> of the <see cref="INode"/></param>
    /// <param name="traversible">When true, will check return null if the <see cref="INode"/> cannot be traversed to from this <see cref="RoomNode"/>.
    /// Only applies when the desired type is <see cref="RoomNode"/>.</param>
    /// <returns>Returns true if the desired <see cref="INode"/> can be cast to the given type.</returns>
    public bool TryGetNodeAs<T>(Direction direction, bool traversible = true) where T : INode
    {
        return GetNodeAs<T>(direction, traversible) != null;
    }

    /// <summary>
    /// Determines whether a given corner is directly accessible from this <see cref="RoomNode"/> with no <see cref="WallBlocker"/> blocking the path.
    /// </summary>
    /// <param name="corner">The corner being evaluated.</param>
    /// <returns>Returns true if the corner is accessible.</returns>
    /// <exception cref="System.ArgumentException">Thrown if <c>corner</c> is not a corner <see cref="Direction"/>.</exception>
    bool CornerAccessible(Direction corner)
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

    float GetNodeSpeed(Direction direction)
    {
        Vector3Int vector = Map.DirectionToVector(direction);
        RoomNode roomNode = this;
        if (vector.x != 0)
        {
            INode node = GetNode(vector.x > 0 ? Direction.East : Direction.West);
            if (node is IDividerNode divider)
            {
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
                roomNode = divider.GetOppositeRoomNode(this);
            }
            else
                roomNode = node as RoomNode;
        }

        return roomNode.RoomNodeSpeed;
    }
    /// <summary>
    /// Notifies all nearby <see cref="RoomNode"/>s that they will need to update their <see cref="NextNodes"/> and <see cref="AdjacentNodes"/>lists (including itself).
    /// </summary>
    void UpdateNearbyNodes()
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
                    Map.Instance[WorldPosition + new Vector3Int(i, j)]._traversible = null;
                    Map.Instance[WorldPosition + new Vector3Int(i, j)]._speed = null;
                    Map.Instance[WorldPosition + new Vector3Int(i, j)]._speedInverse = null;
                }
                Map.Instance[WorldPosition + new Vector3Int(i, j)]?._adjacentNodes.Clear();
            }
        }
    }
}