using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A <see cref="INode"/> that exists as a space within a <see cref="Map.Room"/>.
/// </summary>
public class RoomNode : INode
{
    //Special RoomNode that serves as a flag.
    static float rad2 = Mathf.Sqrt(2);
    static float rad5 = Mathf.Sqrt(5);

    ///<value>Static property that returns a special <see cref="RoomNode"/> that serves as a flag.</value>
    public static RoomNode Invalid { get; } = new RoomNode(null, -1, -1);
    public static RoomNode Undefined { get; } = new RoomNode(null, -2, -2);

    public const int NORTHEAST = 0, NORTHWEST = 1, SOUTHEAST = 2, SOUTHWEST = 3;
    INode _north, _south, _east, _west;
    bool? _traversible;
    Graphics.Corner _corner;

    public (int x, int y) Coords => (RoomPosition.x, RoomPosition.y);

    public Graphics.Corner Corner
    {
        get
        {
            if(_corner == null)
            {
                _corner = Object.Instantiate(Graphics.Instance.SpritePrefab).AddComponent<Graphics.Corner>();
                _corner.gameObject.name = "Corner";
                _corner.enabled = false;
            }
            return _corner;
        }
    }

    public Floor Floor{ get; }

    public Vector3Int RoomPosition { get; protected set; }

    public virtual Vector3Int SurfacePosition => WorldPosition;

    /// <summary>
    /// Initializes a new instance of the <see cref ="RoomNode"/> class.
    /// </summary>
    /// /// <param name="room">The <see cref="Map.Room"/> containing the <see cref="RoomNode"/>.</param>
    /// <param name="x">The x coordinate of the <see cref="RoomNode"/> within the <see cref="Map.Room"/>.</param>
    /// <param name="y">The y coordinate of the <see cref="RoomNode"/> within the <see cref="Map.Room"/>.</param>
    public RoomNode(Room room, int x, int y)
    {
        Room = room;
        RoomPosition = new Vector3Int(x, y);
        if(x < 0 || y < 0)
            Floor = new Floor(Vector3Int.back);
        else
            Floor = new Floor(WorldPosition);
    }

    public RoomNode(RoomNode node)
    {
        Room = node.Room;
        RoomPosition = node.RoomPosition;
        Floor = node.Floor;
        SetNode(Direction.North, node._north);
        SetNode(Direction.South, _south = node._south);
        SetNode(Direction.East, _east = node._east);
        SetNode(Direction.West, _west = node._west);
        _corner = node._corner;

        Room.ReplaceNode(RoomPosition.x, RoomPosition.y, this);
        Map.Instance[Room.Origin.z].ReplaceNode(WorldPosition.x, WorldPosition.y, this);
    }

    public RoomNode NorthEast => GetNodeAs<RoomNode>(Direction.North)?.GetNodeAs<RoomNode>(Direction.East) ?? GetNodeAs<RoomNode>(Direction.East)?.GetNodeAs<RoomNode>(Direction.North);
    public RoomNode NorthWest => GetNodeAs<RoomNode>(Direction.North)?.GetNodeAs<RoomNode>(Direction.West) ?? GetNodeAs<RoomNode>(Direction.West)?.GetNodeAs<RoomNode>(Direction.North);

    public RoomNode SouthEast => GetNodeAs<RoomNode>(Direction.South)?.GetNodeAs<RoomNode>(Direction.East) ?? GetNodeAs<RoomNode>(Direction.East)?.GetNodeAs<RoomNode>(Direction.South);
    public RoomNode SouthWest => GetNodeAs<RoomNode>(Direction.South)?.GetNodeAs<RoomNode>(Direction.West) ?? GetNodeAs<RoomNode>(Direction.West)?.GetNodeAs<RoomNode>(Direction.South);

    /// <value>Property <c>Empty</c> determines if an object is currently placed at the <see cref ="RoomNode"/>.</value>
    public bool Empty 
    { 
        get => _occupant == null;
    }

    SpriteObject _occupant;

    public SpriteObject Occupant
    {
        get => _occupant;
        set
        {
            _occupant = value;
            UpdateNearbyNextNodes();
        }
    }

    Pawn _standing = null;

    public Pawn Standing 
    { 
        get => _standing; 
        
        set 
        {
            _standing = value;
            _traversible = null;
            UpdateNearbyNextNodes();
        }
    }
 

    /// <value>Property <c>Room</c> respresents the <see cref="Map.Room"/> containing the <see cref="RoomNode"/>.</value>
    public Room Room { get; set; }

   

