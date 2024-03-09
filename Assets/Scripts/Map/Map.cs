using System;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.Data.Serializable;
using Assets.Scripts.Map.Node;
using Assets.Scripts.Map.Sprite_Object;
using Assets.Scripts.Utility;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.Map
{
    /// <summary>
    /// Enum of the cardinal and ordinal directions. <see cref="Direction"/>s that are opposite are bitwise complements.
    /// </summary>
    public enum Direction
    {
        /// <summary>Points in no particular direction. Used when the direction is ambiguous or trivial.</summary>
        Undirected = -100,
        /// <summary>Points north. Because the game is isometric from the user perspective this points to the upper left.</summary>
        North = 0,
        /// <summary>Points south. Because the game is isometric from the user perspective this points to the lower right.</summary>
        South = ~North,
        /// <summary>Points east. Because the game is isometric from the user perspective this points to the upper right.</summary>
        East = 1,
        /// <summary>Points south. Because the game is isometric from the user perspective this points to the lower left.</summary>
        West = ~East,
        /// <summary>Points northeast. Because the game is isometric from the user perspective this points directly up.</summary>
        NorthEast = 2,
        /// <summary>Points northwest. Because the game is isometric from the user perspective this points to the left.</summary>
        NorthWest = 3,
        /// <summary>Points southeast. Because the game is isometric from the user perspective this points to the right.</summary>
        SouthEast = ~NorthWest,
        /// <summary>Points southwest. Because the game is isometric from the user perspective this points directly down.</summary>
        SouthWest = ~NorthEast
    }

    /// <summary>
    /// Enum referring to the position of an <see cref="IWorldPosition"/> relative to it's tile.
    /// </summary>
    public enum MapAlignment
    {
        /// <summary>Runs along the x-axis below the tile.</summary>
        XEdge = 0,
        /// <summary>Runs along the y-axis to the left of the tile.</summary>
        YEdge = 1,
        /// <summary>Is positioned in the very center of the tile.</summary>
        Center = 2,
        /// <summary>Is positioned in the lower corner of the tile.</summary>
        Corner = 3
    }

    /// <summary>
    /// Enum referring to targets on the map for AI actions.
    /// </summary>
    public enum PointOfInterest
    {
        /// <summary>Indicated locations where an actor can get food.</summary>
        Food,
        /// <summary>Indicates locations where an actor can sit. </summary>
        Sit,
        /// <summary>Indicates locations where an actor can lay down. </summary>
        Lay
    }

    /// <summary>
    /// The <see cref="Map"/> class contains and controls all data regarding the game's world and map.
    /// </summary>
    public class Map : MonoBehaviour, IDataPersistence
    {
        
        private Layer[] _layers;
        private List<Sector> _sectors = new();

        /// <value>Accessor for the <see cref="Map"/> singleton instance.</value>
        public static Map Instance { get; private set; }

        /// <value>True if the <see cref="Map"/> has completed it's initial setup.</value>
        public static bool Ready { get; private set; }

        /// <value>Iterates over the list of all <see cref="RoomNode"/>s on the <see cref="Map"/>.</value>
        public IEnumerable<RoomNode> AllNodes
        {
            get
            {
                foreach (Layer layer in _layers)
                {
                    if (layer == null)
                        yield break;

                    foreach (RoomNode node in layer.GetNodes)
                        yield return node;
                }
            }
        }

        /// <value>The size of the <see cref="Map"/> along it's y-axis.</value>
        public int MapLength { get; } = 40;

        /// <value>The size of the <see cref="Map"/> along it's x-axis.</value>
        public int MapWidth { get; } = 40;

        /// <summary>
        /// Indexer for <see cref="RoomNode"/>s on the <see cref="Map"/>. 
        /// </summary>
        /// <param name="x">The x-position of the <see cref="RoomNode"/>.</param>
        /// <param name="y">The y-position of the <see cref="RoomNode"/>.</param>
        /// <param name="z">The z-position of the <see cref="RoomNode"/>.</param>
        /// <returns>Returns the <see cref="RoomNode"/> at the given <see cref="Map"/> coordinates.</returns>
        public RoomNode this[int x, int y, int z]
        {
            get
            {
                foreach (Layer layer in _layers)
                {
                    if (layer == null)
                        return RoomNode.Undefined;
                    if (z < layer.Height)
                        return layer[x, y];

                    z -= layer.Height;
                }

                return RoomNode.Undefined;
            }
        }

        /// <summary>
        /// Indexer for <see cref="RoomNode"/>s on the <see cref="Map"/> at a relative position above or below a given coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="z">The z-coordinate.</param>
        /// <param name="relZ">The number of <see cref="Layer"/>s above the given position for the desired <see cref="RoomNode"/>.</param>
        /// <returns>Returns the desired <see cref="RoomNode"/>, if it exists.</returns>
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

        /// <summary>
        /// Indexer for <see cref="RoomNode"/>s on the <see cref="Map"/>. 
        /// </summary>
        /// <param name="position">The <see cref="Map"/> coordinates of the <see cref="RoomNode"/>.</param>
        /// <returns>Returns the <see cref="RoomNode"/> at the given <see cref="Map"/> coordinates.</returns>
        public RoomNode this[Vector3Int position] => Instance[position.x,position.y, position.z];

        /// <summary>
        /// Indexer for <see cref="RoomNode"/>s on the <see cref="Map"/> at a relative position above or below a given coordinates.
        /// </summary>
        /// <param name="position">The <see cref="Map"/> coordinates.</param>
        /// <param name="relZ">The number of <see cref="Layer"/>s above the given position for the desired <see cref="RoomNode"/>.</param>
        /// <returns>Returns the desired <see cref="RoomNode"/>, if it exists.</returns>
        public RoomNode this[Vector3Int position, int relZ] => Instance[position.x, position.y, position.z, relZ];

        /// <summary>
        /// Indexer for <see cref="Layer"/>s on the <see cref="Map"/>.
        /// </summary>
        /// <param name="z">The z coordinate of the <see cref="Layer"/>.</param>
        /// <returns>Returns the <see cref="Layer"/> at the given elevation.</returns>
        public Layer this[int z]
        {
            get
            {
                foreach (Layer layer in _layers)
                {
                    if (layer == null)
                        return null;
                    if (z < layer.Height)
                        return layer;

                    z -= layer.Height;
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

        /// <summary>
        /// Indexer for <see cref="Layer"/>s on the <see cref="Map"/>.
        /// </summary>
        /// <param name="z">The z coordinate of the <see cref="Layer"/>.</param>
        /// <param name="relZ">The number of <see cref="Layer"/>s above the given z coordinate for the desired <see cref="Layer"/>.</param>
        /// <returns>Returns the <see cref="Layer"/> at the given elevation.</returns>
        public Layer this[int z, int relZ]
        {
            get
            {
                for (var i = 0; i < _layers.Length; i++)
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
                for (var i = 0; i < _layers.Length; i++)
                {
                    if (z < _layers[i].Height)
                    {
                        if (i + relZ < 0 || i + relZ >= _layers.Length)
                            throw new ArgumentException();
                        else
                            _layers[i + relZ] = value;
                        return;
                    }

                    z -= _layers[i].Height;
                }
            }
        }

        /// <summary>
        /// Estimates the distance between two <see cref="RoomNode"/>s using an admissible heuristic.
        /// The heuristic is based on the Manhattan distance, but allows for diagonal movement at 3 different angles.
        /// </summary>
        /// <param name="start">The starting <see cref="RoomNode"/>.</param>
        /// <param name="end">The ending <see cref="RoomNode"/>.</param>
        /// <returns>Returns the estimated path length from <paramref name="start"/> to <paramref name="end"/>.</returns>
        public static float EstimateDistance(IWorldPosition start, IWorldPosition end)
        {
            return Vector3Int.Distance(start.WorldPosition, end.WorldPosition);
            /*int xDiff = Mathf.Abs(start.WorldPosition.x - end.WorldPosition.x);
            int yDiff = Mathf.Abs(start.WorldPosition.y - end.WorldPosition.y);
            int zDiff = Mathf.Abs(start.WorldPosition.z - end.WorldPosition.z);
            return (xDiff, yDiff) switch
            {
                _ when xDiff > 2 * yDiff => xDiff + yDiff * (Utility.Utility.RAD5 - 1),                                                                         //Equal to yDiff * RAD5 + (xDiff - yDiff);
                _ when xDiff > yDiff => xDiff * (Utility.Utility.RAD5 - Utility.Utility.RAD2) + yDiff * (2 * Utility.Utility.RAD2 - Utility.Utility.RAD5),
                _ when 2 * xDiff > yDiff => xDiff * (2 * Utility.Utility.RAD2 - Utility.Utility.RAD5) + yDiff * (Utility.Utility.RAD5 - Utility.Utility.RAD2),
                _ => xDiff * (Utility.Utility.RAD5 - 1) + yDiff,
            } + zDiff;*/
        }

        //Rooms are no longer tracked by the map, as that was never really used. Currently just sets up the RoomNodes that can be above the Room, which should be changed later.
        /// <summary>
        /// Adds a new <see cref="Room"/> to the list of <see cref="Room"/>s on the <see cref="Map"/>.
        /// </summary>
        /// <param name="room">The <see cref="Room"/> being added.</param>
        public void AddRooms(Room room)
        {
            int roomZ = room.Origin.z;
            Layer roomLayer = Instance[roomZ];
            int z = roomLayer.Origin.z + roomLayer.Height;

            Instance[roomZ, 1] ??= new Layer(MapWidth, MapLength, new Vector3Int(0, 0, z), roomLayer.LayerID + 1);

            foreach (RoomNode node in room.GetNodes)
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

        /// <summary>
        /// Calculates the approximate distance between two positions based on the network of interconnected <see cref="Room"/>s and <see cref="ConnectingNode"/>s.
        /// </summary>
        /// <param name="startPosition">The starting position in <see cref="Map"/> coordinates.</param>
        /// <param name="endPosition">The ending position in <see cref="Map"/> coordinates.</param>
        /// <returns>Returns the estimated distance between two points.</returns>
        public float ApproximateDistance(Vector3Int startPosition, Vector3Int endPosition)
        {
            RoomNode end = Instance[endPosition];

            if (!Sector.SameSector(Instance[startPosition], end))
                return float.PositiveInfinity;

            Room startingRoom = Instance[startPosition].Room;
            Room endingRoom = end.Room;

        Dictionary<INode, (float score, IReference queueNode)> gScore = new();

        PriorityQueue<INode, float> nodeQueue = new(PriorityQueue<INode, float>.MinComparer.Instance);

            if (startingRoom == endingRoom)
            {
                float score = Vector3Int.Distance(startPosition, endPosition);
                gScore[end] = (score, nodeQueue.Push(end, score));
            }

            foreach (ConnectingNode node in startingRoom.Connections)
            {
                float score = Vector3Int.Distance(startPosition, node.WorldPosition);
                gScore[node] = (score, nodeQueue.Push(node, score + Vector3Int.Distance(node.WorldPosition, endPosition)));
            }

            int loopCount = 0;

            while (!nodeQueue.Empty && loopCount++ < 1000)
            {
                INode currentNode = nodeQueue.Pop();
                if (currentNode == end)
                {
                    return gScore[end].score;
                }

                if (!currentNode.Traversable)
                    continue;
                var current = currentNode as ConnectingNode;

                float currentScore = gScore[current!].score;

                if (current.AdjacentToRoom(endingRoom))
                {
                    float nextScore = currentScore + Vector3Int.Distance(current.WorldPosition, endPosition);
                    if (gScore.TryGetValue(end, out var score))
                    {
                        if (score.score < nextScore)
                            continue;
                        else
                        {
                            gScore[end] = (nextScore, score.queueNode);
                            nodeQueue.ChangePriority(score.queueNode, nextScore);
                        }
                    }
                    else
                        gScore[end] = (nextScore, nodeQueue.Push(end, nextScore));
                }

                foreach (ConnectingNode next in current.FirstNode.Room.Connections)
                {
                    if (next == current) continue;

                    float nextScore = current.FirstNode.Room.GetDistance(current.FirstNode, next) + currentScore;
                    if (gScore.TryGetValue(next, out (float score, IReference queueNode) score))
                    {
                        if (score.score <= nextScore) continue;

                        gScore[end] = (nextScore, score.queueNode);
                        nodeQueue.ChangePriority(score.queueNode, nextScore + Vector3.Distance(endPosition, next.WorldPosition));
                        
                    }
                    else
                        gScore[next] = (nextScore, nodeQueue.Push(next, nextScore + Vector3.Distance(endPosition, next.WorldPosition)));
                }

                if (!current.IsWithinSingleRoom)
                {
                    foreach (ConnectingNode next in current.SecondNode.Room.Connections)
                    {
                        if (next == current) continue;

                        float nextScore = current.SecondNode.Room.GetDistance(current.SecondNode, next) + currentScore;
                        if (gScore.TryGetValue(next, out (float score, IReference queueNode) score))
                        {
                            if (score.score < nextScore) continue;

                            gScore[end] = (nextScore, score.queueNode);
                            nodeQueue.ChangePriority(score.queueNode,
                                nextScore + Vector3.Distance(endPosition, next.WorldPosition));

                        }
                        else
                            gScore[next] = (nextScore,
                                nodeQueue.Push(next, nextScore + Vector3.Distance(endPosition, next.WorldPosition)));
                    }
                }
            }

            return float.PositiveInfinity;
        }

        /// <summary>
        /// Determines if a door can be placed at a given position.
        /// </summary>
        /// <param name="position">The <see cref="Map"/> position of the door.</param>
        /// <param name="alignment">The <see cref="MapAlignment"/> of the door.</param>
        /// <returns>Returns true if a door can be placed at the given coordinates.</returns>
        public bool CanPlaceDoor(Vector3Int position, MapAlignment alignment)
        {
            if (WithinConstraints(position, alignment))
            {
                if (alignment == MapAlignment.XEdge)
                {
                    if (Instance[position + Vector3Int.down] == null || Instance[position] == null)
                        return false;
                }
                else
                {
                    if (Instance[position + Vector3Int.left] == null || Instance[position] == null)
                        return false;
                }


                for (int i = -1; i <= 1; i++)
                {
                    //Checks that the wall location must not be a door or null.
                    if (alignment == MapAlignment.XEdge ?
                            (GetWall(alignment, position + i * Vector3Int.right)?.WallSprite.IsDoor ?? true) :
                            (GetWall(alignment, position + i * Vector3Int.up)?.WallSprite.IsDoor ?? true))
                        return false;
                }

                for (int i = 0; i <= 1; i++)
                {
                    int xVal = position.x + (alignment == MapAlignment.XEdge ? i : 0);
                    int yVal = position.y + (alignment == MapAlignment.YEdge ? i : 0);
                    if (Graphics.Instance.IsCorner(new Vector3Int(xVal, yVal, position.z)))
                        return false;
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if a <see cref="SpriteObject"/> can be placed at the given position.
        /// </summary>
        /// <param name="position">The <see cref="Map"/> position of the <see cref="SpriteObject"/>. </param>
        /// <param name="dimensions">The 3D dimensions of the <see cref="SpriteObject"/>.</param>
        /// <returns>Returns true if the <see cref="SpriteObject"/> can be placed.</returns>
        public bool CanPlaceObject(Vector3Int position, Vector3Int dimensions)
        {
            if (!WithinConstraints(position, MapAlignment.Center))
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

        /// <summary>
        /// Determines if a <see cref="WallSprite"/> can be placed at a given position.
        /// </summary>
        /// <param name="position">The <see cref="Map"/> position of the <see cref="WallSprite"/>.</param>
        /// <param name="alignment">The <see cref="MapAlignment"/> of the <see cref="WallSprite"/>.</param>
        /// <returns>Returns true if a <see cref="WallSprite"/> can be placed at the given coordinates.</returns>
        public bool CanPlaceWall(Vector3Int position, MapAlignment alignment)
        {
            if (WithinConstraints(position, alignment) && Instance.IsSupported(position, alignment))
            {
                if (GetWall(alignment, position) == null)
                {
                    for (int i = -2; i <= 3; i++)
                    {
                        if (alignment == MapAlignment.XEdge ?
                                (GetWall(MapAlignment.YEdge, position + i * Vector3Int.right)?.WallSprite.IsDoor ?? false) &&
                                (GetWall(MapAlignment.YEdge, position + i * Vector3Int.right + Vector3Int.down)?.WallSprite.IsDoor ?? false)
                                :
                                (GetWall(MapAlignment.XEdge, position + i * Vector3Int.up)?.WallSprite.IsDoor ?? false) &&
                                (GetWall(MapAlignment.XEdge, position + i * Vector3Int.up + Vector3Int.left)?.WallSprite.IsDoor ?? false))
                            return false;
                    }
                    for (int i = -2; i <= 2; i++)
                    {
                        if (alignment == MapAlignment.XEdge ?
                                (GetWall(alignment, position + i * Vector3Int.up) != null) :
                                (GetWall(alignment, position + i * Vector3Int.right) != null))
                            return false;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds the <see cref="ConnectingNode"/> at the specified location.
        /// </summary>
        /// <param name="position">The <see cref="Map"/> coordinates of the <see cref="ConnectingNode"/>.</param>
        /// <returns>Returns the specified <see cref="ConnectingNode"/>.</returns>
        public ConnectingNode GetConnectionNode(Vector3Int position)
        {
            if (Instance[position].TryGetNodeAs(Direction.South, out ConnectingNode southNode))
                return southNode;
            if (Instance[position].TryGetNodeAs(Direction.West, out ConnectingNode westNode))
                return westNode;
            return null;
        }

        /// <summary>
        /// Finds the <see cref="WallBlocker"/> at the specified location.
        /// </summary>
        /// <param name="alignment">The <see cref="MapAlignment"/> of the <see cref="WallBlocker"/>.</param>
        /// <param name="position">The <see cref="Map"/> coordinates of the <see cref="WallBlocker"/>.</param>
        /// <returns>Returns the specified <see cref="WallBlocker"/>.</returns>
        public WallBlocker GetWall(MapAlignment alignment, Vector3Int position)
        {
            if (!WithinConstraints(position, alignment))
                return null;

            if (alignment == MapAlignment.XEdge)
            {
                return Instance[position].GetNodeAs<WallBlocker>(Direction.South);
            }
            else
            {
                return Instance[position].GetNodeAs<WallBlocker>(Direction.West);
            }
        }

        /// <summary>
        /// Evaluates if a certain position is supported by the <see cref="Layer"/> below it, and thus building features can be placed there.
        /// </summary>
        /// <param name="position">The <see cref="Map"/> coordinates being checked for support.</param>
        /// <param name="alignment">The <see cref="MapAlignment"/> of the position being checked.</param>
        /// <returns>Returns true if the position is supported by the <see cref="Layer"/> beneath it.</returns>
        public bool IsSupported(Vector3Int position, MapAlignment alignment)
        {
            if (position.z <= 0)
                return true;
            RoomNode beneath = Instance[position + Vector3Int.back];
            if (beneath == null || beneath.Room is Layer)
            {
                switch (alignment)
                {
                    case MapAlignment.XEdge:
                        beneath = Instance[position + Vector3Int.back + Vector3Int.down];
                        if (beneath == null || beneath.Room is Layer)
                        {
                            return false;
                        }
                        break;

                    case MapAlignment.YEdge:
                        beneath = Instance[position + Vector3Int.back + Vector3Int.left];
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

        /// <inheritdoc/>
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
                BuildFunctions.PlaceDoor(door.Position, door.Alignment, AccentMaterial.Stone);
            }

            foreach (SerializableStair stair in gameData.Stairs)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new StairNode(stair.Position, stair.Direction);
            }

            RoomNode.Invalid.Floor.Enabled = false;
            RoomNode.Undefined.Floor.Enabled = false;

            Graphics.Instance.SetLevel();
            BuildFunctions.Confirm();
            GameManager.MapChangingLate += BuildSectors;

            Ready = true;
        }

        /// <summary>
        /// Creates a <see cref="DoorConnector"/> at the specified position.
        /// </summary>
        /// <param name="position">The <see cref="Map"/> coordinates of the <see cref="DoorConnector"/>.</param>
        /// <param name="alignment">The <see cref="MapAlignment"/> of the <see cref="DoorConnector"/></param>
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
            // ReSharper disable once ObjectCreationAsStatement
            new DoorConnector(node1, node2, position);
        }

        /// <summary>
        /// Removes a <see cref="DoorConnector"/> at the specified position.
        /// </summary>
        /// <param name="position">The <see cref="Map"/> coordinates of the <see cref="DoorConnector"/>.</param>
        /// <param name="alignment">The <see cref="MapAlignment"/> of the <see cref="DoorConnector"/></param>
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

        /// <summary>
        /// Removes the specified <see cref="WallBlocker"/>.
        /// </summary>
        /// <param name="wall">The <see cref="WallBlocker"/> being removed.</param>
        public void RemoveWall(WallBlocker wall)
        {
            wall.RemoveWall();

            Room roomA = wall.FirstNode.Room;
            Room roomB = wall.SecondNode.Room;
            if (roomA != roomB)
            {
                if (roomA.Length * roomA.Width < roomB.Length * roomB.Width)
                {
                    (roomB, roomA) = (roomA, roomB);
                }

                roomA.EnvelopRoom(roomB);
            }
        }

        /// <inheritdoc/>
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
                            mapData[i * MapLength * MapWidth + j * MapLength + k] = new SerializableNode(Instance[j, k, 0, i], out bool checkSouth, out bool checkWest);

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

            Array.Resize(ref mapData, arrayLength);

            gameData.Map = mapData;
            gameData.MapWidth = MapWidth;
            gameData.MapLength = MapLength;
            gameData.MapHeight = _layers[0].Height;
            gameData.Layers = layerNumber;
        }

        [UsedImplicitly]
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
                Destroy(this);
        }

        /// <summary>
        /// Divides the <see cref="RoomNode"/>s on the <see cref="Map"/> into <see cref="Sector"/>s, and reserves <see cref="RoomNode"/> that are bottlenecks.
        /// </summary>
        private void BuildSectors()
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

        /// <summary>
        /// Determines if the position is within the bounds of the <see cref="Map"/>.
        /// </summary>
        /// <param name="position">The <see cref="Map"/> coordinates being evaluated.</param>
        /// <param name="alignment">The <see cref="MapAlignment"/> of the position.</param>
        /// <returns>Returns true if the position is within the <see cref="Map"/>.</returns>
        private bool WithinConstraints(Vector3Int position, MapAlignment alignment)
        {
            if (position is { x: > 0, y: > 0 })

                return alignment switch
                {
                    MapAlignment.XEdge => position.x < MapWidth && position.y < MapLength - 1,
                    MapAlignment.YEdge => position.x < MapWidth - 1 && position.y < MapLength,
                    _ => position.x < MapWidth - 1 && position.y < MapLength - 1,
                };
            else
            {
                return false;
            }
        }
    }
}