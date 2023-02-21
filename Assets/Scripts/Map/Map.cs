using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Undirected = -100,
    North = 0,
    South = ~North,
    East = 1,
    West = ~East,
    NorthEast = 2,
    NorthWest = 3,
    SouthEast = ~NorthWest,
    SouthWest = ~NorthEast
}

public enum MapAlignment
{
    XEdge = 0,
    YEdge = 1,
    Center = 2,
    Corner = 3
}

public class Map : MonoBehaviour, IDataPersistence
{
    static Map _instance;
    Layer[] _layers;
    List<Room> _rooms;
    List<Sector> _sectors = new();



    /// <value>Accessor for the <see cref="Map"/> singleton instance.</value>
    public static Map Instance => _instance;
    public static bool Ready { get; private set; } = false;

    public IEnumerable<RoomNode> AllNodes
    {
        get
        {
            foreach (Layer layer in _layers)
            {
                if (layer == null)
                    yield break;

                foreach (RoomNode node in layer.Nodes)
                    yield return node;
            }
        }
    }

    public int MapLength { get; } = 40;
    public int MapWidth { get; } = 40;
    public RoomNode this[int x, int y, int z]
    {
        get
        {
            for(int i = 0; i < _layers.Length; i++)
            {
                if (_layers[i] == null)
                    return RoomNode.Undefined;
                if (z < _layers[i].Height)
                    return _layers[i][x, y];

                z -= _layers[i].Height;
            }
            return RoomNode.Undefined;
        }
    }

    public RoomNode this[int x, int y, int z, int relZ]
    {
        get
        {
            for (int i = 0; i < _layers.Length; i++)
            {
                if (_layers[i] == null)
                    return RoomNode.Undefined;
                if (z < _layers[i].Height)
                {
                    if (i + relZ < 0 || i + relZ >= _layers.Length || _layers[i + relZ] == null)
                        return RoomNode.Undefined;
                    return _layers[i + relZ][x, y];
                }
                z -= _layers[i].Height;
            }
            return RoomNode.Undefined;
        }
    }

    public RoomNode this[Vector3Int position] => Instance[position.x,position.y, position.z];
    public RoomNode this[Vector3Int position, int relZ] => Instance[position.x, position.y, position.z, relZ];

    public Layer this[int z]
    {
        get
        {
            for (int i = 0; i < _layers.Length; i++)
            {
                if (_layers[i] == null)
                    return null;
                if (z < _layers[i].Height)
                    return _layers[i];

                z -= _layers[i].Height;
            }
            return null;
        }
        set
        {
            for (int i = 0; i < _layers.Length; i++)
            {
                if (_layers[i] == null || z < _layers[i].Height)
                {
                    _layers[i] = value;
                    return;
                }

                z -= _layers[i].Height;
            }
        }
    }

    public Layer this[int z, int relZ]
    {
        get
        {
            for (int i = 0; i < _layers.Length; i++)
            {
                if (_layers[i] == null)
                    return null;
                if (z < _layers[i].Height)
                {
                    if (i + relZ < 0 || i + relZ >= _layers.Length || _layers[i + relZ] == null)
                        return null;

                    return _layers[i + relZ];
                }
                z -= _layers[i].Height;
            }
            return null;
        }
        set
        {
            for (int i = 0; i < _layers.Length; i++)
            {
                if (z < _layers[i].Height)
                {
                    if (i + relZ < 0 || i + relZ >= _layers.Length)
                        throw new System.ArgumentException();
                    else
                        _layers[i + relZ] = value;
                    return;
                }

                z -= _layers[i].Height;
            }
        }
    }

    public static Vector3Int DirectionToVector(Direction direction)
    {
        return direction switch
        {
            Direction.North => Vector3Int.up,
            Direction.South => Vector3Int.down,
            Direction.East => Vector3Int.right,
            Direction.West => Vector3Int.left,
            Direction.NorthEast => new Vector3Int(1, 1),
            Direction.SouthEast => new Vector3Int(1, -1),
            Direction.NorthWest => new Vector3Int(-1, 1),
            Direction.SouthWest => new Vector3Int(-1, -1),
            _ => default,
        };
    }