    public bool Traversible
    {
        get
        {
            try
            {
                if (this == Undefined || this == Invalid)
                    return false;
                if (_traversible == null)
                {
                    bool test = Floor.Enabled && Empty && Standing == null;
                    test = test && (_north is not Wall && _north != Undefined && (_north is ConnectionNode || (TryGetNodeAs<RoomNode>(Direction.North) && GetNodeAs<RoomNode>(Direction.North).Empty)));
                    test = test && (_south is not Wall && _south != Undefined && (_south is ConnectionNode || (TryGetNodeAs<RoomNode>(Direction.South) && GetNodeAs<RoomNode>(Direction.South).Empty)));
                    test = test && (_east is not Wall && _east != Undefined && (_east is ConnectionNode || (TryGetNodeAs<RoomNode>(Direction.East) && GetNodeAs<RoomNode>(Direction.East).Empty)));
                    test = test && (_west is not Wall && _west != Undefined && (_west is ConnectionNode || (TryGetNodeAs<RoomNode>(Direction.West) && GetNodeAs<RoomNode>(Direction.West).Empty)));
                    test = test && (_north is ConnectionNode || _east is ConnectionNode || CornerAccessible(NORTHEAST));
                    test = test && (_north is ConnectionNode || _west is ConnectionNode || CornerAccessible(NORTHWEST));
                    test = test && (_south is ConnectionNode || _east is ConnectionNode || CornerAccessible(SOUTHEAST));
                    test = test && (_south is ConnectionNode || _west is ConnectionNode || CornerAccessible(SOUTHWEST));
                    _traversible = test;
                }
                return _traversible.Value;
            }
            catch(System.NullReferenceException e)
            {
                throw e;
            }
        }

        set
        {
            _traversible = value;
            UpdateNearbyNextNodes();
        }
    }

    

   
    ///<value>Property <c>WorldPosition</c> represents the position of the <see cref="RoomNode"/> within the overworld.</value>
    public Vector3Int WorldPosition => Room.GetWorldPosition(this);

    public bool CornerAccessible(int corner)
    {
        try
        {
            switch (corner)
            {
                case NORTHEAST:
                    RoomNode cornerNode = NorthEast;
                    if (cornerNode == null)
                        return false;
                    return cornerNode != Invalid && cornerNode != Undefined && cornerNode.Floor.Enabled && cornerNode._south is not Wall && cornerNode._west is not Wall;
                case NORTHWEST:
                    cornerNode = NorthWest;
                    if (cornerNode == null)
                        return false;
                    return cornerNode != Invalid && cornerNode != Undefined && cornerNode.Floor.Enabled && cornerNode._south is not Wall && cornerNode._east is not Wall;
                case SOUTHEAST:
                    cornerNode = SouthEast;
                    if (cornerNode == null)
                        return false;
                    return cornerNode != Invalid && cornerNode != Undefined && cornerNode.Floor.Enabled && cornerNode._north is not Wall && cornerNode._west is not Wall;
                case SOUTHWEST:
                    cornerNode = SouthWest;
                    if (cornerNode == null)
                        return false;
                    return cornerNode != Invalid && cornerNode != Undefined && cornerNode.Floor.Enabled && cornerNode._north is not Wall && cornerNode._east is not Wall;
            }
        }catch(UnityException)
        {

        }
        throw new System.ArgumentException();
    }

    public void Reassign(Room room, int x, int y)
    {
        Room = room;
        RoomPosition = new Vector3Int(x, y, RoomPosition.z);
    }

    public void SetZ(int z)
    {
        RoomPosition = new Vector3Int(RoomPosition.x, RoomPosition.y, z);
        if(z > 0)
        {
            if (Map.Instance[z] != null && Map.Instance[z].Origin.z == z)
                Map.Instance[WorldPosition].Floor.Enabled = false;
            else
                Map.Instance[WorldPosition, 1].Floor.Enabled = false;
        }
    }

    bool _nextNodesKnown = false;
    List<(RoomNode, float)> _nextNodes = new List<(RoomNode, float)>();

