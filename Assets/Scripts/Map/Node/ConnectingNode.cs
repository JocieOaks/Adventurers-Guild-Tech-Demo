using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Assets.Scripts.Map.Node
{
    /// <summary>
    /// Class <see cref="ConnectingNode"/> is a <see cref="IDividerNode"/> that connects two adjoining <see cref="Room"/>s.
    /// </summary>
    public abstract class ConnectingNode : IDividerNode
    {
        /// <summary>
        /// Initializes a new reference of the <see cref="ConnectingNode"/> class.
        /// </summary>
        /// <param name="connection1">The first of two <see cref="RoomNode"/> that is adjacent to the <see cref="ConnectingNode"/></param>
        /// <param name="connection2">The second of two <see cref="RoomNode"/> that is adjacent to the <see cref="ConnectingNode"/></param>
        /// <param name="worldPosition">The <see cref="IWorldPosition.WorldPosition"/> of the <see cref="ConnectingNode"/>.</param>
        protected ConnectingNode(RoomNode connection1, RoomNode connection2, Vector3Int worldPosition)
        {
            FirstNode = connection1;
            SecondNode = connection2;

            Alignment = connection1.WorldPosition.x == connection2.WorldPosition.x
                ? MapAlignment.XEdge
                : MapAlignment.YEdge;

            WorldPosition = worldPosition;

            GameManager.MapChangingLate += RegisterRooms;
        }

        /// <inheritdoc/>
        public IEnumerable<INode> AdjacentNodes
        {
            get
            {
                yield return FirstNode;
                yield return SecondNode;
            }
        }

        /// <inheritdoc/>
        public MapAlignment Alignment { get; }

        /// <inheritdoc/>
        public RoomNode FirstNode { get; }

        /// <value>Returns true if both <see cref="RoomNode"/>'s bordering the <see cref="ConnectingNode"/> are within the same <see cref="Scripts.Map.Room"/>.</value>
        public bool IsWithinSingleRoom => (FirstNode.Room == SecondNode.Room);

        /// <inheritdoc/>
        public Vector3Int NearestCornerPosition => WorldPosition;

        /// <inheritdoc/>
        public RoomNode Node => FirstNode;

        /// <inheritdoc/>
        public Room Room
        {
            get
            {
                if (IsWithinSingleRoom)
                    return FirstNode.Room;
                return null;
            }
        }

        /// <inheritdoc/>
        public RoomNode SecondNode { get; }

        /// <inheritdoc/>
        public abstract bool Obstructed { get; }

        /// <inheritdoc/>
        public bool AdjacentToRoomNode(RoomNode node)
        {
            return node == FirstNode || node == SecondNode;
        }

        /// <inheritdoc/>
        public Vector3Int WorldPosition { get; protected set; }

        /// <inheritdoc/>
        public abstract Vector3Int Dimensions { get; }

        /// <inheritdoc/>
        public bool AdjacentToRoom(Room room)
        {
            return FirstNode.Room == room || SecondNode.Room == room;
        }

        /// <summary>
        /// Called when the <see cref="ConnectingNode"/> is removed from connecting two <see cref="RoomNode"/>s.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Gives the room that is connected to a given room by the <see cref="ConnectingNode"/>.
        /// </summary>
        /// <param name="startingRoom">The initial room that is connected to the <see cref="ConnectingNode"/>.</param>
        /// <returns>Returns the other <see cref="Room"/> adjacent to the <see cref="ConnectingNode"/>.</returns>
        /// <exception cref="System.ArgumentException">Throws this exception if startingRoom is not connected to the <see cref="ConnectingNode"/>.</exception>
        [UsedImplicitly]
        public Room GetConnectedRoom(Room startingRoom)
        {
            if (FirstNode.Room == startingRoom)
                return SecondNode.Room;
            if (SecondNode.Room == startingRoom)
                return FirstNode.Room;

            throw new System.ArgumentException();
        }

        /// <summary>
        /// Determines which of the two <see cref="Scripts.Map.Room"/>s connected to this <see cref="ConnectingNode"/> is shared by the given <see cref="INode"/>.
        /// </summary>
        /// <param name="node">The <see cref="INode"/> that may share a <see cref="Scripts.Map.Room"/> with the <see cref="ConnectingNode"/>.</param>
        /// <returns>Returns the <see cref="Scripts.Map.Room"/> shared with <paramref name="node"/>. Returns null if there is no shared room.</returns>
        public Room GetCommonRoom(INode node)
        {
            if (node is ConnectingNode connection)
            {
                if (connection.FirstNode.Room == FirstNode.Room || connection.FirstNode.Room == SecondNode.Room)
                    return connection.FirstNode.Room;
                if (connection.SecondNode.Room == FirstNode.Room || connection.SecondNode.Room == SecondNode.Room)
                    return connection.SecondNode.Room;
            }
            else
            {
                if(node.Room == FirstNode.Room || node.Room == SecondNode.Room)
                    return node.Room;
            }
            return null;
        }

        /// <inheritdoc/>
        public virtual RoomNode GetOppositeRoomNode(RoomNode entrance)
        {
            if (entrance == FirstNode)
                return SecondNode;
            else if (entrance == SecondNode)
                return FirstNode;
            else
            {
                throw new System.ArgumentException();
            }
        }

        /// <summary>
        /// Gives the <see cref="RoomNode"/> adjacent to the <see cref="ConnectingNode"/> in the given <see cref="Room"/>.
        /// </summary>
        /// <param name="room">The <see cref="Room"/> where the <see cref="RoomNode"/> should be found.</param>
        /// <returns>Returns the <see cref="RoomNode"/> adjacent to the <see cref="ConnectingNode"/> in room.</returns>
        /// <exception cref="System.ArgumentException">Throws exception if the <see cref="ConnectingNode"/> is not adjacent to the given <see cref="Room"/>.</exception>
        public RoomNode GetRoomNode(Room room)
        {
            if (FirstNode.Room == room) 
                return FirstNode;
            
            if (SecondNode.Room == room)
                return SecondNode;
            
            throw new System.ArgumentException();
        }

        /// <inheritdoc/>
        public bool HasNavigatedTo(RoomNode node)
        {
            return node == FirstNode || node == SecondNode;
        }

        /// <summary>
        /// Adds this <see cref="ConnectingNode"/> to the <see cref="Scripts.Map.Room.Connections"/> of it's adjacent <see cref="Scripts.Map.Room"/>s.
        /// </summary>
        public virtual void RegisterRooms()
        {
            FirstNode.Room.AddConnection(this);
            if (SecondNode.Room != FirstNode.Room)
                SecondNode.Room.AddConnection(this);

            GameManager.MapChangingLate -= RegisterRooms;
        }
    }
}