    static readonly float RAD2_2 = Mathf.Sqrt(2) / 2;

    public static Vector3 DirectionToVectorNormalized(Direction direction)
    {
        return direction switch
        {
            Direction.North => Vector3.up,
            Direction.South => Vector3.down,
            Direction.East => Vector3.right,
            Direction.West => Vector3.left,
            Direction.NorthEast => new Vector3(RAD2_2, RAD2_2),
            Direction.SouthEast => new Vector3(RAD2_2, -RAD2_2),
            Direction.NorthWest => new Vector3(-RAD2_2, RAD2_2),
            Direction.SouthWest => new Vector3(-RAD2_2, -RAD2_2),
            _ => default,
        };
    }

    public static MapAlignment DirectionToEdgeAlignment(Direction direction)
    {
        return (direction == Direction.North || direction == Direction.South) ? MapAlignment.XEdge : MapAlignment.YEdge;
    }

    public static Vector3Int Floor(Vector3Int position)
    {
        Layer layer = Instance[position.z];
        position.z = layer.Origin.z;
        return position;
    }

    public static RoomNode GetNodeFromSceneCoordinates(Vector3 position, float level)
    {
        float x = (position.x - 154 + 2 * (position.y - 2 * level)) / 4f;
        float y = position.y - 2 - x - 2 * level;

        return Instance[Mathf.RoundToInt(x) > 0 ? Mathf.RoundToInt(x) : 0, Mathf.RoundToInt(y) > 0 ? Mathf.RoundToInt(y) : 0, (int)level];

    }

    public static Vector3 MapCoordinatesToSceneCoordinates(Vector3 position, MapAlignment alignment = MapAlignment.Center)
    {
        return MapCoordinatesToSceneCoordinates(position.x, position.y, position.z, alignment);
    }

    public static Vector3 MapCoordinatesToSceneCoordinates(float mapX, float mapY, float mapZ, MapAlignment alignment = MapAlignment.Center)
    {
        float x = 150 + 2 * mapX - 2 * mapY;
        float y = 2 + mapX + mapY;
        switch (alignment)
        {
            case MapAlignment.XEdge:

                x++;
                break;
            case MapAlignment.YEdge:
                x--;
                break;
            case MapAlignment.Corner:
                y--;
                break;
        }
        return new Vector3(x, y + 2 * mapZ);
    }