    /// <summary>
    /// Returns the list of all nodes that are one step away from the current node.
    /// If the list is not already made it will create it.
    /// </summary>
    /// <returns>Returns the list of all nodes that are one step away from the current node.</returns>
    public IEnumerable NextNodes
    {
        get
        {
            lock (_nextNodes)
            {
                if (!_nextNodesKnown)
                {
                    _nextNodes.Clear();

                    bool north = GetNodeAs<RoomNode>(Direction.North)?.Traversible ?? false;
                    bool south = GetNodeAs<RoomNode>(Direction.South)?.Traversible ?? false;
                    bool east = GetNodeAs<RoomNode>(Direction.East)?.Traversible ?? false;
                    bool west = GetNodeAs<RoomNode>(Direction.West)?.Traversible ?? false;

                    if (north)
                    {
                        _nextNodes.Add((GetNodeAs<RoomNode>(Direction.North), 1));
                    }
                    if (south)
                    {
                        _nextNodes.Add((GetNodeAs<RoomNode>(Direction.South), 1));
                    }
                    if (east)
                    {
                        _nextNodes.Add((GetNodeAs<RoomNode>(Direction.East), 1));
                    }
                    if (west)
                    {
                        _nextNodes.Add((GetNodeAs<RoomNode>(Direction.West), 1));
                    }

                    bool NE = GetNodeAs<RoomNode>(Direction.North)?.GetNodeAs<RoomNode>(Direction.East)?.Traversible ?? false;
                    bool NW = GetNodeAs<RoomNode>(Direction.North)?.GetNodeAs<RoomNode>(Direction.West)?.Traversible ?? false;
                    bool SE = GetNodeAs<RoomNode>(Direction.South)?.GetNodeAs<RoomNode>(Direction.East)?.Traversible ?? false;
                    bool SW = GetNodeAs<RoomNode>(Direction.South)?.GetNodeAs<RoomNode>(Direction.West)?.Traversible ?? false;

                    if (NE)
                    {
                        _nextNodes.Add((GetNodeAs<RoomNode>(Direction.North).GetNodeAs<RoomNode>(Direction.East), rad2));
                    }
                    if (NW)
                    {
                        _nextNodes.Add((GetNodeAs<RoomNode>(Direction.North).GetNodeAs<RoomNode>(Direction.West), rad2));
                    }
                    if (SE)
                    {
                        _nextNodes.Add((GetNodeAs<RoomNode>(Direction.South).GetNodeAs<RoomNode>(Direction.East), rad2));
                    }
                    if (SW)
                    {
                        _nextNodes.Add((GetNodeAs<RoomNode>(Direction.South).GetNodeAs<RoomNode>(Direction.West), rad2));
                    }

                    if (north && NE)
                    {
                        if (GetNodeAs<RoomNode>(Direction.North)?.GetNodeAs<RoomNode>(Direction.East)?.GetNodeAs<RoomNode>(Direction.North)?.Traversible ?? false)
                            _nextNodes.Add((GetNodeAs<RoomNode>(Direction.North).GetNodeAs<RoomNode>(Direction.East).GetNodeAs<RoomNode>(Direction.North), rad5));
                    }
                    if (north && NW)
                    {
                        if (GetNodeAs<RoomNode>(Direction.North)?.GetNodeAs<RoomNode>(Direction.West)?.GetNodeAs<RoomNode>(Direction.North)?.Traversible ?? false)
                            _nextNodes.Add((GetNodeAs<RoomNode>(Direction.North).GetNodeAs<RoomNode>(Direction.West).GetNodeAs<RoomNode>(Direction.North), rad5));
                    }
                    if (south && SE)
                    {
                        if (GetNodeAs<RoomNode>(Direction.South)?.GetNodeAs<RoomNode>(Direction.East)?.GetNodeAs<RoomNode>(Direction.South)?.Traversible ?? false)
                            _nextNodes.Add((GetNodeAs<RoomNode>(Direction.South).GetNodeAs<RoomNode>(Direction.East).GetNodeAs<RoomNode>(Direction.South), rad5));
                    }
                    if (south && SW)
                    {
                        if (GetNodeAs<RoomNode>(Direction.South)?.GetNodeAs<RoomNode>(Direction.West)?.GetNodeAs<RoomNode>(Direction.South)?.Traversible ?? false)
                            _nextNodes.Add((GetNodeAs<RoomNode>(Direction.South).GetNodeAs<RoomNode>(Direction.West).GetNodeAs<RoomNode>(Direction.South), rad5));
                    }
                    if (east && NE)
                    {
                        if (GetNodeAs<RoomNode>(Direction.East)?.GetNodeAs<RoomNode>(Direction.North)?.GetNodeAs<RoomNode>(Direction.East)?.Traversible ?? false)
                            _nextNodes.Add((GetNodeAs<RoomNode>(Direction.East).GetNodeAs<RoomNode>(Direction.North).GetNodeAs<RoomNode>(Direction.East), rad5));
                    }
                    if (east && SE)
                    {
                        if (GetNodeAs<RoomNode>(Direction.East)?.GetNodeAs<RoomNode>(Direction.South)?.GetNodeAs<RoomNode>(Direction.East)?.Traversible ?? false)
                            _nextNodes.Add((GetNodeAs<RoomNode>(Direction.East).GetNodeAs<RoomNode>(Direction.South).GetNodeAs<RoomNode>(Direction.East), rad5));
                    }
                    if (west && NW)
                    {
                        if (GetNodeAs<RoomNode>(Direction.West)?.GetNodeAs<RoomNode>(Direction.North)?.GetNodeAs<RoomNode>(Direction.West)?.Traversible ?? false)
                            _nextNodes.Add((GetNodeAs<RoomNode>(Direction.West).GetNodeAs<RoomNode>(Direction.North).GetNodeAs<RoomNode>(Direction.West), rad5));
                    }
                    if (west && SW)
                    {
                        if (GetNodeAs<RoomNode>(Direction.West)?.GetNodeAs<RoomNode>(Direction.South)?.GetNodeAs<RoomNode>(Direction.West)?.Traversible ?? false)
                            _nextNodes.Add((GetNodeAs<RoomNode>(Direction.West).GetNodeAs<RoomNode>(Direction.South).GetNodeAs<RoomNode>(Direction.West), rad5));
                    }
                }
            }
            _nextNodesKnown = true;
            return _nextNodes;
        }
    }