    public static Vector3Int SceneCoordinatesToMapCoordinates(Vector3 position, int level)
    {
        float x = (position.x - 154 + 2 * (position.y - 2 * level)) / 4f;
        float y = position.y - 2 - x - 2 * level;

        return new Vector3Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y), level);
    }

    public static Direction VectorToDirection(Vector3 vector, bool cardinal = false)
    { 
        Vector2 gameVector = new(vector.x, vector.y);

        if (gameVector == Vector2.zero)
            return Direction.Undirected;

        int best = 7;
        float best_product = Vector2.Dot(gameVector, new Vector2(0, 1));

        for (int i = 0; i < 7; i++)
        {
            if (cardinal && i % 2 == 0)
                continue;
            var value = i switch
            {
                1 => Vector2.Dot(gameVector, new Vector2(1, 0)),
                2 => Vector2.Dot(gameVector, new Vector2(RAD2_2, -RAD2_2)),
                3 => Vector2.Dot(gameVector, new Vector2(0, -1)),
                4 => Vector2.Dot(gameVector, new Vector2(-RAD2_2, -RAD2_2)),
                5 => Vector2.Dot(gameVector, new Vector2(-1, 0)),
                6 => Vector2.Dot(gameVector, new Vector2(-RAD2_2, RAD2_2)),
                _ => Vector2.Dot(gameVector, new Vector2(RAD2_2, RAD2_2)),
            };
            if (value > best_product)
            {
                best_product = value;
                best = i;
            }
        }

        return best switch
        {
            0 => Direction.NorthEast,
            1 => Direction.East,
            2 => Direction.SouthEast,
            3 => Direction.South,
            4 => Direction.SouthWest,
            5 => Direction.West,
            6 => Direction.NorthWest,
            7 => Direction.North,
            _ => Direction.Undirected,
        };
    }
    public void AddRooms(Room room)
    {
        _rooms.Add(room);
        int roomZ = room.Origin.z;
        Layer roomLayer = Instance[roomZ];
        int z = roomLayer.Origin.z + roomLayer.Height;

        if (Instance[roomZ, 1] == null)
            Instance[roomZ, 1] = new Layer(MapWidth, MapLength, new Vector3Int(0, 0, z), roomLayer.LayerID + 1);

        foreach (RoomNode node in room.Nodes)
        {
            Vector3Int position = node.WorldPosition;
            if (Instance[position.x, position.y, z] == null || Instance[position.x, position.y, z] == RoomNode.Undefined)
            {
                Instance[z].InstantiateRoomNode(position.x, position.y);
            }
            if (Instance[position.x - 1, position.y, z] == null || Instance[position.x - 1, position.y, z] == RoomNode.Undefined)
            {
                Instance[z].InstantiateRoomNode(position.x - 1, position.y);
            }
            if (Instance[position.x + 1, position.y, z] == null || Instance[position.x + 1, position.y, z] == RoomNode.Undefined)
            {
                Instance[z].InstantiateRoomNode(position.x + 1, position.y);
            }
            if (Instance[position.x, position.y - 1, z] == null || Instance[position.x, position.y - 1, z] == RoomNode.Undefined)
            {
                Instance[z].InstantiateRoomNode(position.x, position.y - 1);
            }
            if (Instance[position.x, position.y + 1, z] == null || Instance[position.x, position.y + 1, z] == RoomNode.Undefined)
            {
                Instance[z].InstantiateRoomNode(position.x, position.y + 1);
            }
            if (Instance[position.x - 1, position.y - 1, z] == null || Instance[position.x - 1, position.y - 1, z] == RoomNode.Undefined)
            {
                Instance[z].InstantiateRoomNode(position.x - 1, position.y - 1);
            }
            if (Instance[position.x + 1, position.y + 1, z] == null || Instance[position.x + 1, position.y + 1, z] == RoomNode.Undefined)
            {
                Instance[z].InstantiateRoomNode(position.x + 1, position.y + 1);
            }
            if (Instance[position.x + 1, position.y - 1, z] == null || Instance[position.x + 1, position.y - 1, z] == RoomNode.Undefined)
            {
                Instance[z].InstantiateRoomNode(position.x + 1, position.y - 1);
            }
            if (Instance[position.x - 1, position.y + 1, z] == null || Instance[position.x - 1, position.y + 1, z] == RoomNode.Undefined)
            {
                Instance[z].InstantiateRoomNode(position.x - 1, position.y + 1);
            }
        }
    }

    void BuildSectors()
    {
        Sector.DivideIntoSectors(Instance, ref _sectors);
        foreach(Sector sector in _sectors)
        {
            foreach(INode node in sector.BottleNecks)
            {
                if(node is RoomNode roomNode)
                {
                    roomNode.Reserved = true;
                }
            }
        }
    }

    public float ApproximateDistance(Vector3Int startPosition, Vector3Int endPosition)
    {
        RoomNode end = Instance[endPosition];

        if (!Sector.SameSector(Instance[startPosition], end))
            return float.PositiveInfinity;

        Room startingRoom = Instance[startPosition].Room;
        Room endingRoom = end.Room;

        Dictionary<INode, float> g_score = new();

        PriorityQueue<INode, float> nodeQueue = new(false);

        if (startingRoom == endingRoom)
        {
            float score = Vector3Int.Distance(startPosition, endPosition);
            g_score[end] = score;

            nodeQueue.Push(end, score);
        }

        foreach (ConnectingNode node in startingRoom.Connections)
        {
            float score = Vector3Int.Distance(startPosition, node.WorldPosition);
            g_score[node] = score;
            nodeQueue.Push(node, score + Vector3Int.Distance(node.WorldPosition, endPosition));
        }

        while (!nodeQueue.Empty && nodeQueue.Count < 50)
        {
            INode currentNode = nodeQueue.Pop();
            if (currentNode == end)
            {
                return g_score[end];
            }

            if (!currentNode.Traversable)
                continue;
            ConnectingNode current = currentNode as ConnectingNode;

            float currentScore = g_score[current];

            if (current.AdjacentToRoom(endingRoom))
            {
                float nextScore = currentScore + Vector3Int.Distance(current.WorldPosition, endPosition);
                if (g_score.TryGetValue(end, out var score) && score < nextScore)
                {
                    continue;
                }

                g_score[end] = nextScore;
                nodeQueue.Push(end, nextScore, true);
            }

            List<ConnectingNode> nextNodes = current.ConnectionNodes;

            foreach (ConnectingNode next in nextNodes)
            {
                float nextScore = current.GetDistance(next) + currentScore;
                if (g_score.TryGetValue(next, out var score) && score < nextScore)
                {
                    continue;
                }
                g_score[next] = nextScore;
                nodeQueue.Push(next, nextScore + Vector3.Distance(endPosition, next.WorldPosition), true);
            }
        }

        return float.PositiveInfinity;
    }

    public bool CanPlaceDoor(Vector3Int position, MapAlignment alignment)
    {
        int x = position.x;
        int y = position.y;
        int z = position.z;
        if (WithinConstraints(x, y, z, alignment))
        {
            if (alignment == MapAlignment.XEdge)
            {
                if (Instance[x, y - 1, z] == null || Instance[x, y, z] == null)
                    return false;
            }
            else
            {
                if (Instance[x - 1, y, z] == null || Instance[x, y, z] == null)
                    return false;
            }


            for (int i = -1; i <= 1; i++)
            {
                //Checks that the wall location must not be a door or null.
                if (alignment == MapAlignment.XEdge ?
                    (GetWall(alignment, x + i, y, z)?.WallSprite.IsDoor ?? true) :
                    (GetWall(alignment, x, y + i, z)?.WallSprite.IsDoor ?? true))
                    return false;
            }

            for (int i = 0; i <= 1; i++)
            {
                int xVal = x + (alignment == MapAlignment.XEdge ? i : 0);
                int yVal = y + (alignment == MapAlignment.YEdge ? i : 0);
                if (IsCorner(xVal, yVal, z))
                    return false;
            }

            return true;
        }
        return false;
    }

    public bool CanPlaceObject(Vector3Int position, Vector3Int dimensions)
    {
        if (!WithinConstraints(position.x, position.y, position.z, MapAlignment.Center))
            return false;

        for (int i = 0; i < dimensions.x; i++)
        {
            for (int j = 0; j < dimensions.y; j++)
            {
                RoomNode roomNode = Instance[position + Vector3Int.right * i + Vector3Int.up * j];
                if (!roomNode.Empty || !roomNode.Floor.Enabled || roomNode == RoomNode.Undefined || roomNode is StairNode)
                    return false;
                if (j != 0 && i != 0)
                    if (GetWall(MapAlignment.XEdge, position + Vector3Int.right * i + Vector3Int.up * j) != null || GetWall(MapAlignment.YEdge, position + Vector3Int.right * i + Vector3Int.up * j) != null)
                        return false;
            }
        }
        return true;
    }

    public bool CanPlaceWall(int x, int y, int z, MapAlignment alignment)
    {
        if (WithinConstraints(x, y, z, alignment) && Instance.IsSupported(x, y, z, alignment))
        {
            if (GetWall(alignment, x, y, z) == null)
            {
                for (int i = -2; i <= 3; i++)
                {
                    if (alignment == MapAlignment.XEdge ?
                        (GetWall(MapAlignment.YEdge, x + i, y, z)?.WallSprite.IsDoor ?? false) &&
                        (GetWall(MapAlignment.YEdge, x + i, y - 1, z)?.WallSprite.IsDoor ?? false)
                        :
                        (GetWall(MapAlignment.XEdge, x, y + i, z)?.WallSprite.IsDoor ?? false) &&
                        (GetWall(MapAlignment.XEdge, x - 1, y + i, z)?.WallSprite.IsDoor ?? false))
                        return false;
                }
                for (int i = -2; i <= 2; i++)
                {
                    if (alignment == MapAlignment.XEdge ?
                        (GetWall(alignment, x, y + i, z) != null) :
                        (GetWall(alignment, x + i, y, z) != null))
                        return false;
                }
                return true;
            }
        }
        return false;

    }

    public bool CanPlaceWall(Vector3Int position, MapAlignment alignment)
    {
        return CanPlaceWall(position.x, position.y, position.z, alignment);
    }

    public ConnectingNode GetConnectionNode(Vector3Int position)
    {
        if (Instance[position].TryGetNodeAs(Direction.South, out ConnectingNode southNode))
            return southNode;
        if (Instance[position].TryGetNodeAs(Direction.West, out ConnectingNode westNode))
            return westNode;
        return null;
    }

    public Graphics.Corner GetCorner(int x, int y, int z)
    {
        if (WithinConstraints(x, y, z, MapAlignment.Corner) && Instance[x, y, z] != null)
            return Instance[x, y, z].Corner;
        else
            return null;
    }

    public Graphics.Corner GetCorner(Vector3Int position)
    {
        return GetCorner(position.x, position.y, position.z);
    }

    public WallBlocker GetWall(MapAlignment alignment, Vector3Int position)
    {
        return GetWall(alignment, position.x, position.y, position.z);
    }

    public WallBlocker GetWall(MapAlignment alignment, int x, int y, int z)
    {
        if (!WithinConstraints(x, y, z, alignment))
            return null;

        if (alignment == MapAlignment.XEdge)
        {
            return Instance[x, y, z].GetNodeAs<WallBlocker>(Direction.South);
        }
        else
        {
            return Instance[x, y, z].GetNodeAs<WallBlocker>(Direction.West);
        }
    }

    /// <summary>
    /// Evaluates if a certain position is supported by the layer below it, and thus building features can be placed there.
    /// </summary>
    /// <param name="position">World position being checked.</param>
    /// <param name="alignment">The alignment of the position being checked.</param>
    /// <returns>Returns true if the position is supported by the layer beneath it.</returns>
    public bool IsSupported(Vector3Int position, MapAlignment alignment)
    {
        return IsSupported(position.x, position.y, position.z, alignment);
    }

    public bool IsSupported(RoomNode node)
    {
        return !(node == null || node.Room is Layer);
    }

    public bool IsSupported(int x, int y, int z, MapAlignment alignment)
    {
        if (z <= 0)
            return true;
        RoomNode beneath = Instance[x, y, z - 1];
        if (beneath == null || beneath.Room is Layer)
        {
            switch (alignment)
            {
                case MapAlignment.XEdge:
                    beneath = Instance[x, y - 1, z - 1];
                    if (beneath == null || beneath.Room is Layer)
                    {
                        return false;
                    }
                    break;

                case MapAlignment.YEdge:
                    beneath = Instance[x - 1, y, z - 1];
                    if (beneath == null || beneath.Room is Layer)
                    {
                        return false;
                    }
                    break;
                case MapAlignment.Center:
                    return false;
            }
        }
        return true;
    }

    public void LoadData(GameData gameData)
    {
        int mapWidth = gameData.MapWidth;
        int mapLength = gameData.MapLength;

        _layers = new Layer[Mathf.Max(10, gameData.Layers)];
        RoomNode[,] nodes = new RoomNode[MapWidth, MapLength];
        _layers[0] = new Layer(nodes, Vector3Int.zero, 0);

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapLength; j++)
            {
                RoomNode next = new(Instance[0], i, j);
                nodes[i, j] = next;
                if (i > 0)
                {
                    next.SetNode(Direction.West, nodes[i - 1, j]);
                }
                if (j > 0)
                {
                    next.SetNode(Direction.South, nodes[i, j - 1]);
                }
            }
        }

        for (int i = 0; i < gameData.Layers; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                for (int k = 0; k < mapLength; k++)
                {
                    if (!gameData.Map[i * mapWidth * mapLength + j * mapLength + k].IsUndefined)
                        gameData.Map[i * mapWidth * mapLength + j * mapLength + k].SetNode(Instance[j, k, 0, i]);
                }
            }
        }

        foreach (SerializableDoor door in gameData.Doors)
        {
            PlaceDoor(door.Position, door.Alignment);
            Graphics.Instance.PlaceDoor(door.Position, door.Alignment, AccentMaterial.Stone);
        }

        foreach (SerializableStair stair in gameData.Stairs)
        {
            new StairNode(stair.Position, stair.Direction);
        }

        RoomNode.Invalid.Floor.Enabled = false;
        RoomNode.Undefined.Floor.Enabled = false;

        Graphics.Instance.UpdateGraphics();
        Graphics.Instance.SetLevel();
        Graphics.Instance.Confirm();
        GameManager.MapChangingSecond += BuildSectors;

        Ready = true;
    }

    /// <summary>
    /// Finds the shortest path for an <see cref="AdventurerPawn"/> to take to travel from one <see cref="RoomNode"/> to another.
    /// Assumes that the <see cref="RoomNode"/>s are in differnt <see cref="Room"/>s.
    /// </summary>
    /// <param name="start">The <see cref="RoomNode"/> the <see cref="AdventurerPawn"/> is starting in</param>
    /// <param name="end">The <see cref="RoomNode"/> the <see cref="AdventurerPawn"/> wishes to end in.</param>
    /// <returns>Returns a <see cref="IEnumerable"/> of <see cref="ConnectingNode"/>s designating the path for the <see cref="AdventurerPawn"/> to take, or null if no path exists.</returns>
    public IEnumerator NavigateBetweenRooms(IWorldPosition start, IWorldPosition end)
    {

        if (!Sector.SameSector(start, end))
            yield return float.PositiveInfinity;

        Room startingRoom = start.Room;
        Room endingRoom = end.Room;

        Dictionary<IWorldPosition, float> g_score = new();
        Dictionary<IWorldPosition, IWorldPosition> immediatePredecessor = new();
        Dictionary<(IWorldPosition, IWorldPosition), IEnumerator> paths = new();

        PriorityQueue<(IWorldPosition, IWorldPosition), float> nodeQueue = new(false);

        Vector3Int endPosition = end.WorldPosition;

        g_score.Add(start, 0);

        if (startingRoom == endingRoom)
        {
            nodeQueue.Push((start, end), Vector3Int.Distance(start.WorldPosition, endPosition));
        }

        foreach (ConnectingNode node in startingRoom.Connections)
        {
            nodeQueue.Push((start, node), Vector3Int.Distance(start.WorldPosition, node.WorldPosition) + Vector3Int.Distance(node.WorldPosition, endPosition));
        }

        while (!nodeQueue.Empty && nodeQueue.Count < 50)
        {
            (IWorldPosition prevNode, IWorldPosition current) = nodeQueue.Pop();
            if (current == end)
            {
                if (immediatePredecessor.TryGetValue(end, out IWorldPosition preceding) && preceding == prevNode)
                {
                    yield return g_score[end];
                    if(end is RoomNode)
                        yield return end;
                    IEnumerator path = paths[(preceding, end)];
                    while (path.MoveNext())
                        yield return path.Current;

                    if (preceding != start)
                    {
                        IWorldPosition receding = preceding;
                        yield return receding;
                        preceding = immediatePredecessor[receding];
                        while (preceding != start)
                        {
                            foreach (RoomNode node in (preceding as ConnectingNode).GetPath(receding as ConnectingNode))
                                yield return node;

                            receding = preceding;
                            yield return receding;
                            preceding = immediatePredecessor[receding];
                        }
                        path = paths[(preceding, receding)];

                        while (path.MoveNext())
                            yield return path.Current;
                    }
                    yield break;
                }
                else
                {

                    if(GetPath(prevNode, end, endingRoom, out float score))
                        nodeQueue.Push((prevNode, end), score);

                    continue;
                }
            }

            if (current is not ConnectingNode currentNode || currentNode.Obstructed)
                continue;

            float currentScore;

            if (prevNode == start)
            {
                GetPath(start, currentNode, startingRoom, out currentScore);
            }
            else
            {
                currentScore = g_score[currentNode];
            }

            if (currentNode.AdjacentToRoom(endingRoom))
            {
                nodeQueue.Push((currentNode, end), currentScore + Vector3Int.Distance(currentNode.WorldPosition, endPosition));
            }

            List<ConnectingNode> nextNodes = currentNode.ConnectionNodes;

            foreach (ConnectingNode next in nextNodes)
            {
                float nextScore = currentNode.GetDistance(next) + currentScore;
                if (g_score.TryGetValue(next, out var score))
                {
                    if (score < nextScore)
                        continue;
                    else
                        nodeQueue.Push((currentNode, next), nextScore + Vector3.Distance(endPosition, next.WorldPosition), true);
                }
                else
                    nodeQueue.Push((currentNode, next), nextScore + Vector3.Distance(endPosition, next.WorldPosition));

                g_score[next] = nextScore;
                immediatePredecessor[next] = currentNode;
            }

        }

        yield return float.PositiveInfinity;


        bool GetPath(IWorldPosition start, IWorldPosition end, Room room, out float score)
        {
            IEnumerator pathIter = room.Navigate(start, end);
            pathIter.MoveNext();
            if (!g_score.TryGetValue(end, out score) || score > (float)pathIter.Current + g_score[start])
            {
                paths[(start, end)] = pathIter;
                score = (float)pathIter.Current + g_score[start];
                g_score[end] = score;
                immediatePredecessor[end] = start;
                return true;
            }

            return false;
        }
    }

    public Layer NextLayer(int z, int relZ)
    {
        for (int i = 0; i < _layers.Length; i++)
        {
            if (_layers[i] == null)
                return _layers[i - 1];
            if (z < _layers[i].Height)
            {
                if (i + relZ < 0)
                    return null;
                else if (i + relZ >= _layers.Length || _layers[i + relZ] == null)
                    return _layers[i];

                return _layers[i + relZ];
            }
            z -= _layers[i].Height;
        }

        throw new System.ArgumentException("Unreachable z parameter.");
    }

    public void PlaceDoor(Vector3Int position, MapAlignment alignment)
    {
        RoomNode node1 = null, node2 = Instance[position.x, position.y, position.z];
        switch (alignment)
        {
            case MapAlignment.XEdge:
                node1 = Instance[position.x, position.y - 1, position.z];
                break;
            case MapAlignment.YEdge:
                node1 = Instance[position.x - 1, position.y, position.z];
                break;
        }
        new DoorConnector(node1, node2, position);
    }

    public void RemoveDoor(Vector3Int position, MapAlignment alignment)
    {
        int x = position.x;
        int y = position.y;
        int z = position.z;

        ConnectingNode door;
        if (alignment == MapAlignment.XEdge)
        {
            if (!Instance[x, y, z].TryGetNodeAs(Direction.South, out door))
                if (!Instance[x - 1, y, z].TryGetNodeAs(Direction.South, out door))
                    if (!Instance[x + 1, y, z].TryGetNodeAs(Direction.South, out door))
                        return;
        }
        else
        {
            if (!Instance[x, y, z].TryGetNodeAs(Direction.West, out door))
                if (!Instance[x, y - 1, z].TryGetNodeAs(Direction.West, out door))
                    if (!Instance[x, y + 1, z].TryGetNodeAs(Direction.West, out door))
                        return;
        }

        door.Disconnect();

    }

    public void RemoveWall(Vector3Int position, MapAlignment alignment)
    {
        int x = position.x;
        int y = position.y;
        int z = position.z;
        RoomNode b, a = Instance[x, y, z];

        if (alignment == MapAlignment.XEdge)
        {
            b = Instance[x, y - 1, z];
            b.SetNode(Direction.North, a);
        }
        else
        {
            b = Instance[x - 1, y, z];
            b.SetNode(Direction.East, a);
        }

        Room roomA = a.Room;
        Room roomB = b.Room;
        if (roomA != roomB)
        {
            if (roomA.Length * roomA.Width < roomB.Length * roomB.Width)
            {
                (roomB, roomA) = (roomA, roomB);
            }

            roomA.EnvelopRoom(roomB);

            _rooms.Remove(roomB);
        }
    }

    public void SaveData(GameData gameData)
    {
        gameData.Doors = new List<SerializableDoor>();
        gameData.Stairs = new List<SerializableStair>();
        gameData.SpriteObjects = new List<SpriteObject>();
        int layerNumber = 10;
        for (int i = 0; i < _layers.Length; i++)
        {
            if (_layers[i] == null)
            {
                layerNumber = i;
                break;
            }
        }

        int arrayLength = layerNumber * MapWidth * MapLength;
        SerializableNode[] mapData = new SerializableNode[layerNumber * MapWidth * MapLength];
        for (int i = 0; i < layerNumber; i++)
        {
            for (int j = 0; j < MapWidth; j++)
            {
                for (int k = 0; k < MapLength; k++)
                {
                    if (Instance[j, k, 0, i] != RoomNode.Invalid)
                    {
                        bool checkSouth = false;
                        bool checkWest = false;
                        mapData[i * MapLength * MapWidth + j * MapLength + k] = new SerializableNode(Instance[j, k, 0, i], ref checkSouth, ref checkWest);

                        if (Instance[j, k, 0, i] is StairNode stair)
                        {
                            gameData.Stairs.Add(new SerializableStair(stair));
                        }

                        if (checkSouth)
                        {
                            if (Instance[j, k, 0, i].TryGetNodeAs(Direction.South, out DoorConnector door))
                            {
                                gameData.Doors.Add(new SerializableDoor(door));
                            }
                        }
                        if (checkWest)
                        {
                            if (Instance[j, k, 0, i].TryGetNodeAs(Direction.West, out DoorConnector door))
                            {
                                gameData.Doors.Add(new SerializableDoor(door));
                            }
                        }
                    }
                }
            }
        }

        System.Array.Resize(ref mapData, arrayLength);

        gameData.Map = mapData;
        gameData.MapWidth = MapWidth;
        gameData.MapLength = MapLength;
        gameData.MapHeight = _layers[0].Height;
        gameData.Layers = layerNumber;
    }

    public void SetWall(MapAlignment alignment, Vector3Int position, WallBlocker wall)
    {
        SetWall(alignment, position.x, position.y, position.z, wall);
    }

    public void SetWall(MapAlignment alignment, int x, int y, int z, WallBlocker wall)
    {
        if (alignment == MapAlignment.XEdge)
        {
            Instance[x, y, z].SetNode(Direction.South, wall);
        }
        else
        {
            Instance[x, y, z].SetNode(Direction.West,wall);
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            _rooms = new List<Room>();
        }
        else
            Destroy(this);
    }

    bool IsCorner(int x, int y, int z)
    {
        return Graphics.Corner.GetSpriteIndex(new Vector3Int(x, y, z)) != -1;
    }

    bool WithinConstraints(int x, int y, int _, MapAlignment alignment)
    {
        if (x > 0 && y > 0)

            return alignment switch
            {
                MapAlignment.XEdge => x < MapWidth && y < MapLength - 1,
                MapAlignment.YEdge => x < MapWidth - 1 && y < MapLength,
                _ => x < MapWidth - 1 && y < MapLength - 1,
            };
        else
        {
            return false;
        }
    }
}