    public INode GetNode(Direction direction)
    {
        switch(direction)
        {
            case Direction.North:
                return _north;
            case Direction.South:
                return _south;
            case Direction.East:
                return _east;
            case Direction.West:
                return _west;
            default:
                return null;
        }
    }

    public void SetNode(Direction direction, INode node, bool recursive = true)
    {
        if (this == Undefined || this == Invalid)
            return;

        bool checkContiguous = false;
        RoomNode node2 = null;
        if (node is Wall && TryGetNodeAs(direction, out node2, false) && recursive)
        {
            checkContiguous = true;
            node2.SetNode(~direction, node, false);
        }
        else if (node is Door door && TryGetNodeAs(direction, out Wall wall))
        {
            door.Wall = wall;
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

        UpdateNearbyNextNodes();
    }

    public virtual T GetNodeAs<T>(Direction direction, bool traversible = true) where T : INode
    {
        INode node = GetNode(direction);

        if(node is T)
        {
            if (typeof(T) != typeof(RoomNode) || !traversible)
                return (T)node;
            else
            {
                if(node is Stair nodeStair && nodeStair.Direction == ~direction)
                {
                    if (nodeStair.WorldPosition.z == WorldPosition.z - 1)
                        return (T)node;
                    else
                        return default(T);
                }
                RoomNode roomNode = (RoomNode)node;
                if (roomNode != Undefined && roomNode.WorldPosition.z == WorldPosition.z)
                    return (T)node;
            }
        }
        else if(node is Door && typeof(T) == typeof(Wall))
        {
            return (T)(INode)(node as Door).Wall;
        }
        return default(T);
    }

    public bool TryGetNodeAs<T>(Direction direction, out T node, bool traversible = true) where T : INode
    {
        node = GetNodeAs<T>(direction, traversible);
        return node != null;
    }

    public bool TryGetNodeAs<T>(Direction direction, bool traversible = true) where T : INode
    {
        return GetNodeAs<T>(direction, traversible) != null;
    }

    List<INode> _adjacentNodes = new List<INode>();

    public List<INode> AdjacentNodes
    {
        get
        {
            if(_adjacentNodes.Count == 0)
            {
                _adjacentNodes.Add(_north);
                _adjacentNodes.Add(_south);
                _adjacentNodes.Add(_east);
                _adjacentNodes.Add(_west);
            }
            return _adjacentNodes;
        }
    }

    /// <summary>
    /// Notifies all nearby nodes that they will need to update their NearbyNodes lists (including itself)
    /// </summary>
    void UpdateNearbyNextNodes()
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
                    Map.Instance[WorldPosition + new Vector3Int(i, j)]._traversible = null;
                Map.Instance[WorldPosition + new Vector3Int(i, j)]?._adjacentNodes.Clear();
            }
        }
    }
}